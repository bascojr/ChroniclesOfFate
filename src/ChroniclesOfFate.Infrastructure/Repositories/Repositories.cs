using ChroniclesOfFate.Core.Entities;
using ChroniclesOfFate.Core.Enums;
using ChroniclesOfFate.Core.Interfaces;
using ChroniclesOfFate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChroniclesOfFate.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public virtual async Task<T> AddAsync(T entity) { await _dbSet.AddAsync(entity); return entity; }
    public virtual Task UpdateAsync(T entity) { _dbSet.Update(entity); return Task.CompletedTask; }
    public virtual Task DeleteAsync(T entity) { _dbSet.Remove(entity); return Task.CompletedTask; }
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }
    public async Task<User?> GetByUsernameAsync(string username) => await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    public async Task<User?> GetByEmailAsync(string email) => await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    public async Task<User?> GetByRefreshTokenAsync(string refreshToken) => await _dbSet.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    public async Task<bool> UsernameExistsAsync(string username) => await _dbSet.AnyAsync(u => u.Username == username);
    public async Task<bool> EmailExistsAsync(string email) => await _dbSet.AnyAsync(u => u.Email == email);
}

public class GameSessionRepository : Repository<GameSession>, IGameSessionRepository
{
    public GameSessionRepository(ApplicationDbContext context) : base(context) { }
    
    public async Task<IEnumerable<GameSession>> GetByUserIdAsync(string userId) =>
        await _dbSet.Where(s => s.UserId == userId).Include(s => s.Character).ToListAsync();
    
    public async Task<GameSession?> GetWithCharacterAsync(int sessionId) =>
        await _dbSet.Include(s => s.Character).ThenInclude(c => c!.EquippedStorybooks).ThenInclude(es => es.Storybook)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    
    public async Task<GameSession?> GetFullSessionAsync(int sessionId) =>
        await _dbSet.Include(s => s.Character).ThenInclude(c => c!.EquippedStorybooks).ThenInclude(es => es.Storybook)
            .Include(s => s.UnlockedStorybooks).FirstOrDefaultAsync(s => s.Id == sessionId);
}

public class CharacterRepository : Repository<Character>, ICharacterRepository
{
    public CharacterRepository(ApplicationDbContext context) : base(context) { }
    
    public async Task<Character?> GetWithStorybooksAsync(int characterId) =>
        await _dbSet.Include(c => c.EquippedStorybooks).ThenInclude(es => es.Storybook)
            .Include(c => c.Skills).ThenInclude(cs => cs.Skill)
            .FirstOrDefaultAsync(c => c.Id == characterId);
    
    public async Task<Character?> GetWithFullDetailsAsync(int characterId) =>
        await _dbSet.Include(c => c.EquippedStorybooks).ThenInclude(es => es.Storybook)
            .Include(c => c.Skills).ThenInclude(cs => cs.Skill)
            .Include(c => c.EventHistory).Include(c => c.BattleHistory)
            .FirstOrDefaultAsync(c => c.Id == characterId);
    
    public async Task<IEnumerable<Character>> GetTopCharactersByPowerAsync(int count) =>
        await _dbSet.OrderByDescending(c => c.Strength + c.Agility + c.Intelligence + c.Endurance + c.Charisma + c.Luck)
            .Take(count).ToListAsync();
}

public class StorybookRepository : Repository<Storybook>, IStorybookRepository
{
    public StorybookRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Storybook>> GetWithEventsAsync() =>
        await _dbSet.Include(s => s.Events).ToListAsync();
    
    public async Task<Storybook?> GetWithEventsAsync(int storybookId) =>
        await _dbSet.Include(s => s.Events).ThenInclude(e => e.Choices)
            .FirstOrDefaultAsync(s => s.Id == storybookId);
    
    public async Task<IEnumerable<Storybook>> GetUnlockedForSessionAsync(int sessionId) =>
        await _context.GameSessions.Where(gs => gs.Id == sessionId)
            .SelectMany(gs => gs.UnlockedStorybooks).ToListAsync();
}

