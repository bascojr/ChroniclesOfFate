namespace ChroniclesOfFate.Core.Entities;

/// <summary>
/// Represents a message log entry for a game session
/// </summary>
public class MessageLogEntry
{
    public int Id { get; set; }
    public int GameSessionId { get; set; }
    public GameSession? GameSession { get; set; }

    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "action"; // action, positive, negative
    public int Year { get; set; }
    public int Month { get; set; }

    /// <summary>
    /// JSON serialized stat changes
    /// </summary>
    public string? StatChangesJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
