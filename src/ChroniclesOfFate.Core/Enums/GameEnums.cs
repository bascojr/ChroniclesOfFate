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
    Battle = 3,
    Study = 4
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