public class RandomEventRepository : Repository<RandomEvent>, IRandomEventRepository
{
    public RandomEventRepository(ApplicationDbContext context) : base(context) { }
    
    public async Task<IEnumerable<RandomEvent>> GetGlobalEventsAsync() =>
        await _dbSet.Where(e => e.StorybookId == null && e.IsActive).Include(e => e.Choices).ToListAsync();
    
    public async Task<IEnumerable<RandomEvent>> GetByStorybookIdAsync(int storybookId) =>
        await _dbSet.Where(e => e.StorybookId == storybookId && e.IsActive).Include(e => e.Choices).ToListAsync();
    
    public async Task<IEnumerable<RandomEvent>> GetEligibleEventsAsync(ActionType action, Season season, IEnumerable<int> storybookIds)
    {
        var actionStr = action.ToString();
        var seasonStr = season.ToString();
        var sbIds = storybookIds.ToList();
        
        return await _dbSet.Where(e => e.IsActive &&
            (e.StorybookId == null || sbIds.Contains(e.StorybookId.Value)) &&
            e.TriggerActions.Contains(actionStr) &&
            (e.TriggerSeasons == null || e.TriggerSeasons.Contains(seasonStr)))
            .Include(e => e.Choices).Include(e => e.Storybook).ToListAsync();
    }
    
    public async Task<RandomEvent?> GetWithChoicesAsync(int eventId) =>
        await _dbSet
            .Include(e => e.Choices).ThenInclude(c => c.GrantSkill)
            .Include(e => e.Choices).ThenInclude(c => c.FailureGrantSkill)
            .Include(e => e.Storybook)
            .FirstOrDefaultAsync(e => e.Id == eventId);
}

public class EventChoiceRepository : Repository<EventChoice>, IEventChoiceRepository
{
    public EventChoiceRepository(ApplicationDbContext context) : base(context) { }
    public async Task<IEnumerable<EventChoice>> GetByEventIdAsync(int eventId) =>
        await _dbSet.Where(c => c.RandomEventId == eventId).OrderBy(c => c.DisplayOrder).ToListAsync();
}

public class TrainingScenarioRepository : Repository<TrainingScenario>, ITrainingScenarioRepository
{
    public TrainingScenarioRepository(ApplicationDbContext context) : base(context) { }
    
    public async Task<IEnumerable<TrainingScenario>> GetByStatAsync(StatType stat) =>
        await _dbSet.Where(t => t.PrimaryStat == stat && t.IsActive).ToListAsync();
    
    public async Task<IEnumerable<TrainingScenario>> GetAvailableForLevelAsync(int level) =>
        await _dbSet.Where(t => t.RequiredLevel <= level && t.IsActive).ToListAsync();
    
    public async Task<IEnumerable<TrainingScenario>> GetWithSeasonalBonusAsync(Season season)
    {
        var seasonStr = season.ToString();
        return await _dbSet.Where(t => t.IsActive && t.BonusSeasons != null && t.BonusSeasons.Contains(seasonStr)).ToListAsync();
    }
}

public class EnemyRepository : Repository<Enemy>, IEnemyRepository
{
    private static readonly Random _random = new();
    public EnemyRepository(ApplicationDbContext context) : base(context) { }
    
    public async Task<IEnumerable<Enemy>> GetByDifficultyTierAsync(int tier) =>
        await _dbSet.Where(e => e.DifficultyTier == tier && e.IsActive).ToListAsync();
    
    public async Task<IEnumerable<Enemy>> GetEligibleEnemiesAsync(int characterLevel, Season season)
    {
        int minTier = Math.Max(1, characterLevel - 2);
        int maxTier = characterLevel + 3;
        var seasonStr = season.ToString();
        
        return await _dbSet.Where(e => e.IsActive && e.DifficultyTier >= minTier && e.DifficultyTier <= maxTier &&
            (e.ActiveSeasons == null || e.ActiveSeasons.Contains(seasonStr))).ToListAsync();
    }
    
