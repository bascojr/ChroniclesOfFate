namespace ChroniclesOfFate.Core.Enums;

/// <summary>
/// Character statistics that can be trained and affect combat/events
/// </summary>
public enum StatType
{
    Strength = 0,
    Agility = 1,
    Intelligence = 2,
    Endurance = 3,
    Charisma = 4,
    Luck = 5
}

/// <summary>
/// Actions available during each turn
/// </summary>
public enum ActionType
{
    Train = 0,
    Rest = 1,
    Explore = 2,
    Battle = 3
}

/// <summary>
/// Rarity levels for storybooks and events
/// </summary>
public enum Rarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4
}

/// <summary>
/// Event outcome types
/// </summary>
public enum EventOutcome
{
    Positive = 0,
    Negative = 1,
    Neutral = 2,
    Critical = 3
}

/// <summary>
/// Character class affecting base stats and growth
/// </summary>
public enum CharacterClass
{
    Warrior = 0,
    Mage = 1,
    Rogue = 2,
    Cleric = 3,
    Ranger = 4
}

/// <summary>
/// Game state for save/load
/// </summary>
public enum GameState
{
    NewGame = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3
}

/// <summary>
/// Seasons affect available events and training bonuses
/// </summary>
public enum Season
{
    Spring = 0,
    Summer = 1,
    Autumn = 2,
    Winter = 3
}

/// <summary>
/// Battle result types
/// </summary>
public enum BattleResult
{
    Victory = 0,
    Defeat = 1,
    Draw = 2,
    Fled = 3
}

/// <summary>
/// Skill types - determines how the skill functions
/// </summary>
public enum SkillType
{
    /// <summary>
    /// Passive combat bonuses (evasion, critical chance, damage reduction)
    /// </summary>
    Passive = 0,

    /// <summary>
    /// Active skills used during combat with chance to trigger
    /// </summary>
    Active = 1,

    /// <summary>
    /// Adventure bonuses (gold gain, XP gain, energy gain)
    /// </summary>
    Bonus = 2
}

/// <summary>
/// Specific passive skill effects
/// </summary>
public enum PassiveEffect
{
    Evasion = 0,           // Chance to dodge attacks
    CriticalChance = 1,    // Chance for critical hits
    DamageReduction = 2,   // Reduce incoming damage
    LifeSteal = 3,         // Heal on damage dealt
    CounterAttack = 4,     // Chance to counter when hit
    Thorns = 5             // Reflect damage when hit
}

/// <summary>
/// Specific bonus skill effects
/// </summary>
public enum BonusEffect
{
    GoldGain = 0,          // Bonus gold from all sources
    ExperienceGain = 1,    // Bonus XP from all sources
    EnergyGain = 2,        // Bonus energy recovery
    HealthRegen = 3,       // Heal each turn
    LuckBoost = 4,         // Improved event outcomes
    TrainingBoost = 5      // Bonus stats from training
}
