namespace ChroniclesOfFate.Core.Entities;

/// <summary>
/// Join table for Character and Storybook (equipped books)
/// Maximum of 5 storybooks can be equipped
/// </summary>
public class CharacterStorybook
{
    public int CharacterId { get; set; }
    public Character? Character { get; set; }
    
    public int StorybookId { get; set; }
    public Storybook? Storybook { get; set; }
    
    /// <summary>
    /// Slot position (1-5)
    /// </summary>
    public int SlotPosition { get; set; }
    
    public DateTime EquippedAt { get; set; } = DateTime.UtcNow;
}
