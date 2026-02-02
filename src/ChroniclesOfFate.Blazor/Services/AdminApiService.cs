using ChroniclesOfFate.Core.DTOs;
using System.Net.Http.Json;

namespace ChroniclesOfFate.Blazor.Services;

public interface IAdminApiService
{
    // Dashboard
    Task<AdminDashboardDto?> GetDashboardAsync();

    // Lookups
    Task<List<LookupDto>> GetStorybookLookupAsync();
    Task<List<LookupDto>> GetSkillLookupAsync();
    Task<List<LookupDto>> GetEnemyLookupAsync();
    Task<List<LookupDto>> GetRandomEventLookupAsync();

    // Training Scenarios
    Task<List<AdminTrainingScenarioDto>> GetAllTrainingScenariosAsync();
    Task<AdminTrainingScenarioDto?> GetTrainingScenarioAsync(int id);
    Task<AdminTrainingScenarioDto?> CreateTrainingScenarioAsync(CreateTrainingScenarioDto dto);
    Task<AdminTrainingScenarioDto?> UpdateTrainingScenarioAsync(int id, UpdateTrainingScenarioDto dto);
    Task<bool> DeleteTrainingScenarioAsync(int id);

    // Enemies
    Task<List<AdminEnemyDto>> GetAllEnemiesAsync();
    Task<AdminEnemyDto?> GetEnemyAsync(int id);
    Task<AdminEnemyDto?> CreateEnemyAsync(CreateEnemyDto dto);
    Task<AdminEnemyDto?> UpdateEnemyAsync(int id, UpdateEnemyDto dto);
    Task<bool> DeleteEnemyAsync(int id);

    // Storybooks
    Task<List<AdminStorybookDto>> GetAllStorybooksAsync();
    Task<AdminStorybookDto?> GetStorybookAsync(int id);
    Task<AdminStorybookDto?> CreateStorybookAsync(CreateStorybookDto dto);
    Task<AdminStorybookDto?> UpdateStorybookAsync(int id, UpdateStorybookDto dto);
    Task<bool> DeleteStorybookAsync(int id);

    // Random Events
    Task<List<AdminRandomEventDto>> GetAllRandomEventsAsync();
    Task<AdminRandomEventDto?> GetRandomEventAsync(int id);
    Task<AdminRandomEventDto?> CreateRandomEventAsync(CreateRandomEventDto dto);
    Task<AdminRandomEventDto?> UpdateRandomEventAsync(int id, UpdateRandomEventDto dto);
    Task<bool> DeleteRandomEventAsync(int id);

    // Event Choices
    Task<List<AdminEventChoiceDto>> GetEventChoicesAsync(int eventId);
    Task<AdminEventChoiceDto?> GetEventChoiceAsync(int id);
    Task<AdminEventChoiceDto?> CreateEventChoiceAsync(CreateEventChoiceDto dto);
    Task<AdminEventChoiceDto?> UpdateEventChoiceAsync(int id, UpdateEventChoiceDto dto);
    Task<bool> DeleteEventChoiceAsync(int id);

    // Skills
    Task<List<AdminSkillDto>> GetAllSkillsAsync();
    Task<AdminSkillDto?> GetSkillAsync(int id);
    Task<AdminSkillDto?> CreateSkillAsync(CreateSkillDto dto);
    Task<AdminSkillDto?> UpdateSkillAsync(int id, UpdateSkillDto dto);
    Task<bool> DeleteSkillAsync(int id);
}

public class AdminApiService : IAdminApiService
{
    private readonly HttpClient _httpClient;

    public AdminApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ============ Dashboard ============

