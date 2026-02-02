using ChroniclesOfFate.Core.DTOs;
using ChroniclesOfFate.Core.Entities;
using ChroniclesOfFate.Core.Enums;
using System.Security.Claims;

namespace ChroniclesOfFate.Core.Interfaces;

/// <summary>
/// JWT token service
/// </summary>
public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

/// <summary>
/// Authentication service
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto dto);
    Task<bool> RevokeTokenAsync(string userId);
}

/// <summary>
/// Game session management service
/// </summary>
public interface IGameSessionService
{
    Task<GameSessionDto> CreateSessionAsync(string userId, CreateGameSessionDto dto);
    Task<GameSessionDto?> GetSessionAsync(int sessionId, string userId);
    Task<IEnumerable<GameSessionListDto>> GetUserSessionsAsync(string userId);
    Task<bool> DeleteSessionAsync(int sessionId, string userId);
    Task<GameStateDto> GetGameStateAsync(int sessionId, string userId);
}

/// <summary>
/// Character management service
/// </summary>
public interface ICharacterService
{
    Task<CharacterDto> CreateCharacterAsync(CreateCharacterDto dto, int sessionId, List<int>? storybookIds = null);
    Task<CharacterDto?> GetCharacterAsync(int characterId);
    Task<CharacterDto> ApplyClassBonusesAsync(Character character);
    Task<int> CalculateFinalScoreAsync(int characterId);
}

/// <summary>
/// Core game turn processing service
/// </summary>
public interface ITurnService
{
    Task<TurnResultDto> ProcessTurnAsync(int sessionId, TurnActionDto action);
    Task<TrainingResultDto> ProcessTrainingAsync(int characterId, int scenarioId);
    Task<TurnResultDto> ProcessRestAsync(int characterId);
    Task<TurnResultDto> ProcessExploreAsync(int characterId);
}

/// <summary>
/// Random event service
/// </summary>
public interface IRandomEventService
{
    Task<RandomEventDto?> TryTriggerEventAsync(
        int characterId,
        ActionType action,
        IEnumerable<int> equippedStorybookIds,
        bool preferHigherRarity = false);

    Task<EventChoiceResultDto> ProcessChoiceAsync(
        int characterId,
        int eventId,
        int choiceId);

    Task<IEnumerable<RandomEventDto>> GetAvailableEventsAsync(
        int characterId,
        ActionType action);
}

/// <summary>
/// Auto-battle simulation service
/// </summary>
public interface IBattleService
{
    Task<BattleResultDto> SimulateBattleAsync(int characterId, int enemyId);
    Task<BattleResultDto> SimulateRandomBattleAsync(int characterId);
    Task<IEnumerable<EnemyDto>> GetAvailableEnemiesAsync(int characterId);
    int CalculateCombatPower(Character character);
    int CalculateEnemyPower(Enemy enemy);
}

/// <summary>
/// Storybook management service
/// </summary>
public interface IStorybookService
{
    Task<IEnumerable<StorybookDto>> GetAllStorybooksAsync();
    Task<IEnumerable<StorybookDto>> GetUnlockedStorybooksAsync(int sessionId);
    Task<IEnumerable<StorybookDto>> GetEquippedStorybooksAsync(int characterId);
    Task<bool> EquipStorybookAsync(int characterId, int storybookId, int slot);
    Task<bool> UnequipStorybookAsync(int characterId, int slot);
    Task<bool> SetLoadoutAsync(int characterId, LoadoutDto loadout);
    Task<bool> UnlockStorybookAsync(int sessionId, int storybookId);
}

/// <summary>
/// Training service
/// </summary>
public interface ITrainingService
{
    Task<IEnumerable<TrainingScenarioDto>> GetAvailableScenariosAsync(int characterId);
    Task<TrainingResultDto> ExecuteTrainingAsync(int characterId, int scenarioId);
    double CalculateSeasonalBonus(TrainingScenario scenario, Season season);
}

/// <summary>
/// Random number generation service (for testability)
/// </summary>
public interface IRandomService
{
    int Next(int maxValue);
    int Next(int minValue, int maxValue);
    double NextDouble();
    bool RollChance(double probability);
    int RollDice(int sides, int count = 1);
}

/// <summary>
/// Game progression and ending calculation
/// </summary>
public interface IProgressionService
{
    Task<int> CalculateFinalScoreAsync(Character character);
    Task<string> DetermineEndingAsync(Character character);
    Task<bool> CheckLevelUpAsync(Character character);
    int GetExperienceForLevel(int level);
}
