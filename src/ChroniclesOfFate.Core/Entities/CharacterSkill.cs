namespace ChroniclesOfFate.Core.Entities;

/// <summary>
/// Junction table for Character-Skill many-to-many relationship
/// </summary>
public class CharacterSkill
{
    public int Id { get; set; }

    public int CharacterId { get; set; }
    public Character? Character { get; set; }

    public int SkillId { get; set; }
    public Skill? Skill { get; set; }

    /// <summary>
    /// When the skill was acquired
    /// </summary>
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Which turn the skill was acquired on
    /// </summary>
    public int AcquiredOnTurn { get; set; }

    /// <summary>
    /// How the skill was acquired (e.g., "Event: Ancient Wisdom")
    /// </summary>
    public string? AcquisitionSource { get; set; }
}
