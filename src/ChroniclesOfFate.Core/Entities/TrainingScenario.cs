using ChroniclesOfFate.Core.Enums;

namespace ChroniclesOfFate.Core.Entities;

/// <summary>
/// Training scenarios available for each stat
/// Different scenarios have different energy costs and stat gains
/// </summary>
public class TrainingScenario
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Primary stat this training improves
    /// </summary>
    public StatType PrimaryStat { get; set; }
    
    /// <summary>
    /// Secondary stat that may also improve
    /// </summary>
    public StatType? SecondaryStat { get; set; }
    
    /// <summary>
    /// Base stat gain for primary stat
    /// </summary>
    public int BaseStatGain { get; set; } = 5;
    
    /// <summary>
    /// Base stat gain for secondary stat (if any)
    /// </summary>
    public int SecondaryStatGain { get; set; } = 2;
    
    /// <summary>
    /// Energy cost to perform this training
    /// </summary>
    public int EnergyCost { get; set; } = 20;
    
    /// <summary>
    /// Chance of bonus stat gain (0.0 - 1.0)
    /// </summary>
    public double BonusChance { get; set; } = 0.1;
    
    /// <summary>
    /// Bonus stat gain when triggered
    /// </summary>
    public int BonusStatGain { get; set; } = 3;
    
    /// <summary>
    /// Chance of injury/failure (0.0 - 1.0)
    /// </summary>
    public double FailureChance { get; set; } = 0.05;
    
    /// <summary>
    /// Health penalty on failure
    /// </summary>
    public int FailureHealthPenalty { get; set; } = 10;
    
    /// <summary>
    /// Seasons with bonus effectiveness (null = none)
    /// </summary>
    public string? BonusSeasons { get; set; }
    
    /// <summary>
    /// Seasonal bonus multiplier
    /// </summary>
    public double SeasonalBonusMultiplier { get; set; } = 1.2;
    
    /// <summary>
    /// Experience gained from training
    /// </summary>
    public int ExperienceGain { get; set; } = 10;
    
    /// <summary>
    /// Minimum level required
    /// </summary>
    public int RequiredLevel { get; set; } = 1;
    
    /// <summary>
    /// Narrative text shown during training
    /// </summary>
    public string TrainingNarrative { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
