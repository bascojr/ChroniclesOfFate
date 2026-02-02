using ChroniclesOfFate.Core.Enums;

namespace ChroniclesOfFate.Core.Entities;

/// <summary>
/// Represents a game save/session
/// </summary>
public class GameSession
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public GameState State { get; set; } = GameState.NewGame;
    public int? FinalScore { get; set; }
    public string? EndingType { get; set; }
    
    // Relationships
    public Character? Character { get; set; }
    public ICollection<Storybook> UnlockedStorybooks { get; set; } = new List<Storybook>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
