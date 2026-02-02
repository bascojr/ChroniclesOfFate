using ChroniclesOfFate.Core.DTOs;
using ChroniclesOfFate.Core.Entities;
using ChroniclesOfFate.Core.Enums;
using ChroniclesOfFate.Core.Interfaces;

namespace ChroniclesOfFate.Infrastructure.Services;

public class GameSessionService : IGameSessionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICharacterService _characterService;
    private readonly ITrainingService _trainingService;
    private readonly IStorybookService _storybookService;

    public GameSessionService(
        IUnitOfWork unitOfWork,
        ICharacterService characterService,
        ITrainingService trainingService,
        IStorybookService storybookService)
    {
        _unitOfWork = unitOfWork;
        _characterService = characterService;
        _trainingService = trainingService;
        _storybookService = storybookService;
    }

    public async Task<GameSessionDto> CreateSessionAsync(string userId, CreateGameSessionDto dto)
    {
        var session = new GameSession
        {
            UserId = userId,
            SessionName = dto.SessionName,
            State = GameState.InProgress
        };

        await _unitOfWork.GameSessions.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        var character = await _characterService.CreateCharacterAsync(dto.Character, session.Id, dto.SelectedStorybookIds);
        session = await _unitOfWork.GameSessions.GetWithCharacterAsync(session.Id);

        return MapToSessionDto(session!);
    }

    public async Task<GameSessionDto?> GetSessionAsync(int sessionId, string userId)
    {
        var session = await _unitOfWork.GameSessions.GetWithCharacterAsync(sessionId);
        if (session == null || session.UserId != userId)
            return null;
        return MapToSessionDto(session);
    }

    public async Task<IEnumerable<GameSessionListDto>> GetUserSessionsAsync(string userId)
    {
        var sessions = await _unitOfWork.GameSessions.GetByUserIdAsync(userId);
        return sessions.Select(s => new GameSessionListDto(
            s.Id, s.SessionName, s.State,
            s.Character?.Name, s.Character?.Level, s.Character?.TotalTurns,
            s.UpdatedAt
        ));
    }

    public async Task<bool> DeleteSessionAsync(int sessionId, string userId)
    {
        var session = await _unitOfWork.GameSessions.GetByIdAsync(sessionId);
        if (session == null || session.UserId != userId)
            return false;

        await _unitOfWork.GameSessions.DeleteAsync(session);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<GameStateDto> GetGameStateAsync(int sessionId, string userId)
    {
        var session = await _unitOfWork.GameSessions.GetFullSessionAsync(sessionId);
        if (session == null || session.UserId != userId)
            throw new InvalidOperationException("Session not found");

        var character = session.Character ?? throw new InvalidOperationException("Character not found");

        var training = await _trainingService.GetAvailableScenariosAsync(character.Id);
        var equipped = await _storybookService.GetEquippedStorybooksAsync(character.Id);
        var available = await _storybookService.GetUnlockedStorybooksAsync(sessionId);

        var seasonInfo = new SeasonInfoDto(
            character.CurrentSeason,
            character.CurrentSeason.ToString(),
            GetSeasonDescription(character.CurrentSeason),
            GetSeasonBonuses(character.CurrentSeason)
        );

        var actions = new List<string> { "Train", "Rest", "Explore", "Battle" };

        return new GameStateDto(
            MapToCharacterDto(character),
            training.ToList(),
            equipped.ToList(),
            available.ToList(),
            seasonInfo,
            actions
        );
    }

    private static string GetSeasonDescription(Season season) => season switch
    {
        Season.Spring => "A time of renewal. Nature awakens and new opportunities arise.",
        Season.Summer => "The sun shines bright. Perfect for physical training and exploration.",
        Season.Autumn => "Harvest season. A time for reflection and magical studies.",
        Season.Winter => "Cold winds blow. Rest well and prepare for challenges ahead.",
        _ => "Another month in your journey."
    };

    private static List<string> GetSeasonBonuses(Season season) => season switch
    {
        Season.Spring => new List<string> { "Endurance training +20%", "Exploration rewards +10%" },
        Season.Summer => new List<string> { "Endurance training +20%", "Battle experience +15%" },
        Season.Autumn => new List<string> { "Arcane Studies +25%", "Event chance +10%" },
        Season.Winter => new List<string> { "Rest recovery +20%", "Intelligence training +15%" },
        _ => new List<string>()
    };

    private static GameSessionDto MapToSessionDto(GameSession s) => new(
        s.Id, s.SessionName, s.State, s.FinalScore, s.EndingType,
        s.Character != null ? MapToCharacterDto(s.Character) : null,
        s.CreatedAt, s.UpdatedAt
    );

    private static CharacterDto MapToCharacterDto(Character c)
    {
        var storybooks = c.EquippedStorybooks?.Where(es => es.Storybook != null)
            .Select(es => MapStorybookToDto(es.Storybook!)).ToList() ?? new List<StorybookDto>();

        return new CharacterDto(
            c.Id, c.Name, c.Class,
            c.Strength, c.Agility, c.Intelligence, c.Endurance, c.Charisma, c.Luck,
            c.CurrentEnergy, c.MaxEnergy, c.CurrentHealth, c.MaxHealth,
            c.Level, c.Experience, c.Gold, c.Reputation,
            c.CurrentYear, c.CurrentMonth, c.TotalTurns,
            c.CurrentSeason, c.TotalPower, c.IsGameComplete, storybooks
        );
    }

    private static StorybookDto MapStorybookToDto(Storybook s) => new(
        s.Id, s.Name, s.Description, s.IconUrl, s.Theme,
        s.StrengthBonus, s.AgilityBonus, s.IntelligenceBonus,
        s.EnduranceBonus, s.CharismaBonus, s.LuckBonus, s.EventTriggerChance,
        s.Events?.Select(e => new StorybookEventSummaryDto(e.Title, e.Description, e.Rarity)).ToList()
            ?? new List<StorybookEventSummaryDto>()
    );
}

