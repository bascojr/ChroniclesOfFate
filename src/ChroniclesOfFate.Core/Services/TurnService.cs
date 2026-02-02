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
            // Try to trigger a mini random event (20% chance)
            var miniEventResult = TryTriggerMiniEvent(character);
            if (miniEventResult != null)
            {
                // Merge mini event results into the turn result
                var updatedStatChanges = result.StatChanges.ToList();
                updatedStatChanges.AddRange(miniEventResult.StatChanges);

                var updatedNarrative = result.Narrative + " " + miniEventResult.Narrative;

                result = new TurnResultDto(
                    result.Success,
                    updatedNarrative,
                    updatedStatChanges,
                    result.TriggeredEvent,
                    result.BattleResult,
                    result.UpdatedCharacter
                );
            }

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

            // Refresh character DTO with latest values
            result = new TurnResultDto(
                result.Success,
                result.Narrative,
                result.StatChanges,
                result.TriggeredEvent,
                result.BattleResult,
                await GetCharacterDtoAsync(character)
            );
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

    private record MiniEventResult(string Narrative, List<StatChangeDto> StatChanges);

    private MiniEventResult? TryTriggerMiniEvent(Character character)
    {
        // 20% chance to trigger a mini event
        if (!_random.RollChance(0.20))
            return null;

        var statChanges = new List<StatChangeDto>();
        string narrative;

        // 70% positive, 30% negative
        bool isPositive = _random.RollChance(0.70);

        if (isPositive)
        {
            // Positive mini events
            int eventType = _random.Next(5);
            switch (eventType)
            {
                case 0: // Random stat increase
                    var randomStat = (StatType)_random.Next(6);
                    int statGain = _random.Next(2, 5);
                    int oldStat = character.GetStat(randomStat);
                    character.AddStat(randomStat, statGain);
                    statChanges.Add(new StatChangeDto(randomStat.ToString(), oldStat, character.GetStat(randomStat), statGain));
                    narrative = GetPositiveStatNarrative(randomStat, statGain);
                    break;

                case 1: // Gold found
                    int goldGain = _random.Next(10, 30) + (character.Luck / 10);
                    int oldGold = character.Gold;
                    character.Gold += goldGain;
                    statChanges.Add(new StatChangeDto("Gold", oldGold, character.Gold, goldGain));
                    narrative = GetPositiveGoldNarrative(goldGain);
                    break;

                case 2: // Energy boost
                    int energyGain = _random.Next(10, 25);
                    int oldEnergy = character.CurrentEnergy;
                    character.CurrentEnergy = Math.Min(character.MaxEnergy, character.CurrentEnergy + energyGain);
                    int actualEnergyGain = character.CurrentEnergy - oldEnergy;
                    if (actualEnergyGain > 0)
                    {
                        statChanges.Add(new StatChangeDto("Energy", oldEnergy, character.CurrentEnergy, actualEnergyGain));
                        narrative = GetPositiveEnergyNarrative(actualEnergyGain);
                    }
                    else
                    {
                        return null; // No effect, skip event
                    }
                    break;

                case 3: // Reputation gain
                    int repGain = _random.Next(3, 8);
                    int oldRep = character.Reputation;
                    character.Reputation += repGain;
                    statChanges.Add(new StatChangeDto("Reputation", oldRep, character.Reputation, repGain));
                    narrative = GetPositiveReputationNarrative(repGain);
                    break;

                default: // Experience bonus
                    int expGain = _random.Next(10, 25);
                    int oldExp = character.Experience;
                    character.Experience += expGain;
                    statChanges.Add(new StatChangeDto("Experience", oldExp, character.Experience, expGain));
                    narrative = GetPositiveExperienceNarrative(expGain);
                    break;
            }
        }
        else
        {
            // Negative mini events
            int eventType = _random.Next(4);
            switch (eventType)
            {
                case 0: // Random stat decrease
                    var randomStat = (StatType)_random.Next(6);
                    int statLoss = _random.Next(1, 3);
                    int oldStat = character.GetStat(randomStat);
                    character.AddStat(randomStat, -statLoss);
                    int actualLoss = oldStat - character.GetStat(randomStat);
                    if (actualLoss > 0)
                    {
                        statChanges.Add(new StatChangeDto(randomStat.ToString(), oldStat, character.GetStat(randomStat), -actualLoss));
                        narrative = GetNegativeStatNarrative(randomStat, actualLoss);
                    }
                    else
                    {
                        return null;
                    }
                    break;

                case 1: // Gold lost
                    int goldLoss = _random.Next(5, 15);
                    if (character.Gold >= goldLoss)
                    {
                        int oldGold = character.Gold;
                        character.Gold -= goldLoss;
                        statChanges.Add(new StatChangeDto("Gold", oldGold, character.Gold, -goldLoss));
                        narrative = GetNegativeGoldNarrative(goldLoss);
                    }
                    else
                    {
                        return null;
                    }
                    break;

                case 2: // Energy drain
                    int energyLoss = _random.Next(5, 15);
                    if (character.CurrentEnergy > energyLoss)
                    {
                        int oldEnergy = character.CurrentEnergy;
                        character.CurrentEnergy -= energyLoss;
                        statChanges.Add(new StatChangeDto("Energy", oldEnergy, character.CurrentEnergy, -energyLoss));
                        narrative = GetNegativeEnergyNarrative(energyLoss);
                    }
                    else
                    {
                        return null;
                    }
                    break;

                default: // Minor health loss
                    int healthLoss = _random.Next(3, 10);
                    if (character.CurrentHealth > healthLoss + 10) // Don't kill the player
                    {
                        int oldHealth = character.CurrentHealth;
                        character.CurrentHealth -= healthLoss;
                        statChanges.Add(new StatChangeDto("Health", oldHealth, character.CurrentHealth, -healthLoss));
                        narrative = GetNegativeHealthNarrative(healthLoss);
                    }
                    else
                    {
                        return null;
                    }
                    break;
            }
        }

        return new MiniEventResult(narrative, statChanges);
    }

    private string GetPositiveStatNarrative(StatType stat, int gain)
    {
        return stat switch
        {
            StatType.Strength => $"A surge of determination strengthens your muscles! (+{gain} Strength)",
            StatType.Agility => $"Your reflexes feel sharper than ever! (+{gain} Agility)",
            StatType.Intelligence => $"A moment of clarity expands your understanding! (+{gain} Intelligence)",
            StatType.Endurance => $"You feel more resilient and hardy! (+{gain} Endurance)",
            StatType.Charisma => $"Your confidence grows after a positive encounter! (+{gain} Charisma)",
            StatType.Luck => $"A fortunate omen crosses your path! (+{gain} Luck)",
            _ => $"You feel stronger! (+{gain} {stat})"
        };
    }

    private string GetPositiveGoldNarrative(int gold)
    {
        string[] narratives = {
            $"You find a forgotten pouch of coins! (+{gold} Gold)",
            $"A grateful stranger rewards your kindness! (+{gold} Gold)",
            $"You discover a small cache of treasure! (+{gold} Gold)",
            $"A merchant overpays you by accident! (+{gold} Gold)"
        };
        return narratives[_random.Next(narratives.Length)];
    }

    private string GetPositiveEnergyNarrative(int energy)
    {
        string[] narratives = {
            $"A refreshing breeze invigorates you! (+{energy} Energy)",
            $"You find a moment of perfect rest! (+{energy} Energy)",
            $"A kind soul offers you a revitalizing meal! (+{energy} Energy)",
            $"The beauty of nature fills you with renewed vigor! (+{energy} Energy)"
        };
        return narratives[_random.Next(narratives.Length)];
    }

    private string GetPositiveReputationNarrative(int rep)
    {
        string[] narratives = {
            $"Word of your deeds spreads favorably! (+{rep} Reputation)",
            $"A local bard sings of your exploits! (+{rep} Reputation)",
            $"Your good nature impresses those nearby! (+{rep} Reputation)",
            $"People whisper admiringly as you pass! (+{rep} Reputation)"
        };
        return narratives[_random.Next(narratives.Length)];
    }

    private string GetPositiveExperienceNarrative(int exp)
    {
        string[] narratives = {
            $"You reflect on past lessons and gain insight! (+{exp} Experience)",
            $"A wise traveler shares valuable knowledge! (+{exp} Experience)",
            $"You observe something that deepens your understanding! (+{exp} Experience)",
            $"Experience from your journey crystallizes in your mind! (+{exp} Experience)"
        };
        return narratives[_random.Next(narratives.Length)];
    }

    private string GetNegativeStatNarrative(StatType stat, int loss)
    {
        return stat switch
        {
            StatType.Strength => $"A bout of weakness leaves you feeling frail. (-{loss} Strength)",
            StatType.Agility => $"A stumble makes you doubt your reflexes. (-{loss} Agility)",
            StatType.Intelligence => $"Mental fog clouds your thoughts. (-{loss} Intelligence)",
            StatType.Endurance => $"Fatigue wears down your resilience. (-{loss} Endurance)",
            StatType.Charisma => $"An awkward encounter dents your confidence. (-{loss} Charisma)",
            StatType.Luck => $"An ill omen dampens your fortune. (-{loss} Luck)",
            _ => $"You feel weakened. (-{loss} {stat})"
        };
    }

    private string GetNegativeGoldNarrative(int gold)
    {
        string[] narratives = {
            $"A pickpocket lightens your purse! (-{gold} Gold)",
            $"You lose some coins through a hole in your pocket. (-{gold} Gold)",
            $"An unexpected toll drains your funds. (-{gold} Gold)",
            $"A scam artist tricks you out of some gold. (-{gold} Gold)"
        };
        return narratives[_random.Next(narratives.Length)];
    }

    private string GetNegativeEnergyNarrative(int energy)
    {
        string[] narratives = {
            $"The heat of the day saps your strength. (-{energy} Energy)",
            $"Restless thoughts disturb your peace. (-{energy} Energy)",
            $"A sudden chill leaves you feeling drained. (-{energy} Energy)",
            $"An uneasy feeling exhausts you. (-{energy} Energy)"
        };
        return narratives[_random.Next(narratives.Length)];
    }

    private string GetNegativeHealthNarrative(int health)
    {
        string[] narratives = {
            $"You stub your toe badly on a rock. (-{health} Health)",
            $"A minor fall leaves you bruised. (-{health} Health)",
            $"Something you ate doesn't agree with you. (-{health} Health)",
            $"A small creature bites you unexpectedly. (-{health} Health)"
        };
        return narratives[_random.Next(narratives.Length)];
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
                es.Storybook.Theme,
                es.Storybook.StrengthBonus,
                es.Storybook.AgilityBonus,
                es.Storybook.IntelligenceBonus,
                es.Storybook.EnduranceBonus,
                es.Storybook.CharismaBonus,
                es.Storybook.LuckBonus,
                es.Storybook.EventTriggerChance,
                es.Storybook.Events?.Select(e => new StorybookEventSummaryDto(e.Title, e.Description, e.Rarity)).ToList()
                    ?? new List<StorybookEventSummaryDto>()
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