    public async Task<Enemy?> GetRandomEnemyAsync(int minTier, int maxTier)
    {
        var enemies = await _dbSet.Where(e => e.IsActive && e.DifficultyTier >= minTier && e.DifficultyTier <= maxTier).ToListAsync();
        return enemies.Count > 0 ? enemies[_random.Next(enemies.Count)] : null;
    }
}

public class BattleLogRepository : Repository<BattleLog>, IBattleLogRepository
{
    public BattleLogRepository(ApplicationDbContext context) : base(context) { }
    
    public async Task<IEnumerable<BattleLog>> GetByCharacterIdAsync(int characterId) =>
        await _dbSet.Where(b => b.CharacterId == characterId).Include(b => b.Enemy)
            .OrderByDescending(b => b.OccurredAt).ToListAsync();
    
    public async Task<int> GetBattleCountAsync(int characterId) =>
        await _dbSet.CountAsync(b => b.CharacterId == characterId);
    
    public async Task<int> GetVictoryCountAsync(int characterId) =>
        await _dbSet.CountAsync(b => b.CharacterId == characterId && b.Result == BattleResult.Victory);
}

public class GameEventRepository : Repository<GameEvent>, IGameEventRepository
{
    public GameEventRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<GameEvent>> GetByCharacterIdAsync(int characterId) =>
        await _dbSet.Where(e => e.CharacterId == characterId).Include(e => e.RandomEvent)
            .OrderByDescending(e => e.OccurredAt).ToListAsync();

    public async Task<IEnumerable<GameEvent>> GetRecentEventsAsync(int characterId, int count) =>
        await _dbSet.Where(e => e.CharacterId == characterId).Include(e => e.RandomEvent)
            .OrderByDescending(e => e.OccurredAt).Take(count).ToListAsync();

    public async Task<bool> HasEventOccurredAsync(int characterId, int eventId) =>
        await _dbSet.AnyAsync(e => e.CharacterId == characterId && e.RandomEventId == eventId);
}

public class MessageLogEntryRepository : Repository<MessageLogEntry>, IMessageLogEntryRepository
{
    public MessageLogEntryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<MessageLogEntry>> GetBySessionAsync(int sessionId, int limit = 20) =>
        await _dbSet.Where(m => m.GameSessionId == sessionId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync();
}

public class SkillRepository : Repository<Skill>, ISkillRepository
{
    public SkillRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Skill>> GetByTypeAsync(SkillType skillType) =>
        await _dbSet.Where(s => s.SkillType == skillType && s.IsActive).ToListAsync();

    public async Task<IEnumerable<Skill>> GetActiveSkillsAsync() =>
        await _dbSet.Where(s => s.IsActive).ToListAsync();

    public async Task<Skill?> GetWithDetailsAsync(int skillId) =>
        await _dbSet.FirstOrDefaultAsync(s => s.Id == skillId);
}

public class CharacterSkillRepository : Repository<CharacterSkill>, ICharacterSkillRepository
{
    public CharacterSkillRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<CharacterSkill>> GetByCharacterIdAsync(int characterId) =>
        await _dbSet.Where(cs => cs.CharacterId == characterId).ToListAsync();

    public async Task<IEnumerable<CharacterSkill>> GetByCharacterWithSkillsAsync(int characterId) =>
        await _dbSet.Where(cs => cs.CharacterId == characterId)
            .Include(cs => cs.Skill)
            .ToListAsync();

    public async Task<bool> CharacterHasSkillAsync(int characterId, int skillId) =>
        await _dbSet.AnyAsync(cs => cs.CharacterId == characterId && cs.SkillId == skillId);

    public async Task<CharacterSkill?> AddSkillToCharacterAsync(int characterId, int skillId, string? source = null, int turn = 0)
    {
        // Check if character already has this skill
        if (await CharacterHasSkillAsync(characterId, skillId))
            return null;

        var characterSkill = new CharacterSkill
        {
            CharacterId = characterId,
            SkillId = skillId,
            AcquisitionSource = source,
            AcquiredOnTurn = turn,
            AcquiredAt = DateTime.UtcNow
        };

        await _dbSet.AddAsync(characterSkill);
        return characterSkill;
    }
}
