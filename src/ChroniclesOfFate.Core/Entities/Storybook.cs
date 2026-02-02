using ChroniclesOfFate.Core.Enums;

namespace ChroniclesOfFate.Core.Entities;

/// <summary>
/// Storybooks add additional random events to scenarios when equipped
/// Players can equip up to 5 storybooks at a time
/// </summary>
public class Storybook
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;

    /// <summary>
    /// Theme of the storybook affects which events can trigger
    /// </summary>
    public string Theme { get; set; } = string.Empty;
    
    /// <summary>
    /// Stat bonuses when equipped
    /// </summary>
    public int StrengthBonus { get; set; }
    public int AgilityBonus { get; set; }
    public int IntelligenceBonus { get; set; }
    public int EnduranceBonus { get; set; }
    public int CharismaBonus { get; set; }
    public int LuckBonus { get; set; }
    
    /// <summary>
    /// Chance modifier for triggering book-specific events (0.0 - 1.0)
    /// </summary>
    public double EventTriggerChance { get; set; } = 0.15;
    
    /// <summary>
    /// Events specific to this storybook
    /// </summary>
    public ICollection<RandomEvent> Events { get; set; } = new List<RandomEvent>();
    
    /// <summary>
    /// Many-to-many with characters
    /// </summary>
    public ICollection<CharacterStorybook> CharacterStorybooks { get; set; } = new List<CharacterStorybook>();
    
    public bool IsUnlockable { get; set; } = true;
    public string? UnlockCondition { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