public class CharacterService : ICharacterService
{
    private readonly IUnitOfWork _unitOfWork;

    public CharacterService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CharacterDto> CreateCharacterAsync(CreateCharacterDto dto, int sessionId, List<int>? storybookIds = null)
    {
        var character = new Character
        {
            Name = dto.Name,
            Class = dto.Class,
            GameSessionId = sessionId,
            CurrentEnergy = 100,
            CurrentHealth = 100
        };

        // Apply class bonuses
        ApplyClassBonuses(character);

        await _unitOfWork.Characters.AddAsync(character);
        await _unitOfWork.SaveChangesAsync();

        // Equip selected storybooks and apply initial stat bonuses
        if (storybookIds != null && storybookIds.Any())
        {
            var validIds = storybookIds.Take(5).ToList(); // Max 5 storybooks
            int slot = 1;

            foreach (var storybookId in validIds)
            {
                var storybook = await _unitOfWork.Storybooks.GetByIdAsync(storybookId);
                if (storybook == null) continue;

                // Equip the storybook
                character.EquippedStorybooks.Add(new CharacterStorybook
                {
                    CharacterId = character.Id,
                    StorybookId = storybookId,
                    SlotPosition = slot++
                });

                // Apply initial stat bonuses from storybook (these are permanent starting boosts)
                character.Strength += storybook.StrengthBonus;
                character.Agility += storybook.AgilityBonus;
                character.Intelligence += storybook.IntelligenceBonus;
                character.Endurance += storybook.EnduranceBonus;
                character.Charisma += storybook.CharismaBonus;
                character.Luck += storybook.LuckBonus;
            }

            await _unitOfWork.Characters.UpdateAsync(character);
            await _unitOfWork.SaveChangesAsync();
        }

        return await ApplyClassBonusesAsync(character);
    }

    public async Task<CharacterDto?> GetCharacterAsync(int characterId)
    {
        var character = await _unitOfWork.Characters.GetWithStorybooksAsync(characterId);
        if (character == null) return null;

        return MapToDto(character);
    }

    public Task<CharacterDto> ApplyClassBonusesAsync(Character character)
    {
        return Task.FromResult(MapToDto(character));
    }

    public async Task<int> CalculateFinalScoreAsync(int characterId)
    {
        var character = await _unitOfWork.Characters.GetWithFullDetailsAsync(characterId);
        if (character == null) return 0;

        int score = character.TotalPower * 10;
        score += character.Level * 100;
        score += character.Gold / 10;
        score += character.Reputation * 5;

        var victories = await _unitOfWork.BattleLogs.GetVictoryCountAsync(characterId);
        score += victories * 50;

        return score;
    }

