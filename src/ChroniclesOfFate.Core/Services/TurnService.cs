using ChroniclesOfFate.Core.DTOs;
using ChroniclesOfFate.Core.Entities;
using ChroniclesOfFate.Core.Enums;
using ChroniclesOfFate.Core.Interfaces;

namespace ChroniclesOfFate.Core.Services;

/// <summary>
/// Core game loop - processes each turn/month
/// </summary>
public class TurnService : ITurnService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITrainingService _trainingService;
    private readonly IRandomEventService _eventService;
    private readonly IBattleService _battleService;
    private readonly IRandomService _random;

    public TurnService(
        IUnitOfWork unitOfWork,
        ITrainingService trainingService,
        IRandomEventService eventService,
        IBattleService battleService,
        IRandomService random)
    {
        _unitOfWork = unitOfWork;
        _trainingService = trainingService;
        _eventService = eventService;
        _battleService = battleService;
        _random = random;
    }

    public async Task<TurnResultDto> ProcessTurnAsync(int sessionId, TurnActionDto action)
    {
        var session = await _unitOfWork.GameSessions.GetWithCharacterAsync(sessionId)
            ?? throw new InvalidOperationException("Game session not found");

        var character = session.Character
            ?? throw new InvalidOperationException("Character not found");

        if (character.IsGameComplete)
        {
            return CreateErrorResult(character, "The 10-year journey has ended. Your story is complete!");
        }

        // Process the action
        TurnResultDto result = action.Action switch
        {
            ActionType.Train => await ProcessTrainingTurnAsync(character, action.TargetId),
            ActionType.Rest => await ProcessRestAsync(character.Id),
            ActionType.Explore => await ProcessExploreAsync(character.Id),
            ActionType.Battle => await ProcessBattleTurnAsync(character, action.TargetId),
            ActionType.Study => await ProcessStudyAsync(character),
            _ => CreateErrorResult(character, "Unknown action type")
        };

        // Advance turn if action was successful
        if (result.Success)
        {
            character.AdvanceTurn();
            
            // Check for game completion
            if (character.IsGameComplete)
            {
                session.State = GameState.Completed;
                session.CompletedAt = DateTime.UtcNow;
                await _unitOfWork.GameSessions.UpdateAsync(session);
            }

            await _unitOfWork.Characters.UpdateAsync(character);
            await _unitOfWork.SaveChangesAsync();
        }

        return result;
    }

    public async Task<TrainingResultDto> ProcessTrainingAsync(int characterId, int scenarioId)
    {
        return await _trainingService.ExecuteTrainingAsync(characterId, scenarioId);
    }

    public async Task<TurnResultDto> ProcessRestAsync(int characterId)
    {
        var character = await _unitOfWork.Characters.GetWithStorybooksAsync(characterId)
            ?? throw new InvalidOperationException("Character not found");

        var statChanges = new List<StatChangeDto>();
        var narrativeParts = new List<string>();

        // Energy recovery based on endurance
        int energyRecovery = 30 + (character.Endurance / 20);
        int oldEnergy = character.CurrentEnergy;
        character.CurrentEnergy = Math.Min(character.MaxEnergy, character.CurrentEnergy + energyRecovery);
        statChanges.Add(new StatChangeDto("Energy", oldEnergy, character.CurrentEnergy, character.CurrentEnergy - oldEnergy));

        // Health recovery
        int healthRecovery = 10 + (character.Endurance / 30);
        int oldHealth = character.CurrentHealth;
        character.CurrentHealth = Math.Min(character.MaxHealth, character.CurrentHealth + healthRecovery);
        statChanges.Add(new StatChangeDto("Health", oldHealth, character.CurrentHealth, character.CurrentHealth - oldHealth));

        narrativeParts.Add(GenerateRestNarrative(character.CurrentSeason));

        // Small chance of random event during rest
        RandomEventDto? triggeredEvent = null;
        var storybookIds = character.EquippedStorybooks.Select(es => es.StorybookId).ToList();
        if (_random.RollChance(0.15))
        {
            triggeredEvent = await _eventService.TryTriggerEventAsync(characterId, ActionType.Rest, storybookIds);
            if (triggeredEvent != null)
            {
                narrativeParts.Add($"During your rest, something unexpected happens: {triggeredEvent.Title}");
            }
        }

        await _unitOfWork.Characters.UpdateAsync(character);
        await _unitOfWork.SaveChangesAsync();

        return new TurnResultDto(
            true,
            string.Join(" ", narrativeParts),
            statChanges,
            triggeredEvent,
            null,
            await GetCharacterDtoAsync(character)
        );
    }

    public async Task<TurnResultDto> ProcessExploreAsync(int characterId)
    {
        var character = await _unitOfWork.Characters.GetWithStorybooksAsync(characterId)
            ?? throw new InvalidOperationException("Character not found");

        const int exploreCost = 15;
        if (character.CurrentEnergy < exploreCost)
        {
            return CreateErrorResult(character, "Not enough energy to explore. Rest first.");
        }

        var statChanges = new List<StatChangeDto>();
        var narrativeParts = new List<string>();

        // Spend energy
        int oldEnergy = character.CurrentEnergy;
        character.CurrentEnergy -= exploreCost;
        statChanges.Add(new StatChangeDto("Energy", oldEnergy, character.CurrentEnergy, -exploreCost));

        narrativeParts.Add(GenerateExploreNarrative(character.CurrentSeason));

        // High chance of random event while exploring
        RandomEventDto? triggeredEvent = null;
        var storybookIds = character.EquippedStorybooks.Select(es => es.StorybookId).ToList();
        
        triggeredEvent = await _eventService.TryTriggerEventAsync(characterId, ActionType.Explore, storybookIds);
        
        if (triggeredEvent != null)
        {
            narrativeParts.Add($"You encounter: {triggeredEvent.Title}");
        }
        else
        {
            // Default exploration rewards if no event
            int goldFound = _random.Next(5, 20) + (character.Luck / 20);
            int oldGold = character.Gold;
            character.Gold += goldFound;
            statChanges.Add(new StatChangeDto("Gold", oldGold, character.Gold, goldFound));
            narrativeParts.Add($"You find {goldFound} gold during your exploration.");

            // Small chance of stat gain
            if (_random.RollChance(0.2))
            {
                var randomStat = (StatType)_random.Next(6);
                int gain = _random.Next(1, 4);
                int oldStat = character.GetStat(randomStat);
                character.AddStat(randomStat, gain);
                statChanges.Add(new StatChangeDto(randomStat.ToString(), oldStat, character.GetStat(randomStat), gain));
                narrativeParts.Add($"Your {randomStat} improved slightly from the experience.");
            }
        }

        await _unitOfWork.Characters.UpdateAsync(character);
        await _unitOfWork.SaveChangesAsync();

        return new TurnResultDto(
            true,
            string.Join(" ", narrativeParts),
            statChanges,
            triggeredEvent,
            null,
            await GetCharacterDtoAsync(character)
        );
    }

    private async Task<TurnResultDto> ProcessTrainingTurnAsync(Character character, int? scenarioId)
    {
        if (!scenarioId.HasValue)
        {
            return CreateErrorResult(character, "No training scenario selected.");
        }

        var trainingResult = await _trainingService.ExecuteTrainingAsync(character.Id, scenarioId.Value);

        // Check for random event after training
        RandomEventDto? triggeredEvent = null;
        var storybookIds = character.EquippedStorybooks.Select(es => es.StorybookId).ToList();
        
        if (trainingResult.Success && _random.RollChance(0.25))
        {
            triggeredEvent = await _eventService.TryTriggerEventAsync(character.Id, ActionType.Train, storybookIds);
        }

        return new TurnResultDto(
            trainingResult.Success,
            trainingResult.Narrative,
            trainingResult.StatChanges,
            triggeredEvent,
            null,
            await GetCharacterDtoAsync(character)
        );
    }

    private async Task<TurnResultDto> ProcessBattleTurnAsync(Character character, int? enemyId)
    {
        BattleResultDto battleResult;
        
        if (enemyId.HasValue)
        {
            battleResult = await _battleService.SimulateBattleAsync(character.Id, enemyId.Value);
        }
        else
        {
            battleResult = await _battleService.SimulateRandomBattleAsync(character.Id);
        }

        var statChanges = new List<StatChangeDto>();
        if (battleResult.ExperienceGained > 0)
            statChanges.Add(new StatChangeDto("Experience", 0, battleResult.ExperienceGained, battleResult.ExperienceGained));
        if (battleResult.GoldGained > 0)
            statChanges.Add(new StatChangeDto("Gold", 0, battleResult.GoldGained, battleResult.GoldGained));
        if (battleResult.HealthLost > 0)
            statChanges.Add(new StatChangeDto("Health", 0, -battleResult.HealthLost, -battleResult.HealthLost));

        return new TurnResultDto(
            battleResult.Result != BattleResult.Fled,
            battleResult.Narrative,
            statChanges,
            null,
            battleResult,
            await GetCharacterDtoAsync(character)
        );
    }

    private async Task<TurnResultDto> ProcessStudyAsync(Character character)
    {
        const int studyCost = 20;
        if (character.CurrentEnergy < studyCost)
        {
            return CreateErrorResult(character, "Not enough energy to study. Rest first.");
        }

        var statChanges = new List<StatChangeDto>();

        // Spend energy
        int oldEnergy = character.CurrentEnergy;
        character.CurrentEnergy -= studyCost;
        statChanges.Add(new StatChangeDto("Energy", oldEnergy, character.CurrentEnergy, -studyCost));

        // Intelligence gain
        int intGain = 3 + _random.Next(1, 4);
        int oldInt = character.Intelligence;
        character.AddStat(StatType.Intelligence, intGain);
        statChanges.Add(new StatChangeDto("Intelligence", oldInt, character.Intelligence, intGain));

        // Experience gain
        int expGain = 15;
        character.Experience += expGain;
        statChanges.Add(new StatChangeDto("Experience", character.Experience - expGain, character.Experience, expGain));

        await _unitOfWork.Characters.UpdateAsync(character);
        await _unitOfWork.SaveChangesAsync();

        string[] narratives =
        {
            "You spend the month studying ancient tomes and arcane knowledge.",
            "Hours of focused study expand your understanding of the world.",
            "You practice mental exercises and meditation, sharpening your mind.",
            "Time spent in the library yields new insights and knowledge."
        };

        return new TurnResultDto(
            true,
            narratives[_random.Next(narratives.Length)],
            statChanges,
            null,
            null,
            await GetCharacterDtoAsync(character)
        );
    }

    private string GenerateRestNarrative(Season season)
    {
        return season switch
        {
            Season.Spring => "You rest among blooming flowers, feeling rejuvenated by the gentle spring breeze.",
            Season.Summer => "You find a cool, shaded spot to rest and recover from the summer heat.",
            Season.Autumn => "The crisp autumn air refreshes you as you take time to recover.",
            Season.Winter => "You rest by a warm fire, sheltered from the winter cold.",
            _ => "You take a well-deserved rest to recover your strength."
        };
    }

    private string GenerateExploreNarrative(Season season)
    {
        return season switch
        {
            Season.Spring => "You venture out into the awakening world, where new paths reveal themselves.",
            Season.Summer => "Under the bright summer sun, you explore distant lands and hidden places.",
            Season.Autumn => "You wander through forests painted in gold and crimson, seeking adventure.",
            Season.Winter => "Braving the cold, you explore snow-covered landscapes and frozen paths.",
            _ => "You set out to explore the unknown, ready for whatever you might find."
        };
    }

    private TurnResultDto CreateErrorResult(Character character, string message)
    {
        return new TurnResultDto(
            false,
            message,
            new List<StatChangeDto>(),
            null,
            null,
            MapToCharacterDto(character)
        );
    }

    private async Task<CharacterDto> GetCharacterDtoAsync(Character character)
    {
        var refreshed = await _unitOfWork.Characters.GetWithStorybooksAsync(character.Id);
        return MapToCharacterDto(refreshed ?? character);
    }

    private CharacterDto MapToCharacterDto(Character c)
    {
        var storybooks = c.EquippedStorybooks?
            .Where(es => es.Storybook != null)
            .Select(es => new StorybookDto(
                es.Storybook!.Id,
                es.Storybook.Name,
                es.Storybook.Description,
                es.Storybook.IconUrl,
                es.Storybook.Rarity,
                es.Storybook.Theme,
                es.Storybook.StrengthBonus,
                es.Storybook.AgilityBonus,
                es.Storybook.IntelligenceBonus,
                es.Storybook.EnduranceBonus,
                es.Storybook.CharismaBonus,
                es.Storybook.LuckBonus,
                es.Storybook.EventTriggerChance
            )).ToList() ?? new List<StorybookDto>();

        return new CharacterDto(
            c.Id,
            c.Name,
            c.Class,
            c.Strength,
            c.Agility,
            c.Intelligence,
            c.Endurance,
            c.Charisma,
            c.Luck,
            c.CurrentEnergy,
            c.MaxEnergy,
            c.CurrentHealth,
            c.MaxHealth,
            c.Level,
            c.Experience,
            c.Gold,
            c.Reputation,
            c.CurrentYear,
            c.CurrentMonth,
            c.TotalTurns,
            c.CurrentSeason,
            c.TotalPower,
            c.IsGameComplete,
            storybooks
        );
    }
}
