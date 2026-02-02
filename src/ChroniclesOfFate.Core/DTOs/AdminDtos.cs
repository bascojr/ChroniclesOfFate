using ChroniclesOfFate.Core.Enums;

namespace ChroniclesOfFate.Core.DTOs;

// ============ Dashboard DTOs ============

public record AdminDashboardDto(
    int TotalTrainingScenarios,
    int TotalEnemies,
    int TotalStorybooks,
    int TotalRandomEvents,
    int TotalEventChoices,
    int TotalSkills,
    int TotalUsers,
    int TotalGameSessions
);

public record LookupDto(int Id, string Name);

// ============ Training Scenario Admin DTOs ============

public record AdminTrainingScenarioDto(
    int Id,
    string Name,
    string Description,
    string ImageUrl,
    StatType PrimaryStat,
    StatType? SecondaryStat,
    StatType? TertiaryStat,
    int BaseStatGain,
    int SecondaryStatGain,
    int TertiaryStatGain,
    int EnergyCost,
    double BonusChance,
    int BonusStatGain,
    double FailureChance,
    int FailureHealthPenalty,
    string? BonusSeasons,
    double SeasonalBonusMultiplier,
    int ExperienceGain,
    int RequiredLevel,
    string TrainingNarrative,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateTrainingScenarioDto(
    string Name,
    string Description,
    string ImageUrl,
    StatType PrimaryStat,
    StatType? SecondaryStat,
    StatType? TertiaryStat,
    int BaseStatGain,
    int SecondaryStatGain,
    int TertiaryStatGain,
    int EnergyCost,
    double BonusChance,
    int BonusStatGain,
    double FailureChance,
    int FailureHealthPenalty,
    string? BonusSeasons,
    double SeasonalBonusMultiplier,
    int ExperienceGain,
    int RequiredLevel,
    string TrainingNarrative,
    bool IsActive
);

public record UpdateTrainingScenarioDto(
    string Name,
    string Description,
    string ImageUrl,
    StatType PrimaryStat,
    StatType? SecondaryStat,
    StatType? TertiaryStat,
    int BaseStatGain,
    int SecondaryStatGain,
    int TertiaryStatGain,
    int EnergyCost,
    double BonusChance,
    int BonusStatGain,
    double FailureChance,
    int FailureHealthPenalty,
    string? BonusSeasons,
    double SeasonalBonusMultiplier,
    int ExperienceGain,
    int RequiredLevel,
    string TrainingNarrative,
    bool IsActive
);

// ============ Enemy Admin DTOs ============

public record AdminEnemyDto(
    int Id,
    string Name,
    string Description,
    string ImageUrl,
    int Strength,
    int Agility,
    int Intelligence,
    int Endurance,
    int Health,
    int DifficultyTier,
    string EnemyType,
    int ExperienceReward,
    int GoldReward,
    int ReputationReward,
    string? LootTable,
    string? ActiveSeasons,
    string? Abilities,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateEnemyDto(
    string Name,
    string Description,
    string ImageUrl,
    int Strength,
    int Agility,
    int Intelligence,
    int Endurance,
    int Health,
    int DifficultyTier,
    string EnemyType,
    int ExperienceReward,
    int GoldReward,
    int ReputationReward,
    string? LootTable,
    string? ActiveSeasons,
    string? Abilities,
    bool IsActive
);

public record UpdateEnemyDto(
    string Name,
    string Description,
    string ImageUrl,
    int Strength,
    int Agility,
    int Intelligence,
    int Endurance,
    int Health,
    int DifficultyTier,
    string EnemyType,
    int ExperienceReward,
    int GoldReward,
    int ReputationReward,
    string? LootTable,
    string? ActiveSeasons,
    string? Abilities,
    bool IsActive
);

// ============ Storybook Admin DTOs ============

public record AdminStorybookDto(
    int Id,
    string Name,
    string Description,
    string IconUrl,
    string Theme,
    int StrengthBonus,
    int AgilityBonus,
    int IntelligenceBonus,
    int EnduranceBonus,
    int CharismaBonus,
    int LuckBonus,
    double EventTriggerChance,
    bool IsUnlockable,
    string? UnlockCondition,
    int EventCount,
    DateTime CreatedAt
);

public record CreateStorybookDto(
    string Name,
    string Description,
    string IconUrl,
    string Theme,
    int StrengthBonus,
    int AgilityBonus,
    int IntelligenceBonus,
    int EnduranceBonus,
    int CharismaBonus,
    int LuckBonus,
    double EventTriggerChance,
    bool IsUnlockable,
    string? UnlockCondition
);

public record UpdateStorybookDto(
    string Name,
    string Description,
    string IconUrl,
    string Theme,
    int StrengthBonus,
    int AgilityBonus,
    int IntelligenceBonus,
    int EnduranceBonus,
    int CharismaBonus,
    int LuckBonus,
    double EventTriggerChance,
    bool IsUnlockable,
    string? UnlockCondition
);

// ============ Random Event Admin DTOs ============

public record AdminRandomEventDto(
    int Id,
    string Title,
    string Description,
    string ImageUrl,
    int? StorybookId,
    string? StorybookName,
    Rarity Rarity,
    EventOutcome OutcomeType,
    string TriggerActions,
    string? TriggerSeasons,
    double BaseProbability,
    string? StatRequirements,
    bool IsActive,
    int ChoiceCount,
    DateTime CreatedAt,
    List<AdminEventChoiceDto> Choices
);

public record CreateRandomEventDto(
    string Title,
    string Description,
    string ImageUrl,
    int? StorybookId,
    Rarity Rarity,
    EventOutcome OutcomeType,
    string TriggerActions,
    string? TriggerSeasons,
    double BaseProbability,
    string? StatRequirements,
    bool IsActive
);

public record UpdateRandomEventDto(
    string Title,
    string Description,
    string ImageUrl,
    int? StorybookId,
    Rarity Rarity,
    EventOutcome OutcomeType,
    string TriggerActions,
    string? TriggerSeasons,
    double BaseProbability,
    string? StatRequirements,
    bool IsActive
);

// ============ Event Choice Admin DTOs ============

public record AdminEventChoiceDto(
    int Id,
    int RandomEventId,
    string Text,
    string ResultDescription,
    string? StatRequirements,
    StatType? CheckStat,
    int CheckDifficulty,
    int StrengthChange,
    int AgilityChange,
    int IntelligenceChange,
    int EnduranceChange,
    int CharismaChange,
    int LuckChange,
    int EnergyChange,
    int HealthChange,
    int GoldChange,
    int ReputationChange,
    int ExperienceChange,
    string? FailureDescription,
    int FailureStrengthChange,
    int FailureAgilityChange,
    int FailureIntelligenceChange,
    int FailureEnduranceChange,
    int FailureCharismaChange,
    int FailureLuckChange,
    int FailureEnergyChange,
    int FailureHealthChange,
    int FailureGoldChange,
    int FailureReputationChange,
    int? FollowUpEventId,
    int? TriggerBattleId,
    int? GrantSkillId,
    string? GrantSkillName,
    int? FailureGrantSkillId,
    string? FailureGrantSkillName,
    int DisplayOrder
);

public record CreateEventChoiceDto(
    int RandomEventId,
    string Text,
    string ResultDescription,
    string? StatRequirements,
    StatType? CheckStat,
    int CheckDifficulty,
    int StrengthChange,
    int AgilityChange,
    int IntelligenceChange,
    int EnduranceChange,
    int CharismaChange,
    int LuckChange,
    int EnergyChange,
    int HealthChange,
    int GoldChange,
    int ReputationChange,
    int ExperienceChange,
    string? FailureDescription,
    int FailureStrengthChange,
    int FailureAgilityChange,
    int FailureIntelligenceChange,
    int FailureEnduranceChange,
    int FailureCharismaChange,
    int FailureLuckChange,
    int FailureEnergyChange,
    int FailureHealthChange,
    int FailureGoldChange,
    int FailureReputationChange,
    int? FollowUpEventId,
    int? TriggerBattleId,
    int? GrantSkillId,
    int? FailureGrantSkillId,
    int DisplayOrder
);

public record UpdateEventChoiceDto(
    string Text,
    string ResultDescription,
    string? StatRequirements,
    StatType? CheckStat,
    int CheckDifficulty,
    int StrengthChange,
    int AgilityChange,
    int IntelligenceChange,
    int EnduranceChange,
    int CharismaChange,
    int LuckChange,
    int EnergyChange,
    int HealthChange,
    int GoldChange,
    int ReputationChange,
    int ExperienceChange,
    string? FailureDescription,
    int FailureStrengthChange,
    int FailureAgilityChange,
    int FailureIntelligenceChange,
    int FailureEnduranceChange,
    int FailureCharismaChange,
    int FailureLuckChange,
    int FailureEnergyChange,
    int FailureHealthChange,
    int FailureGoldChange,
    int FailureReputationChange,
    int? FollowUpEventId,
    int? TriggerBattleId,
    int? GrantSkillId,
    int? FailureGrantSkillId,
    int DisplayOrder
);

// ============ Skill Admin DTOs ============

public record AdminSkillDto(
    int Id,
    string Name,
    string Description,
    string IconUrl,
    SkillType SkillType,
    Rarity Rarity,
    PassiveEffect? PassiveEffect,
    double PassiveValue,
    double TriggerChance,
    int BaseDamage,
    StatType? ScalingStat,
    double ScalingMultiplier,
    string? ActiveNarrative,
    BonusEffect? BonusEffect,
    double BonusPercentage,
    int BonusFlatValue,
    bool IsActive
);

public record CreateSkillDto(
    string Name,
    string Description,
    string IconUrl,
    SkillType SkillType,
    Rarity Rarity,
    PassiveEffect? PassiveEffect,
    double PassiveValue,
    double TriggerChance,
    int BaseDamage,
    StatType? ScalingStat,
    double ScalingMultiplier,
    string? ActiveNarrative,
    BonusEffect? BonusEffect,
    double BonusPercentage,
    int BonusFlatValue,
    bool IsActive
);

public record UpdateSkillDto(
    string Name,
    string Description,
    string IconUrl,
    SkillType SkillType,
    Rarity Rarity,
    PassiveEffect? PassiveEffect,
    double PassiveValue,
    double TriggerChance,
    int BaseDamage,
    StatType? ScalingStat,
    double ScalingMultiplier,
    string? ActiveNarrative,
    BonusEffect? BonusEffect,
    double BonusPercentage,
    int BonusFlatValue,
    bool IsActive
);
