using ChroniclesOfFate.Core.Enums;

namespace ChroniclesOfFate.Core.Entities;

/// <summary>
/// Records battle history and outcomes
/// </summary>
public class BattleLog
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public Character? Character { get; set; }
    
    public int EnemyId { get; set; }
    public Enemy? Enemy { get; set; }
    
    public BattleResult Result { get; set; }
    
    /// <summary>
    /// Turn-by-turn battle log (JSON format)
    /// </summary>
    public string BattleNarrative { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of rounds the battle lasted
    /// </summary>
    public int RoundsCount { get; set; }
    
    // Stats at time of battle
    public int CharacterPowerAtBattle { get; set; }
    public int EnemyPowerAtBattle { get; set; }
    
    // Rewards earned
    public int ExperienceGained { get; set; }
    public int GoldGained { get; set; }
    public int ReputationGained { get; set; }
    
    // Damage taken
    public int HealthLost { get; set; }
    public int EnergySpent { get; set; }
    
    public int Year { get; set; }
    public int Month { get; set; }
    public int Turn { get; set; }
    
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