    private void ApplyClassBonuses(Character c)
    {
        switch (c.Class)
        {
            case CharacterClass.Warrior:
                c.Strength = 20; c.Agility = 12; c.Intelligence = 8;
                c.Endurance = 18; c.Charisma = 10; c.Luck = 12;
                break;
            case CharacterClass.Mage:
                c.Strength = 8; c.Agility = 12; c.Intelligence = 22;
                c.Endurance = 10; c.Charisma = 14; c.Luck = 14;
                break;
            case CharacterClass.Rogue:
                c.Strength = 12; c.Agility = 22; c.Intelligence = 14;
                c.Endurance = 10; c.Charisma = 12; c.Luck = 18;
                break;
            case CharacterClass.Cleric:
                c.Strength = 10; c.Agility = 10; c.Intelligence = 18;
                c.Endurance = 16; c.Charisma = 16; c.Luck = 10;
                break;
            case CharacterClass.Ranger:
                c.Strength = 14; c.Agility = 20; c.Intelligence = 12;
                c.Endurance = 14; c.Charisma = 10; c.Luck = 14;
                break;
        }
    }

    private static CharacterDto MapToDto(Character c)
    {
        var storybooks = c.EquippedStorybooks?.Where(es => es.Storybook != null)
            .Select(es => MapStorybookDto(es.Storybook!)).ToList() ?? new List<StorybookDto>();

        return new CharacterDto(
            c.Id, c.Name, c.Class,
            c.Strength, c.Agility, c.Intelligence, c.Endurance, c.Charisma, c.Luck,
            c.CurrentEnergy, c.MaxEnergy, c.CurrentHealth, c.MaxHealth,
            c.Level, c.Experience, c.Gold, c.Reputation,
            c.CurrentYear, c.CurrentMonth, c.TotalTurns,
            c.CurrentSeason, c.TotalPower, c.IsGameComplete, storybooks
        );
    }

    private static StorybookDto MapStorybookDto(Storybook s) => new(
        s.Id, s.Name, s.Description, s.IconUrl, s.Theme,
        s.StrengthBonus, s.AgilityBonus, s.IntelligenceBonus,
        s.EnduranceBonus, s.CharismaBonus, s.LuckBonus, s.EventTriggerChance,
        s.Events?.Select(e => new StorybookEventSummaryDto(e.Title, e.Description, e.Rarity)).ToList()
            ?? new List<StorybookEventSummaryDto>()
    );
}

public class StorybookService : IStorybookService
{
    private readonly IUnitOfWork _unitOfWork;

    public StorybookService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<StorybookDto>> GetAllStorybooksAsync()
    {
        var storybooks = await _unitOfWork.Storybooks.GetWithEventsAsync();
        return storybooks.Select(MapToDto);
    }

    public async Task<IEnumerable<StorybookDto>> GetUnlockedStorybooksAsync(int sessionId)
    {
        var session = await _unitOfWork.GameSessions.GetFullSessionAsync(sessionId);
        if (session == null) return Enumerable.Empty<StorybookDto>();

        var all = await _unitOfWork.Storybooks.GetWithEventsAsync();
        var unlocked = all.Where(s => !s.IsUnlockable || session.UnlockedStorybooks.Any(u => u.Id == s.Id));
        return unlocked.Select(MapToDto);
    }

    public async Task<IEnumerable<StorybookDto>> GetEquippedStorybooksAsync(int characterId)
    {
        var character = await _unitOfWork.Characters.GetWithStorybooksAsync(characterId);
        if (character == null) return Enumerable.Empty<StorybookDto>();

        return character.EquippedStorybooks
            .Where(es => es.Storybook != null)
            .OrderBy(es => es.SlotPosition)
            .Select(es => MapToDto(es.Storybook!));
    }

