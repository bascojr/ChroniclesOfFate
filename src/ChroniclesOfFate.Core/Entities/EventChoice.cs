using ChroniclesOfFate.Core.Enums;

namespace ChroniclesOfFate.Core.Entities;

/// <summary>
/// Represents a choice/decision within a random event
/// Forms the decision tree structure
/// </summary>
public class EventChoice
{
    public int Id { get; set; }
    public int RandomEventId { get; set; }
    public RandomEvent? RandomEvent { get; set; }
    
    public string Text { get; set; } = string.Empty;
    public string ResultDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// Stat requirements to see this choice (JSON format)
    /// Hidden choices appear when requirements are met
    /// </summary>
    public string? StatRequirements { get; set; }
    
    /// <summary>
    /// Stat that determines success chance for this choice
    /// </summary>
    public StatType? CheckStat { get; set; }
    
    /// <summary>
    /// Difficulty of the stat check (1-100)
    /// </summary>
    public int CheckDifficulty { get; set; }
    
    // Rewards/Penalties on success
    public int StrengthChange { get; set; }
    public int AgilityChange { get; set; }
    public int IntelligenceChange { get; set; }
    public int EnduranceChange { get; set; }
    public int CharismaChange { get; set; }
    public int LuckChange { get; set; }
    public int EnergyChange { get; set; }
    public int HealthChange { get; set; }
    public int GoldChange { get; set; }
    public int ReputationChange { get; set; }
    public int ExperienceChange { get; set; }
    
    // Different outcomes on failure (if stat check exists)
    public string? FailureDescription { get; set; }
    public int FailureStrengthChange { get; set; }
    public int FailureAgilityChange { get; set; }
    public int FailureIntelligenceChange { get; set; }
    public int FailureEnduranceChange { get; set; }
    public int FailureCharismaChange { get; set; }
    public int FailureLuckChange { get; set; }
    public int FailureEnergyChange { get; set; }
    public int FailureHealthChange { get; set; }
    public int FailureGoldChange { get; set; }
    public int FailureReputationChange { get; set; }
    
    /// <summary>
    /// Can trigger a follow-up event
    /// </summary>
    public int? FollowUpEventId { get; set; }
    
    /// <summary>
    /// Can trigger a battle encounter
    /// </summary>
    public int? TriggerBattleId { get; set; }
    
    public int DisplayOrder { get; set; }
}
