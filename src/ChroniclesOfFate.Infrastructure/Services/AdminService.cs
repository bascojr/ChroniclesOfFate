using ChroniclesOfFate.Core.DTOs;
using ChroniclesOfFate.Core.Entities;
using ChroniclesOfFate.Core.Interfaces;
using ChroniclesOfFate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChroniclesOfFate.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;

    public AdminService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============ Dashboard ============

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        return new AdminDashboardDto(
            await _context.TrainingScenarios.CountAsync(),
            await _context.Enemies.CountAsync(),
            await _context.Storybooks.CountAsync(),
            await _context.RandomEvents.CountAsync(),
            await _context.EventChoices.CountAsync(),
            await _context.Skills.CountAsync(),
            await _context.Users.CountAsync(),
            await _context.GameSessions.CountAsync()
        );
    }

    // ============ Lookups ============

    public async Task<IEnumerable<LookupDto>> GetStorybookLookupAsync()
    {
        return await _context.Storybooks
            .Select(s => new LookupDto(s.Id, s.Name))
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupDto>> GetSkillLookupAsync()
    {
        return await _context.Skills
            .Where(s => s.IsActive)
            .Select(s => new LookupDto(s.Id, s.Name))
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupDto>> GetEnemyLookupAsync()
    {
        return await _context.Enemies
            .Where(e => e.IsActive)
            .Select(e => new LookupDto(e.Id, e.Name))
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupDto>> GetRandomEventLookupAsync()
    {
        return await _context.RandomEvents
            .Where(e => e.IsActive)
            .Select(e => new LookupDto(e.Id, e.Title))
            .ToListAsync();
    }

    // ============ Training Scenarios ============

    public async Task<IEnumerable<AdminTrainingScenarioDto>> GetAllTrainingScenariosAsync()
    {
        return await _context.TrainingScenarios
            .OrderBy(t => t.RequiredLevel)
            .ThenBy(t => t.PrimaryStat)
            .Select(t => MapTrainingScenarioToDto(t))
            .ToListAsync();
    }

    public async Task<AdminTrainingScenarioDto?> GetTrainingScenarioAsync(int id)
    {
        var entity = await _context.TrainingScenarios.FindAsync(id);
        return entity == null ? null : MapTrainingScenarioToDto(entity);
    }

    public async Task<AdminTrainingScenarioDto> CreateTrainingScenarioAsync(CreateTrainingScenarioDto dto)
    {
        var entity = new TrainingScenario
        {
            Name = dto.Name,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            PrimaryStat = dto.PrimaryStat,
            SecondaryStat = dto.SecondaryStat,
            TertiaryStat = dto.TertiaryStat,
            BaseStatGain = dto.BaseStatGain,
            SecondaryStatGain = dto.SecondaryStatGain,
            TertiaryStatGain = dto.TertiaryStatGain,
            EnergyCost = dto.EnergyCost,
            BonusChance = dto.BonusChance,
            BonusStatGain = dto.BonusStatGain,
            FailureChance = dto.FailureChance,
            FailureHealthPenalty = dto.FailureHealthPenalty,
            BonusSeasons = dto.BonusSeasons,
            SeasonalBonusMultiplier = dto.SeasonalBonusMultiplier,
            ExperienceGain = dto.ExperienceGain,
            RequiredLevel = dto.RequiredLevel,
            TrainingNarrative = dto.TrainingNarrative,
            IsActive = dto.IsActive
        };

        _context.TrainingScenarios.Add(entity);
        await _context.SaveChangesAsync();
        return MapTrainingScenarioToDto(entity);
    }

    public async Task<AdminTrainingScenarioDto?> UpdateTrainingScenarioAsync(int id, UpdateTrainingScenarioDto dto)
    {
        var entity = await _context.TrainingScenarios.FindAsync(id);
        if (entity == null) return null;

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.ImageUrl = dto.ImageUrl;
        entity.PrimaryStat = dto.PrimaryStat;
        entity.SecondaryStat = dto.SecondaryStat;
        entity.TertiaryStat = dto.TertiaryStat;
        entity.BaseStatGain = dto.BaseStatGain;
        entity.SecondaryStatGain = dto.SecondaryStatGain;
        entity.TertiaryStatGain = dto.TertiaryStatGain;
        entity.EnergyCost = dto.EnergyCost;
        entity.BonusChance = dto.BonusChance;
        entity.BonusStatGain = dto.BonusStatGain;
        entity.FailureChance = dto.FailureChance;
        entity.FailureHealthPenalty = dto.FailureHealthPenalty;
        entity.BonusSeasons = dto.BonusSeasons;
        entity.SeasonalBonusMultiplier = dto.SeasonalBonusMultiplier;
        entity.ExperienceGain = dto.ExperienceGain;
        entity.RequiredLevel = dto.RequiredLevel;
        entity.TrainingNarrative = dto.TrainingNarrative;
        entity.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return MapTrainingScenarioToDto(entity);
    }

    public async Task<bool> DeleteTrainingScenarioAsync(int id)
    {
        var entity = await _context.TrainingScenarios.FindAsync(id);
        if (entity == null) return false;

        _context.TrainingScenarios.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // ============ Enemies ============

    public async Task<IEnumerable<AdminEnemyDto>> GetAllEnemiesAsync()
    {
        return await _context.Enemies
            .OrderBy(e => e.DifficultyTier)
            .ThenBy(e => e.Name)
            .Select(e => MapEnemyToDto(e))
            .ToListAsync();
    }

    public async Task<AdminEnemyDto?> GetEnemyAsync(int id)
    {
        var entity = await _context.Enemies.FindAsync(id);
        return entity == null ? null : MapEnemyToDto(entity);
    }

    public async Task<AdminEnemyDto> CreateEnemyAsync(CreateEnemyDto dto)
    {
        var entity = new Enemy
        {
            Name = dto.Name,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            Strength = dto.Strength,
            Agility = dto.Agility,
            Intelligence = dto.Intelligence,
            Endurance = dto.Endurance,
            Health = dto.Health,
            DifficultyTier = dto.DifficultyTier,
            EnemyType = dto.EnemyType,
            ExperienceReward = dto.ExperienceReward,
            GoldReward = dto.GoldReward,
            ReputationReward = dto.ReputationReward,
            LootTable = dto.LootTable,
            ActiveSeasons = dto.ActiveSeasons,
            Abilities = dto.Abilities,
            IsActive = dto.IsActive
        };

        _context.Enemies.Add(entity);
        await _context.SaveChangesAsync();
        return MapEnemyToDto(entity);
    }

    public async Task<AdminEnemyDto?> UpdateEnemyAsync(int id, UpdateEnemyDto dto)
    {
        var entity = await _context.Enemies.FindAsync(id);
        if (entity == null) return null;

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.ImageUrl = dto.ImageUrl;
        entity.Strength = dto.Strength;
        entity.Agility = dto.Agility;
        entity.Intelligence = dto.Intelligence;
        entity.Endurance = dto.Endurance;
        entity.Health = dto.Health;
        entity.DifficultyTier = dto.DifficultyTier;
        entity.EnemyType = dto.EnemyType;
        entity.ExperienceReward = dto.ExperienceReward;
        entity.GoldReward = dto.GoldReward;
        entity.ReputationReward = dto.ReputationReward;
        entity.LootTable = dto.LootTable;
        entity.ActiveSeasons = dto.ActiveSeasons;
        entity.Abilities = dto.Abilities;
        entity.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return MapEnemyToDto(entity);
    }

    public async Task<bool> DeleteEnemyAsync(int id)
    {
        var entity = await _context.Enemies.FindAsync(id);
        if (entity == null) return false;

        _context.Enemies.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // ============ Storybooks ============

    public async Task<IEnumerable<AdminStorybookDto>> GetAllStorybooksAsync()
    {
        return await _context.Storybooks
            .Include(s => s.Events)
            .OrderBy(s => s.Name)
            .Select(s => MapStorybookToDto(s))
            .ToListAsync();
    }

    public async Task<AdminStorybookDto?> GetStorybookAsync(int id)
    {
        var entity = await _context.Storybooks
            .Include(s => s.Events)
            .FirstOrDefaultAsync(s => s.Id == id);
        return entity == null ? null : MapStorybookToDto(entity);
    }

    public async Task<AdminStorybookDto> CreateStorybookAsync(CreateStorybookDto dto)
    {
        var entity = new Storybook
        {
            Name = dto.Name,
            Description = dto.Description,
            IconUrl = dto.IconUrl,
            Theme = dto.Theme,
            StrengthBonus = dto.StrengthBonus,
            AgilityBonus = dto.AgilityBonus,
            IntelligenceBonus = dto.IntelligenceBonus,
            EnduranceBonus = dto.EnduranceBonus,
            CharismaBonus = dto.CharismaBonus,
            LuckBonus = dto.LuckBonus,
            EventTriggerChance = dto.EventTriggerChance,
            IsUnlockable = dto.IsUnlockable,
            UnlockCondition = dto.UnlockCondition
        };

        _context.Storybooks.Add(entity);
        await _context.SaveChangesAsync();
        return MapStorybookToDto(entity);
    }

    public async Task<AdminStorybookDto?> UpdateStorybookAsync(int id, UpdateStorybookDto dto)
    {
        var entity = await _context.Storybooks
            .Include(s => s.Events)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (entity == null) return null;

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.IconUrl = dto.IconUrl;
        entity.Theme = dto.Theme;
        entity.StrengthBonus = dto.StrengthBonus;
        entity.AgilityBonus = dto.AgilityBonus;
        entity.IntelligenceBonus = dto.IntelligenceBonus;
        entity.EnduranceBonus = dto.EnduranceBonus;
        entity.CharismaBonus = dto.CharismaBonus;
        entity.LuckBonus = dto.LuckBonus;
        entity.EventTriggerChance = dto.EventTriggerChance;
        entity.IsUnlockable = dto.IsUnlockable;
        entity.UnlockCondition = dto.UnlockCondition;

        await _context.SaveChangesAsync();
        return MapStorybookToDto(entity);
    }

    public async Task<bool> DeleteStorybookAsync(int id)
    {
        var entity = await _context.Storybooks.FindAsync(id);
        if (entity == null) return false;

        _context.Storybooks.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // ============ Random Events ============

    public async Task<IEnumerable<AdminRandomEventDto>> GetAllRandomEventsAsync()
    {
        return await _context.RandomEvents
            .Include(e => e.Storybook)
            .Include(e => e.Choices)
                .ThenInclude(c => c.GrantSkill)
            .Include(e => e.Choices)
                .ThenInclude(c => c.FailureGrantSkill)
            .OrderBy(e => e.StorybookId)
            .ThenBy(e => e.Rarity)
            .ThenBy(e => e.Title)
            .Select(e => MapRandomEventToDto(e))
            .ToListAsync();
    }

    public async Task<AdminRandomEventDto?> GetRandomEventAsync(int id)
    {
        var entity = await _context.RandomEvents
            .Include(e => e.Storybook)
            .Include(e => e.Choices)
                .ThenInclude(c => c.GrantSkill)
            .Include(e => e.Choices)
                .ThenInclude(c => c.FailureGrantSkill)
            .FirstOrDefaultAsync(e => e.Id == id);
        return entity == null ? null : MapRandomEventToDto(entity);
    }

    public async Task<AdminRandomEventDto> CreateRandomEventAsync(CreateRandomEventDto dto)
    {
        var entity = new RandomEvent
        {
            Title = dto.Title,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            StorybookId = dto.StorybookId,
            Rarity = dto.Rarity,
            OutcomeType = dto.OutcomeType,
            TriggerActions = dto.TriggerActions,
            TriggerSeasons = dto.TriggerSeasons,
            BaseProbability = dto.BaseProbability,
            StatRequirements = dto.StatRequirements,
            IsActive = dto.IsActive
        };

        _context.RandomEvents.Add(entity);
        await _context.SaveChangesAsync();

        // Reload with includes
        var created = await _context.RandomEvents
            .Include(e => e.Storybook)
            .Include(e => e.Choices)
            .FirstAsync(e => e.Id == entity.Id);

        return MapRandomEventToDto(created);
    }

    public async Task<AdminRandomEventDto?> UpdateRandomEventAsync(int id, UpdateRandomEventDto dto)
    {
        var entity = await _context.RandomEvents
            .Include(e => e.Storybook)
            .Include(e => e.Choices)
                .ThenInclude(c => c.GrantSkill)
            .Include(e => e.Choices)
                .ThenInclude(c => c.FailureGrantSkill)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null) return null;

        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.ImageUrl = dto.ImageUrl;
        entity.StorybookId = dto.StorybookId;
        entity.Rarity = dto.Rarity;
        entity.OutcomeType = dto.OutcomeType;
        entity.TriggerActions = dto.TriggerActions;
        entity.TriggerSeasons = dto.TriggerSeasons;
        entity.BaseProbability = dto.BaseProbability;
        entity.StatRequirements = dto.StatRequirements;
        entity.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return MapRandomEventToDto(entity);
    }

    public async Task<bool> DeleteRandomEventAsync(int id)
    {
        var entity = await _context.RandomEvents.FindAsync(id);
        if (entity == null) return false;

        _context.RandomEvents.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // ============ Event Choices ============

    public async Task<IEnumerable<AdminEventChoiceDto>> GetEventChoicesAsync(int eventId)
    {
        return await _context.EventChoices
            .Include(c => c.GrantSkill)
            .Include(c => c.FailureGrantSkill)
            .Where(c => c.RandomEventId == eventId)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => MapEventChoiceToDto(c))
            .ToListAsync();
    }

    public async Task<AdminEventChoiceDto?> GetEventChoiceAsync(int id)
    {
        var entity = await _context.EventChoices
            .Include(c => c.GrantSkill)
            .Include(c => c.FailureGrantSkill)
            .FirstOrDefaultAsync(c => c.Id == id);
        return entity == null ? null : MapEventChoiceToDto(entity);
    }

    public async Task<AdminEventChoiceDto> CreateEventChoiceAsync(CreateEventChoiceDto dto)
    {
        var entity = new EventChoice
        {
            RandomEventId = dto.RandomEventId,
            Text = dto.Text,
            ResultDescription = dto.ResultDescription,
            StatRequirements = dto.StatRequirements,
            CheckStat = dto.CheckStat,
            CheckDifficulty = dto.CheckDifficulty,
            StrengthChange = dto.StrengthChange,
            AgilityChange = dto.AgilityChange,
            IntelligenceChange = dto.IntelligenceChange,
            EnduranceChange = dto.EnduranceChange,
            CharismaChange = dto.CharismaChange,
            LuckChange = dto.LuckChange,
            EnergyChange = dto.EnergyChange,
            HealthChange = dto.HealthChange,
            GoldChange = dto.GoldChange,
            ReputationChange = dto.ReputationChange,
            ExperienceChange = dto.ExperienceChange,
            FailureDescription = dto.FailureDescription,
            FailureStrengthChange = dto.FailureStrengthChange,
            FailureAgilityChange = dto.FailureAgilityChange,
            FailureIntelligenceChange = dto.FailureIntelligenceChange,
            FailureEnduranceChange = dto.FailureEnduranceChange,
            FailureCharismaChange = dto.FailureCharismaChange,
            FailureLuckChange = dto.FailureLuckChange,
            FailureEnergyChange = dto.FailureEnergyChange,
            FailureHealthChange = dto.FailureHealthChange,
            FailureGoldChange = dto.FailureGoldChange,
            FailureReputationChange = dto.FailureReputationChange,
            FollowUpEventId = dto.FollowUpEventId,
            TriggerBattleId = dto.TriggerBattleId,
            GrantSkillId = dto.GrantSkillId,
            FailureGrantSkillId = dto.FailureGrantSkillId,
            DisplayOrder = dto.DisplayOrder
        };

        _context.EventChoices.Add(entity);
        await _context.SaveChangesAsync();

        // Reload with includes
        var created = await _context.EventChoices
            .Include(c => c.GrantSkill)
            .Include(c => c.FailureGrantSkill)
            .FirstAsync(c => c.Id == entity.Id);

        return MapEventChoiceToDto(created);
    }

    public async Task<AdminEventChoiceDto?> UpdateEventChoiceAsync(int id, UpdateEventChoiceDto dto)
    {
        var entity = await _context.EventChoices
            .Include(c => c.GrantSkill)
            .Include(c => c.FailureGrantSkill)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (entity == null) return null;

        entity.Text = dto.Text;
        entity.ResultDescription = dto.ResultDescription;
        entity.StatRequirements = dto.StatRequirements;
        entity.CheckStat = dto.CheckStat;
        entity.CheckDifficulty = dto.CheckDifficulty;
        entity.StrengthChange = dto.StrengthChange;
        entity.AgilityChange = dto.AgilityChange;
        entity.IntelligenceChange = dto.IntelligenceChange;
        entity.EnduranceChange = dto.EnduranceChange;
        entity.CharismaChange = dto.CharismaChange;
        entity.LuckChange = dto.LuckChange;
        entity.EnergyChange = dto.EnergyChange;
        entity.HealthChange = dto.HealthChange;
        entity.GoldChange = dto.GoldChange;
        entity.ReputationChange = dto.ReputationChange;
        entity.ExperienceChange = dto.ExperienceChange;
        entity.FailureDescription = dto.FailureDescription;
        entity.FailureStrengthChange = dto.FailureStrengthChange;
        entity.FailureAgilityChange = dto.FailureAgilityChange;
        entity.FailureIntelligenceChange = dto.FailureIntelligenceChange;
        entity.FailureEnduranceChange = dto.FailureEnduranceChange;
        entity.FailureCharismaChange = dto.FailureCharismaChange;
        entity.FailureLuckChange = dto.FailureLuckChange;
        entity.FailureEnergyChange = dto.FailureEnergyChange;
        entity.FailureHealthChange = dto.FailureHealthChange;
        entity.FailureGoldChange = dto.FailureGoldChange;
        entity.FailureReputationChange = dto.FailureReputationChange;
        entity.FollowUpEventId = dto.FollowUpEventId;
        entity.TriggerBattleId = dto.TriggerBattleId;
        entity.GrantSkillId = dto.GrantSkillId;
        entity.FailureGrantSkillId = dto.FailureGrantSkillId;
        entity.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync();
        return MapEventChoiceToDto(entity);
    }

    public async Task<bool> DeleteEventChoiceAsync(int id)
    {
        var entity = await _context.EventChoices.FindAsync(id);
        if (entity == null) return false;

        _context.EventChoices.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // ============ Skills ============

    public async Task<IEnumerable<AdminSkillDto>> GetAllSkillsAsync()
    {
        return await _context.Skills
            .OrderBy(s => s.SkillType)
            .ThenBy(s => s.Rarity)
            .ThenBy(s => s.Name)
            .Select(s => MapSkillToDto(s))
            .ToListAsync();
    }

    public async Task<AdminSkillDto?> GetSkillAsync(int id)
    {
        var entity = await _context.Skills.FindAsync(id);
        return entity == null ? null : MapSkillToDto(entity);
    }

    public async Task<AdminSkillDto> CreateSkillAsync(CreateSkillDto dto)
    {
        var entity = new Skill
        {
            Name = dto.Name,
            Description = dto.Description,
            IconUrl = dto.IconUrl,
            SkillType = dto.SkillType,
            Rarity = dto.Rarity,
            PassiveEffect = dto.PassiveEffect,
            PassiveValue = dto.PassiveValue,
            TriggerChance = dto.TriggerChance,
            BaseDamage = dto.BaseDamage,
            ScalingStat = dto.ScalingStat,
            ScalingMultiplier = dto.ScalingMultiplier,
            ActiveNarrative = dto.ActiveNarrative,
            BonusEffect = dto.BonusEffect,
            BonusPercentage = dto.BonusPercentage,
            BonusFlatValue = dto.BonusFlatValue,
            IsActive = dto.IsActive
        };

        _context.Skills.Add(entity);
        await _context.SaveChangesAsync();
        return MapSkillToDto(entity);
    }

    public async Task<AdminSkillDto?> UpdateSkillAsync(int id, UpdateSkillDto dto)
    {
        var entity = await _context.Skills.FindAsync(id);
        if (entity == null) return null;

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.IconUrl = dto.IconUrl;
        entity.SkillType = dto.SkillType;
        entity.Rarity = dto.Rarity;
        entity.PassiveEffect = dto.PassiveEffect;
        entity.PassiveValue = dto.PassiveValue;
        entity.TriggerChance = dto.TriggerChance;
        entity.BaseDamage = dto.BaseDamage;
        entity.ScalingStat = dto.ScalingStat;
        entity.ScalingMultiplier = dto.ScalingMultiplier;
        entity.ActiveNarrative = dto.ActiveNarrative;
        entity.BonusEffect = dto.BonusEffect;
        entity.BonusPercentage = dto.BonusPercentage;
        entity.BonusFlatValue = dto.BonusFlatValue;
        entity.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return MapSkillToDto(entity);
    }

    public async Task<bool> DeleteSkillAsync(int id)
    {
        var entity = await _context.Skills.FindAsync(id);
        if (entity == null) return false;

        _context.Skills.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // ============ Mapping Helpers ============

    private static AdminTrainingScenarioDto MapTrainingScenarioToDto(TrainingScenario t) => new(
        t.Id, t.Name, t.Description, t.ImageUrl,
        t.PrimaryStat, t.SecondaryStat, t.TertiaryStat,
        t.BaseStatGain, t.SecondaryStatGain, t.TertiaryStatGain,
        t.EnergyCost, t.BonusChance, t.BonusStatGain,
        t.FailureChance, t.FailureHealthPenalty,
        t.BonusSeasons, t.SeasonalBonusMultiplier,
        t.ExperienceGain, t.RequiredLevel, t.TrainingNarrative,
        t.IsActive, t.CreatedAt
    );

    private static AdminEnemyDto MapEnemyToDto(Enemy e) => new(
        e.Id, e.Name, e.Description, e.ImageUrl,
        e.Strength, e.Agility, e.Intelligence, e.Endurance, e.Health,
        e.DifficultyTier, e.EnemyType,
        e.ExperienceReward, e.GoldReward, e.ReputationReward,
        e.LootTable, e.ActiveSeasons, e.Abilities,
        e.IsActive, e.CreatedAt
    );

    private static AdminStorybookDto MapStorybookToDto(Storybook s) => new(
        s.Id, s.Name, s.Description, s.IconUrl, s.Theme,
        s.StrengthBonus, s.AgilityBonus, s.IntelligenceBonus,
        s.EnduranceBonus, s.CharismaBonus, s.LuckBonus,
        s.EventTriggerChance, s.IsUnlockable, s.UnlockCondition,
        s.Events?.Count ?? 0, s.CreatedAt
    );

    private static AdminRandomEventDto MapRandomEventToDto(RandomEvent e) => new(
        e.Id, e.Title, e.Description, e.ImageUrl,
        e.StorybookId, e.Storybook?.Name,
        e.Rarity, e.OutcomeType, e.TriggerActions, e.TriggerSeasons,
        e.BaseProbability, e.StatRequirements, e.IsActive,
        e.Choices?.Count ?? 0, e.CreatedAt,
        e.Choices?.OrderBy(c => c.DisplayOrder).Select(MapEventChoiceToDto).ToList() ?? new List<AdminEventChoiceDto>()
    );

    private static AdminEventChoiceDto MapEventChoiceToDto(EventChoice c) => new(
        c.Id, c.RandomEventId, c.Text, c.ResultDescription,
        c.StatRequirements, c.CheckStat, c.CheckDifficulty,
        c.StrengthChange, c.AgilityChange, c.IntelligenceChange,
        c.EnduranceChange, c.CharismaChange, c.LuckChange,
        c.EnergyChange, c.HealthChange, c.GoldChange,
        c.ReputationChange, c.ExperienceChange,
        c.FailureDescription, c.FailureStrengthChange, c.FailureAgilityChange,
        c.FailureIntelligenceChange, c.FailureEnduranceChange, c.FailureCharismaChange,
        c.FailureLuckChange, c.FailureEnergyChange, c.FailureHealthChange,
        c.FailureGoldChange, c.FailureReputationChange,
        c.FollowUpEventId, c.TriggerBattleId,
        c.GrantSkillId, c.GrantSkill?.Name,
        c.FailureGrantSkillId, c.FailureGrantSkill?.Name,
        c.DisplayOrder
    );

    private static AdminSkillDto MapSkillToDto(Skill s) => new(
        s.Id, s.Name, s.Description, s.IconUrl,
        s.SkillType, s.Rarity,
        s.PassiveEffect, s.PassiveValue,
        s.TriggerChance, s.BaseDamage, s.ScalingStat, s.ScalingMultiplier, s.ActiveNarrative,
        s.BonusEffect, s.BonusPercentage, s.BonusFlatValue,
        s.IsActive
    );
}
