using ChroniclesOfFate.Core.Enums;

namespace ChroniclesOfFate.Core.Entities;

/// <summary>
/// Represents the player's character in the game
/// </summary>
public class Character
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CharacterClass Class { get; set; }
    
    // Core Stats (0-999 scale)
    public int Strength { get; set; }
    public int Agility { get; set; }
    public int Intelligence { get; set; }
    public int Endurance { get; set; }
    public int Charisma { get; set; }
    public int Luck { get; set; }
    
    // Resource Stats
    public int CurrentEnergy { get; set; }
    public int MaxEnergy { get; set; } = 100;
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; } = 100;
    
    // Progression
    public int Level { get; set; } = 1;
    public int Experience { get; set; }
    public int Gold { get; set; }
    public int Reputation { get; set; }
    
    // Game Timeline
    public int CurrentYear { get; set; } = 1;
    public int CurrentMonth { get; set; } = 1;
    public int TotalTurns { get; set; }
    
    // Relationships
    public int GameSessionId { get; set; }
    public GameSession? GameSession { get; set; }
    public ICollection<CharacterStorybook> EquippedStorybooks { get; set; } = new List<CharacterStorybook>();
    public ICollection<CharacterSkill> Skills { get; set; } = new List<CharacterSkill>();
    public ICollection<GameEvent> EventHistory { get; set; } = new List<GameEvent>();
    public ICollection<BattleLog> BattleHistory { get; set; } = new List<BattleLog>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the current season based on the month
    /// </summary>
    public Season CurrentSeason => CurrentMonth switch
    {
        >= 3 and <= 5 => Season.Spring,
        >= 6 and <= 8 => Season.Summer,
        >= 9 and <= 11 => Season.Autumn,
        _ => Season.Winter
    };

    /// <summary>
    /// Gets the total power rating based on all stats
    /// </summary>
    public int TotalPower => Strength + Agility + Intelligence + Endurance + Charisma + Luck;

    /// <summary>
    /// Gets a specific stat value by type
    /// </summary>
    public int GetStat(StatType stat) => stat switch
    {
        StatType.Strength => Strength,
        StatType.Agility => Agility,
        StatType.Intelligence => Intelligence,
        StatType.Endurance => Endurance,
        StatType.Charisma => Charisma,
        StatType.Luck => Luck,
        _ => 0
    };

    /// <summary>
    /// Sets a specific stat value by type
    /// </summary>
    public void SetStat(StatType stat, int value)
    {
        value = Math.Clamp(value, 0, 999);
        switch (stat)
        {
            case StatType.Strength: Strength = value; break;
            case StatType.Agility: Agility = value; break;
            case StatType.Intelligence: Intelligence = value; break;
            case StatType.Endurance: Endurance = value; break;
            case StatType.Charisma: Charisma = value; break;
            case StatType.Luck: Luck = value; break;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds to a specific stat
    /// </summary>
    public void AddStat(StatType stat, int amount)
    {
        SetStat(stat, GetStat(stat) + amount);
    }

    /// <summary>
    /// Checks if the game has ended (10 years = 120 months)
    /// </summary>
    public bool IsGameComplete => TotalTurns >= 120;

    /// <summary>
    /// Advances to the next turn/month
    /// </summary>
    public void AdvanceTurn()
    {
        TotalTurns++;
        CurrentMonth++;
        if (CurrentMonth > 12)
        {
            CurrentMonth = 1;
            CurrentYear++;
        }
        UpdatedAt = DateTime.UtcNow;
    }
}
