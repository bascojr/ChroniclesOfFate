using ChroniclesOfFate.Core.Enums;

namespace ChroniclesOfFate.Core.Entities;

/// <summary>
/// Enemy templates for auto-battles
/// </summary>
public class Enemy
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    
    // Combat Stats
    public int Strength { get; set; }
    public int Agility { get; set; }
    public int Intelligence { get; set; }
    public int Endurance { get; set; }
    public int Health { get; set; }
    
    /// <summary>
    /// Difficulty tier (1-10) determines when enemies appear
    /// </summary>
    public int DifficultyTier { get; set; } = 1;
    
    /// <summary>
    /// Enemy type affects combat calculations
    /// </summary>
    public string EnemyType { get; set; } = "Normal";
    
    // Rewards
    public int ExperienceReward { get; set; }
    public int GoldReward { get; set; }
    public int ReputationReward { get; set; }
    
    /// <summary>
    /// Possible item drops (JSON format)
    /// </summary>
    public string? LootTable { get; set; }
    
    /// <summary>
    /// Which seasons this enemy appears (null = all)
    /// </summary>
    public string? ActiveSeasons { get; set; }
    
    /// <summary>
    /// Special abilities (JSON format)
    /// </summary>
    public string? Abilities { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