    public async Task<bool> EquipStorybookAsync(int characterId, int storybookId, int slot)
    {
        if (slot < 1 || slot > 5) return false;

        var character = await _unitOfWork.Characters.GetWithStorybooksAsync(characterId);
        if (character == null) return false;

        // Check if already equipped 5 books
        if (character.EquippedStorybooks.Count >= 5 &&
            !character.EquippedStorybooks.Any(es => es.SlotPosition == slot))
            return false;

        // Remove existing book in slot
        var existing = character.EquippedStorybooks.FirstOrDefault(es => es.SlotPosition == slot);
        if (existing != null)
            character.EquippedStorybooks.Remove(existing);

        // Remove if storybook is in another slot
        var sameBook = character.EquippedStorybooks.FirstOrDefault(es => es.StorybookId == storybookId);
        if (sameBook != null)
            character.EquippedStorybooks.Remove(sameBook);

        character.EquippedStorybooks.Add(new CharacterStorybook
        {
            CharacterId = characterId,
            StorybookId = storybookId,
            SlotPosition = slot
        });

        await _unitOfWork.Characters.UpdateAsync(character);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnequipStorybookAsync(int characterId, int slot)
    {
        var character = await _unitOfWork.Characters.GetWithStorybooksAsync(characterId);
        if (character == null) return false;

        var equipped = character.EquippedStorybooks.FirstOrDefault(es => es.SlotPosition == slot);
        if (equipped == null) return false;

        character.EquippedStorybooks.Remove(equipped);
        await _unitOfWork.Characters.UpdateAsync(character);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetLoadoutAsync(int characterId, LoadoutDto loadout)
    {
        if (loadout.Storybooks.Count > 5) return false;

        var character = await _unitOfWork.Characters.GetWithStorybooksAsync(characterId);
        if (character == null) return false;

        character.EquippedStorybooks.Clear();

        foreach (var item in loadout.Storybooks)
        {
            if (item.SlotPosition < 1 || item.SlotPosition > 5) continue;

            character.EquippedStorybooks.Add(new CharacterStorybook
            {
                CharacterId = characterId,
                StorybookId = item.StorybookId,
                SlotPosition = item.SlotPosition
            });
        }

        await _unitOfWork.Characters.UpdateAsync(character);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnlockStorybookAsync(int sessionId, int storybookId)
    {
        var session = await _unitOfWork.GameSessions.GetFullSessionAsync(sessionId);
        var storybook = await _unitOfWork.Storybooks.GetByIdAsync(storybookId);

        if (session == null || storybook == null) return false;
        if (session.UnlockedStorybooks.Any(s => s.Id == storybookId)) return false;

        session.UnlockedStorybooks.Add(storybook);
        await _unitOfWork.GameSessions.UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static StorybookDto MapToDto(Storybook s) => new(
        s.Id, s.Name, s.Description, s.IconUrl, s.Theme,
        s.StrengthBonus, s.AgilityBonus, s.IntelligenceBonus,
        s.EnduranceBonus, s.CharismaBonus, s.LuckBonus, s.EventTriggerChance,
        s.Events?.Select(e => new StorybookEventSummaryDto(e.Title, e.Description, e.Rarity)).ToList()
            ?? new List<StorybookEventSummaryDto>()
    );
}

public class ProgressionService : IProgressionService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProgressionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> CalculateFinalScoreAsync(Character character)
    {
        int score = character.TotalPower * 10;
        score += character.Level * 100;
        score += character.Gold / 10;
        score += character.Reputation * 5;

        var victories = await _unitOfWork.BattleLogs.GetVictoryCountAsync(character.Id);
        score += victories * 50;

        return score;
    }

    public Task<string> DetermineEndingAsync(Character character)
    {
        string ending;
        int totalStats = character.TotalPower;

        if (totalStats >= 500 && character.Reputation >= 100)
            ending = "Legendary Hero";
        else if (totalStats >= 400)
            ending = "Renowned Champion";
        else if (totalStats >= 300)
            ending = "Accomplished Adventurer";
        else if (totalStats >= 200)
            ending = "Seasoned Traveler";
        else
            ending = "Humble Wanderer";

        return Task.FromResult(ending);
    }

    public async Task<bool> CheckLevelUpAsync(Character character)
    {
        int requiredExp = GetExperienceForLevel(character.Level + 1);
        if (character.Experience < requiredExp) return false;

        character.Level++;
        character.MaxHealth += 10;
        character.MaxEnergy += 5;
        character.CurrentHealth = character.MaxHealth;
        character.CurrentEnergy = character.MaxEnergy;

        await _unitOfWork.Characters.UpdateAsync(character);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public int GetExperienceForLevel(int level)
    {
        return level * level * 50;
    }
}
