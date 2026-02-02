using ChroniclesOfFate.Core.Enums;

namespace ChroniclesOfFate.Core.Entities;

/// <summary>
/// Represents a skill that can be acquired by characters
/// </summary>
public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;

    /// <summary>
    /// The type of skill (Passive, Active, Bonus)
    /// </summary>
    public SkillType SkillType { get; set; }

    /// <summary>
    /// Rarity affects how often the skill appears in events
    /// </summary>
    public Rarity Rarity { get; set; }

    // === PASSIVE SKILL PROPERTIES ===
    /// <summary>
    /// The passive effect type (only for Passive skills)
    /// </summary>
    public PassiveEffect? PassiveEffect { get; set; }

    /// <summary>
    /// The value of the passive effect (e.g., 10 = 10% evasion)
    /// </summary>
    public double PassiveValue { get; set; }

    // === ACTIVE SKILL PROPERTIES ===
    /// <summary>
    /// Chance for active skill to trigger during combat (0.0 - 1.0)
    /// </summary>
    public double TriggerChance { get; set; }

    /// <summary>
    /// Base damage of active skill
    /// </summary>
    public int BaseDamage { get; set; }

    /// <summary>
    /// Which stat scales the active skill damage
    /// </summary>
    public StatType? ScalingStat { get; set; }

    /// <summary>
    /// Multiplier for stat scaling (e.g., 0.5 = 50% of stat added to damage)
    /// </summary>
    public double ScalingMultiplier { get; set; }

    /// <summary>
    /// Narrative text when the active skill triggers
    /// </summary>
    public string? ActiveNarrative { get; set; }

    // === BONUS SKILL PROPERTIES ===
    /// <summary>
    /// The bonus effect type (only for Bonus skills)
    /// </summary>
    public BonusEffect? BonusEffect { get; set; }

    /// <summary>
    /// The percentage bonus (e.g., 10 = 10% more gold)
    /// </summary>
    public double BonusPercentage { get; set; }

    /// <summary>
    /// Flat bonus value (added on top of percentage)
    /// </summary>
    public int BonusFlatValue { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<CharacterSkill> CharacterSkills { get; set; } = new List<CharacterSkill>();
}
