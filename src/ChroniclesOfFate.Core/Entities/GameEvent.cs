using ChroniclesOfFate.Core.Enums;

namespace ChroniclesOfFate.Core.Entities;

/// <summary>
/// Records events that have occurred during gameplay
/// </summary>
public class GameEvent
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public Character? Character { get; set; }
    
    public int RandomEventId { get; set; }
    public RandomEvent? RandomEvent { get; set; }
    
    public int? ChosenOptionId { get; set; }
    public EventChoice? ChosenOption { get; set; }
    
    /// <summary>
    /// Year and month when event occurred
    /// </summary>
    public int Year { get; set; }
    public int Month { get; set; }
    public int Turn { get; set; }
    
    /// <summary>
    /// Whether the stat check succeeded (if applicable)
    /// </summary>
    public bool? CheckSucceeded { get; set; }
    
    /// <summary>
    /// Roll result for stat check
    /// </summary>
    public int? RollResult { get; set; }
    
    /// <summary>
    /// Summary of stat changes from this event
    /// </summary>
    public string ResultSummary { get; set; } = string.Empty;
    
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
