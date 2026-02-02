using ChroniclesOfFate.Core.Entities;
using ChroniclesOfFate.Core.Enums;

namespace ChroniclesOfFate.Core.Interfaces;

/// <summary>
/// Generic repository interface
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

/// <summary>
/// User repository for authentication
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
}

/// <summary>
/// Game session repository
/// </summary>
public interface IGameSessionRepository : IRepository<GameSession>
{
    Task<IEnumerable<GameSession>> GetByUserIdAsync(string userId);
    Task<GameSession?> GetWithCharacterAsync(int sessionId);
    Task<GameSession?> GetFullSessionAsync(int sessionId);
}

/// <summary>
/// Character repository
/// </summary>
public interface ICharacterRepository : IRepository<Character>
{
    Task<Character?> GetWithStorybooksAsync(int characterId);
    Task<Character?> GetWithFullDetailsAsync(int characterId);
    Task<IEnumerable<Character>> GetTopCharactersByPowerAsync(int count);
}

/// <summary>
/// Storybook repository
/// </summary>
public interface IStorybookRepository : IRepository<Storybook>
{
    Task<IEnumerable<Storybook>> GetByRarityAsync(Rarity rarity);
    Task<IEnumerable<Storybook>> GetWithEventsAsync();
    Task<Storybook?> GetWithEventsAsync(int storybookId);
    Task<IEnumerable<Storybook>> GetUnlockedForSessionAsync(int sessionId);
}

/// <summary>
/// Random event repository
/// </summary>
public interface IRandomEventRepository : IRepository<RandomEvent>
{
    Task<IEnumerable<RandomEvent>> GetGlobalEventsAsync();
    Task<IEnumerable<RandomEvent>> GetByStorybookIdAsync(int storybookId);
    Task<IEnumerable<RandomEvent>> GetEligibleEventsAsync(
        ActionType action, 
        Season season, 
        IEnumerable<int> storybookIds);
    Task<RandomEvent?> GetWithChoicesAsync(int eventId);
}

/// <summary>
/// Event choice repository
/// </summary>
public interface IEventChoiceRepository : IRepository<EventChoice>
{
    Task<IEnumerable<EventChoice>> GetByEventIdAsync(int eventId);
}

/// <summary>
/// Training scenario repository
/// </summary>
public interface ITrainingScenarioRepository : IRepository<TrainingScenario>
{
    Task<IEnumerable<TrainingScenario>> GetByStatAsync(StatType stat);
    Task<IEnumerable<TrainingScenario>> GetAvailableForLevelAsync(int level);
    Task<IEnumerable<TrainingScenario>> GetWithSeasonalBonusAsync(Season season);
}

/// <summary>
/// Enemy repository
/// </summary>
public interface IEnemyRepository : IRepository<Enemy>
{
    Task<IEnumerable<Enemy>> GetByDifficultyTierAsync(int tier);
    Task<IEnumerable<Enemy>> GetEligibleEnemiesAsync(int characterLevel, Season season);
    Task<Enemy?> GetRandomEnemyAsync(int minTier, int maxTier);
}

/// <summary>
/// Battle log repository
/// </summary>
public interface IBattleLogRepository : IRepository<BattleLog>
{
    Task<IEnumerable<BattleLog>> GetByCharacterIdAsync(int characterId);
    Task<int> GetBattleCountAsync(int characterId);
    Task<int> GetVictoryCountAsync(int characterId);
}

/// <summary>
/// Game event history repository
/// </summary>
public interface IGameEventRepository : IRepository<GameEvent>
{
    Task<IEnumerable<GameEvent>> GetByCharacterIdAsync(int characterId);
    Task<IEnumerable<GameEvent>> GetRecentEventsAsync(int characterId, int count);
    Task<bool> HasEventOccurredAsync(int characterId, int eventId);
}

/// <summary>
/// Unit of Work pattern for transaction management
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IGameSessionRepository GameSessions { get; }
    ICharacterRepository Characters { get; }
    IStorybookRepository Storybooks { get; }
    IRandomEventRepository RandomEvents { get; }
    IEventChoiceRepository EventChoices { get; }
    ITrainingScenarioRepository TrainingScenarios { get; }
    IEnemyRepository Enemies { get; }
    IBattleLogRepository BattleLogs { get; }
    IGameEventRepository GameEvents { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
