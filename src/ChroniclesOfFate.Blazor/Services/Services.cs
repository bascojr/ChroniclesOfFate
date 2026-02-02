using Blazored.LocalStorage;
using ChroniclesOfFate.Core.DTOs;
using ChroniclesOfFate.Core.Enums;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace ChroniclesOfFate.Blazor.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(string username, string password);
    Task<AuthResponseDto?> RegisterAsync(string username, string email, string password);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
}

public interface IGameApiService
{
    Task<List<GameSessionListDto>> GetSessionsAsync();
    Task<GameSessionDto?> CreateSessionAsync(string name, string characterName, CharacterClass characterClass, List<int>? storybookIds = null);
    Task<GameStateDto?> GetGameStateAsync(int sessionId);
    Task<TurnResultDto?> ProcessTurnAsync(int sessionId, ActionType action, int? targetId = null);
    Task<EventChoiceResultDto?> ProcessEventChoiceAsync(int sessionId, int eventId, int choiceId);
    Task<List<TrainingScenarioDto>> GetTrainingScenariosAsync(int sessionId);
    Task<List<EnemyDto>> GetEnemiesAsync(int sessionId);
    Task<List<StorybookDto>> GetStorybooksAsync();
    Task<bool> EquipStorybookAsync(int sessionId, int storybookId, int slot);
    Task<List<MessageLogEntryDto>> GetMessageLogAsync(int sessionId);
    Task<MessageLogEntryDto?> AddMessageLogAsync(int sessionId, AddMessageLogDto dto);
    Task<List<CharacterSkillDto>> GetSkillsAsync(int sessionId);
}

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;

    public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        
        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        
        return new AuthenticationState(user);
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            keyValuePairs.TryGetValue(ClaimTypes.Role, out var roles);
            if (roles != null)
            {
                if (roles.ToString()!.Trim().StartsWith("["))
                {
                    var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString()!);
                    if (parsedRoles != null)
                        claims.AddRange(parsedRoles.Select(r => new Claim(ClaimTypes.Role, r)));
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()!));
                }
                keyValuePairs.Remove(ClaimTypes.Role);
            }

            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!)));
        }

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = (CustomAuthStateProvider)authStateProvider;
    }

    public async Task<AuthResponseDto?> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { username, password });
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result != null)
        {
            await _localStorage.SetItemAsync("authToken", result.Token);
            await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
            _authStateProvider.NotifyUserAuthentication(result.Token);
        }
        return result;
    }

    public async Task<AuthResponseDto?> RegisterAsync(string username, string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", new { username, email, password });
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result != null)
        {
            await _localStorage.SetItemAsync("authToken", result.Token);
            await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
            _authStateProvider.NotifyUserAuthentication(result.Token);
        }
        return result;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        _httpClient.DefaultRequestHeaders.Authorization = null;
        _authStateProvider.NotifyUserLogout();
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("authToken");
    }
}

public class GameApiService : IGameApiService
{
    private readonly HttpClient _httpClient;

    public GameApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<GameSessionListDto>> GetSessionsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<GameSessionListDto>>("api/game/sessions");
        return result ?? new List<GameSessionListDto>();
    }

    public async Task<GameSessionDto?> CreateSessionAsync(string name, string characterName, CharacterClass characterClass, List<int>? storybookIds = null)
    {
        var dto = new CreateGameSessionDto(name, new CreateCharacterDto(characterName, characterClass), storybookIds);
        var response = await _httpClient.PostAsJsonAsync("api/game/sessions", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<GameSessionDto>();
    }

    public async Task<GameStateDto?> GetGameStateAsync(int sessionId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<GameStateDto>($"api/game/sessions/{sessionId}/state");
        }
        catch
        {
            return null;
        }
    }

    public async Task<TurnResultDto?> ProcessTurnAsync(int sessionId, ActionType action, int? targetId = null)
    {
        var dto = new TurnActionDto(action, targetId);
        var response = await _httpClient.PostAsJsonAsync($"api/game/sessions/{sessionId}/turn", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TurnResultDto>();
    }

    public async Task<EventChoiceResultDto?> ProcessEventChoiceAsync(int sessionId, int eventId, int choiceId)
    {
        var response = await _httpClient.PostAsync($"api/game/sessions/{sessionId}/events/{eventId}/choice/{choiceId}", null);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<EventChoiceResultDto>();
    }

    public async Task<List<TrainingScenarioDto>> GetTrainingScenariosAsync(int sessionId)
    {
        var result = await _httpClient.GetFromJsonAsync<List<TrainingScenarioDto>>($"api/game/sessions/{sessionId}/training");
        return result ?? new List<TrainingScenarioDto>();
    }

    public async Task<List<EnemyDto>> GetEnemiesAsync(int sessionId)
    {
        var result = await _httpClient.GetFromJsonAsync<List<EnemyDto>>($"api/game/sessions/{sessionId}/enemies");
        return result ?? new List<EnemyDto>();
    }

    public async Task<List<StorybookDto>> GetStorybooksAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<StorybookDto>>("api/game/storybooks");
        return result ?? new List<StorybookDto>();
    }

    public async Task<bool> EquipStorybookAsync(int sessionId, int storybookId, int slot)
    {
        var dto = new EquipStorybookDto(storybookId, slot);
        var response = await _httpClient.PostAsJsonAsync($"api/game/sessions/{sessionId}/storybooks/equip", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<MessageLogEntryDto>> GetMessageLogAsync(int sessionId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<MessageLogEntryDto>>($"api/game/sessions/{sessionId}/messagelog");
            return result ?? new List<MessageLogEntryDto>();
        }
        catch
        {
            return new List<MessageLogEntryDto>();
        }
    }

    public async Task<MessageLogEntryDto?> AddMessageLogAsync(int sessionId, AddMessageLogDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/game/sessions/{sessionId}/messagelog", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<MessageLogEntryDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<CharacterSkillDto>> GetSkillsAsync(int sessionId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<CharacterSkillDto>>($"api/game/sessions/{sessionId}/skills");
            return result ?? new List<CharacterSkillDto>();
        }
        catch
        {
            return new List<CharacterSkillDto>();
        }
    }
}
