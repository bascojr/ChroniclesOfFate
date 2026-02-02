using ChroniclesOfFate.Core.Enums;

namespace ChroniclesOfFate.Core.Entities;

/// <summary>
/// Random events that can occur during gameplay
/// Events can be global or tied to specific storybooks
/// </summary>
public class RandomEvent
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// If set, this event only triggers from this storybook
    /// </summary>
    public int? StorybookId { get; set; }
    public Storybook? Storybook { get; set; }
    
    public Rarity Rarity { get; set; }
    public EventOutcome OutcomeType { get; set; }
    
    /// <summary>
    /// Which action types can trigger this event
    /// Stored as comma-separated values: "Train,Explore,Battle"
    /// </summary>
    public string TriggerActions { get; set; } = string.Empty;
    
    /// <summary>
    /// Which seasons this event can occur in (null = all seasons)
    /// Stored as comma-separated values: "Spring,Summer"
    /// </summary>
    public string? TriggerSeasons { get; set; }
    
    /// <summary>
    /// Base probability of this event occurring (0.0 - 1.0)
    /// </summary>
    public double BaseProbability { get; set; } = 0.1;
    
    /// <summary>
    /// Minimum stat requirements to trigger (JSON format)
    /// Example: {"Strength": 50, "Intelligence": 30}
    /// </summary>
    public string? StatRequirements { get; set; }
    
    /// <summary>
    /// Available choices for this event
    /// </summary>
    public ICollection<EventChoice> Choices { get; set; } = new List<EventChoice>();
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