    public async Task<AdminDashboardDto?> GetDashboardAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AdminDashboardDto>("api/admin/dashboard");
        }
        catch
        {
            return null;
        }
    }

    // ============ Lookups ============

    public async Task<List<LookupDto>> GetStorybookLookupAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<LookupDto>>("api/admin/lookups/storybooks");
            return result ?? new List<LookupDto>();
        }
        catch
        {
            return new List<LookupDto>();
        }
    }

    public async Task<List<LookupDto>> GetSkillLookupAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<LookupDto>>("api/admin/lookups/skills");
            return result ?? new List<LookupDto>();
        }
        catch
        {
            return new List<LookupDto>();
        }
    }

    public async Task<List<LookupDto>> GetEnemyLookupAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<LookupDto>>("api/admin/lookups/enemies");
            return result ?? new List<LookupDto>();
        }
        catch
        {
            return new List<LookupDto>();
        }
    }

    public async Task<List<LookupDto>> GetRandomEventLookupAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<LookupDto>>("api/admin/lookups/events");
            return result ?? new List<LookupDto>();
        }
        catch
        {
            return new List<LookupDto>();
        }
    }

    // ============ Training Scenarios ============

    public async Task<List<AdminTrainingScenarioDto>> GetAllTrainingScenariosAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<AdminTrainingScenarioDto>>("api/admin/training-scenarios");
            return result ?? new List<AdminTrainingScenarioDto>();
        }
        catch
        {
            return new List<AdminTrainingScenarioDto>();
        }
    }

    public async Task<AdminTrainingScenarioDto?> GetTrainingScenarioAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AdminTrainingScenarioDto>($"api/admin/training-scenarios/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<AdminTrainingScenarioDto?> CreateTrainingScenarioAsync(CreateTrainingScenarioDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/admin/training-scenarios", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AdminTrainingScenarioDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<AdminTrainingScenarioDto?> UpdateTrainingScenarioAsync(int id, UpdateTrainingScenarioDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/admin/training-scenarios/{id}", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AdminTrainingScenarioDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteTrainingScenarioAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/admin/training-scenarios/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // ============ Enemies ============

    public async Task<List<AdminEnemyDto>> GetAllEnemiesAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<AdminEnemyDto>>("api/admin/enemies");
            return result ?? new List<AdminEnemyDto>();
        }
        catch
        {
            return new List<AdminEnemyDto>();
        }
    }

    public async Task<AdminEnemyDto?> GetEnemyAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AdminEnemyDto>($"api/admin/enemies/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<AdminEnemyDto?> CreateEnemyAsync(CreateEnemyDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/admin/enemies", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AdminEnemyDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<AdminEnemyDto?> UpdateEnemyAsync(int id, UpdateEnemyDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/admin/enemies/{id}", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AdminEnemyDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteEnemyAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/admin/enemies/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // ============ Storybooks ============

    public async Task<List<AdminStorybookDto>> GetAllStorybooksAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<AdminStorybookDto>>("api/admin/storybooks");
            return result ?? new List<AdminStorybookDto>();
        }
        catch
        {
            return new List<AdminStorybookDto>();
        }
    }

    public async Task<AdminStorybookDto?> GetStorybookAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AdminStorybookDto>($"api/admin/storybooks/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<AdminStorybookDto?> CreateStorybookAsync(CreateStorybookDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/admin/storybooks", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AdminStorybookDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<AdminStorybookDto?> UpdateStorybookAsync(int id, UpdateStorybookDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/admin/storybooks/{id}", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AdminStorybookDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteStorybookAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/admin/storybooks/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // ============ Random Events ============

    public async Task<List<AdminRandomEventDto>> GetAllRandomEventsAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<AdminRandomEventDto>>("api/admin/random-events");
            return result ?? new List<AdminRandomEventDto>();
        }
        catch
        {
            return new List<AdminRandomEventDto>();
        }
    }

    public async Task<AdminRandomEventDto?> GetRandomEventAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AdminRandomEventDto>($"api/admin/random-events/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<AdminRandomEventDto?> CreateRandomEventAsync(CreateRandomEventDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/admin/random-events", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AdminRandomEventDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<AdminRandomEventDto?> UpdateRandomEventAsync(int id, UpdateRandomEventDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/admin/random-events/{id}", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AdminRandomEventDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteRandomEventAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/admin/random-events/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // ============ Event Choices ============

    public async Task<List<AdminEventChoiceDto>> GetEventChoicesAsync(int eventId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<AdminEventChoiceDto>>($"api/admin/random-events/{eventId}/choices");
            return result ?? new List<AdminEventChoiceDto>();
        }
        catch
        {
            return new List<AdminEventChoiceDto>();
        }
    }

    public async Task<AdminEventChoiceDto?> GetEventChoiceAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AdminEventChoiceDto>($"api/admin/event-choices/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<AdminEventChoiceDto?> CreateEventChoiceAsync(CreateEventChoiceDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/admin/event-choices", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AdminEventChoiceDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<AdminEventChoiceDto?> UpdateEventChoiceAsync(int id, UpdateEventChoiceDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/admin/event-choices/{id}", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AdminEventChoiceDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteEventChoiceAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/admin/event-choices/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // ============ Skills ============

    public async Task<List<AdminSkillDto>> GetAllSkillsAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<AdminSkillDto>>("api/admin/skills");
            return result ?? new List<AdminSkillDto>();
        }
        catch
        {
            return new List<AdminSkillDto>();
        }
    }

    public async Task<AdminSkillDto?> GetSkillAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AdminSkillDto>($"api/admin/skills/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<AdminSkillDto?> CreateSkillAsync(CreateSkillDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/admin/skills", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AdminSkillDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<AdminSkillDto?> UpdateSkillAsync(int id, UpdateSkillDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/admin/skills/{id}", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AdminSkillDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteSkillAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/admin/skills/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
