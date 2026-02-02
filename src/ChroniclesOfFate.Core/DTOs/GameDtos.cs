using ChroniclesOfFate.Core.Enums;

namespace ChroniclesOfFate.Core.DTOs;

// ============ Authentication DTOs ============

public record LoginDto(string Username, string Password);

public record RegisterDto(string Username, string Email, string Password);

public record AuthResponseDto(
    string Token,
    string RefreshToken,
    DateTime Expiration,
    UserDto User
);

public record RefreshTokenDto(string Token, string RefreshToken);

public record UserDto(int Id, string Username, string Email, string Role);

// ============ Character DTOs ============

public record CreateCharacterDto(
    string Name,
    CharacterClass Class
);

public record CharacterDto(
    int Id,
    string Name,
    CharacterClass Class,
    int Strength,
    int Agility,
    int Intelligence,
    int Endurance,
    int Charisma,
    int Luck,
    int CurrentEnergy,
    int MaxEnergy,
    int CurrentHealth,
    int MaxHealth,
    int Level,
    int Experience,
    int Gold,
    int Reputation,
    int CurrentYear,
    int CurrentMonth,
    int TotalTurns,
    Season CurrentSeason,
    int TotalPower,
    bool IsGameComplete,
    List<StorybookDto> EquippedStorybooks,
    int ExperienceForNextLevel,
    bool IsMaxLevel
);

public record CharacterStatsDto(
    int Strength,
    int Agility,
    int Intelligence,
    int Endurance,
    int Charisma,
    int Luck,
    int TotalPower
);

// ============ Game Session DTOs ============

public record CreateGameSessionDto(string SessionName, CreateCharacterDto Character, List<int>? SelectedStorybookIds = null);

public record GameSessionDto(
    int Id,
    string SessionName,
    GameState State,
    int? FinalScore,
    string? EndingType,
    CharacterDto? Character,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record GameSessionListDto(
    int Id,
    string SessionName,
    GameState State,
    string? CharacterName,
    int? CharacterLevel,
    int? TotalTurns,
    DateTime UpdatedAt
);

// ============ Storybook DTOs ============

public record StorybookDto(
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
    List<StorybookEventSummaryDto> Events
);

public record StorybookEventSummaryDto(
    string Title,
    string Description,
    Rarity Rarity
);

public record EquipStorybookDto(int StorybookId, int SlotPosition);

public record LoadoutDto(List<EquipStorybookDto> Storybooks);

// ============ Turn/Action DTOs ============

public record TurnActionDto(ActionType Action, int? TargetId);

public record TurnResultDto(
    bool Success,
    string Narrative,
    List<StatChangeDto> StatChanges,
    RandomEventDto? TriggeredEvent,
    BattleResultDto? BattleResult,
    CharacterDto UpdatedCharacter
);

public record StatChangeDto(string StatName, int OldValue, int NewValue, int Change);

// ============ Training DTOs ============

public record TrainingScenarioDto(
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
    int ExperienceGain,
    bool HasSeasonalBonus
);

public record TrainingResultDto(
    bool Success,
    string Narrative,
    int PrimaryStatGain,
    int SecondaryStatGain,
    int TertiaryStatGain,
    bool BonusTriggered,
    bool FailureOccurred,
    int EnergySpent,
    int ExperienceGained,
    List<StatChangeDto> StatChanges,
    LevelUpResultDto? LevelUp
);

// ============ Random Event DTOs ============

public record RandomEventDto(
    int Id,
    string Title,
    string Description,
    string ImageUrl,
    Rarity Rarity,
    EventOutcome OutcomeType,
    string? SourceStorybook,
    List<EventChoiceDto> Choices
);

public record EventChoiceDto(
    int Id,
    string Text,
    bool IsHidden,
    StatType? CheckStat,
    int CheckDifficulty,
    string? StatRequirementHint
);

public record EventChoiceResultDto(
    bool Success,
    bool? CheckSucceeded,
    int? RollResult,
    int? CheckDifficulty,
    string ResultDescription,
    List<StatChangeDto> StatChanges,
    RandomEventDto? FollowUpEvent,
    int? TriggerBattleId
);

// ============ Battle DTOs ============

public record EnemyDto(
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
    string EnemyType
);

public record BattleResultDto(
    BattleResult Result,
    string Narrative,
    List<BattleRoundDto> Rounds,
    int ExperienceGained,
    int GoldGained,
    int ReputationGained,
    int HealthLost,
    int EnergySpent,
    EnemyDto Enemy,
    LevelUpResultDto? LevelUp
);

public record BattleRoundDto(
    int RoundNumber,
    string PlayerAction,
    int PlayerDamage,
    string EnemyAction,
    int EnemyDamage,
    int PlayerHealthRemaining,
    int EnemyHealthRemaining,
    string Narrative
);

public record InitiateBattleDto(int? EnemyId);

// ============ Game State DTOs ============

public record GameStateDto(
    CharacterDto Character,
    List<TrainingScenarioDto> AvailableTraining,
    List<StorybookDto> EquippedStorybooks,
    List<StorybookDto> AvailableStorybooks,
    SeasonInfoDto SeasonInfo,
    List<string> AvailableActions
);

public record SeasonInfoDto(
    Season Season,
    string Name,
    string Description,
    List<string> Bonuses
);

// ============ Leaderboard/Stats DTOs ============

public record LeaderboardEntryDto(
    int Rank,
    string Username,
    string CharacterName,
    CharacterClass Class,
    int FinalScore,
    int TotalPower,
    string EndingType,
    DateTime CompletedAt
);

public record PlayerStatsDto(
    int TotalGamesPlayed,
    int GamesCompleted,
    int HighestScore,
    int TotalBattlesWon,
    int TotalEventsCompleted,
    CharacterClass FavoriteClass
);

// ============ Message Log DTOs ============

public record MessageLogEntryDto(
    int Id,
    string Message,
    string Type,
    int Year,
    int Month,
    List<StatChangeDto> StatChanges
);

public record AddMessageLogDto(
    string Message,
    string Type,
    int Year,
    int Month,
    List<StatChangeDto> StatChanges
);

// ============ Skill DTOs ============

public record SkillDto(
    int Id,
    string Name,
    string Description,
    string IconUrl,
    SkillType SkillType,
    Rarity Rarity,
    // Passive
    PassiveEffect? PassiveEffect,
    double PassiveValue,
    // Active
    double TriggerChance,
    int BaseDamage,
    StatType? ScalingStat,
    double ScalingMultiplier,
    string? ActiveNarrative,
    // Bonus
    BonusEffect? BonusEffect,
    double BonusPercentage,
    int BonusFlatValue
);

public record CharacterSkillDto(
    int Id,
    SkillDto Skill,
    DateTime AcquiredAt,
    int AcquiredOnTurn,
    string? AcquisitionSource
);

public record SkillTriggerResultDto(
    string SkillName,
    string Narrative,
    int DamageDealt,
    int HealingDone
);

// ============ Level Up DTOs ============

public record LevelUpResultDto(
    int OldLevel,
    int NewLevel,
    int OldMaxHealth,
    int NewMaxHealth,
    int OldMaxEnergy,
    int NewMaxEnergy,
    int ExperienceForNextLevel
);
