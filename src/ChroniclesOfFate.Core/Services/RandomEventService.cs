using ChroniclesOfFate.Core.DTOs;
using ChroniclesOfFate.Core.Entities;
using ChroniclesOfFate.Core.Enums;
using ChroniclesOfFate.Core.Interfaces;
using System.Text.Json;

namespace ChroniclesOfFate.Core.Services;

/// <summary>
/// Handles random event triggering and decision tree processing
/// </summary>
public class RandomEventService : IRandomEventService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRandomService _random;

    public RandomEventService(IUnitOfWork unitOfWork, IRandomService random)
    {
        _unitOfWork = unitOfWork;
        _random = random;
    }

    public async Task<RandomEventDto?> TryTriggerEventAsync(
        int characterId,
        ActionType action,
        IEnumerable<int> equippedStorybookIds,
        bool preferHigherRarity = false,
        bool doubleNonCommonRate = false)
    {
        var character = await _unitOfWork.Characters.GetByIdAsync(characterId)
            ?? throw new InvalidOperationException("Character not found");

        var eligibleEvents = await _unitOfWork.RandomEvents.GetEligibleEventsAsync(
            action,
            character.CurrentSeason,
            equippedStorybookIds
        );

        var eligibleList = eligibleEvents.ToList();
        if (!eligibleList.Any())
            return null;

        eligibleList = eligibleList
            .Where(e => MeetsStatRequirements(character, e.StatRequirements))
            .ToList();

        if (!eligibleList.Any())
            return null;

        // Build weighted list based on rarity boosts
        var weightedEvents = new List<(RandomEvent ev, double weight)>();

        foreach (var ev in eligibleList)
        {
            double weight = ev.BaseProbability;

            if (ev.StorybookId.HasValue)
            {
                var storybook = await _unitOfWork.Storybooks.GetByIdAsync(ev.StorybookId.Value);
                if (storybook != null)
                {
                    weight *= storybook.EventTriggerChance * 2;
                }
            }

            weight *= (1.0 + character.Luck / 500.0);

            // When preferring higher rarity, boost weights based on rarity
            if (preferHigherRarity)
            {
                double rarityBoost = ev.Rarity switch
                {
                    Rarity.Common => 1.0,
                    Rarity.Uncommon => 1.5,
                    Rarity.Rare => 2.0,
                    Rarity.Epic => 2.5,
                    Rarity.Legendary => 3.0,
                    _ => 1.0
                };
                weight *= rarityBoost;
            }

            // When doubleNonCommonRate is true, double the weight for non-common events
            if (doubleNonCommonRate)
            {
                double rarityMultiplier = ev.Rarity switch
                {
                    Rarity.Common => 1.0,
                    _ => 2.0  // Double rate for Uncommon, Rare, Epic, Legendary
                };
                weight *= rarityMultiplier;
            }

            weightedEvents.Add((ev, weight));
        }

        if (!weightedEvents.Any())
            return null;

        // Select one event based on weighted random selection
        double totalWeight = weightedEvents.Sum(e => e.weight);
        double roll = _random.NextDouble() * totalWeight;
        double cumulative = 0;

        foreach (var (ev, weight) in weightedEvents)
        {
            cumulative += weight;
            if (roll < cumulative)
            {
                var fullEvent = await _unitOfWork.RandomEvents.GetWithChoicesAsync(ev.Id);
                if (fullEvent != null)
                {
                    return MapToEventDto(fullEvent, character);
                }
                break;
            }
        }

        return null;
    }

    public async Task<EventChoiceResultDto> ProcessChoiceAsync(
        int characterId,
        int eventId,
        int choiceId)
    {
        var character = await _unitOfWork.Characters.GetByIdAsync(characterId)
            ?? throw new InvalidOperationException("Character not found");

        var randomEvent = await _unitOfWork.RandomEvents.GetWithChoicesAsync(eventId)
            ?? throw new InvalidOperationException("Event not found");

        var choice = randomEvent.Choices.FirstOrDefault(c => c.Id == choiceId)
            ?? throw new InvalidOperationException("Choice not found");

        var statChanges = new List<StatChangeDto>();
        bool? checkSucceeded = null;
        int? rollResult = null;
        string resultDescription;

        if (choice.CheckStat.HasValue && choice.CheckDifficulty > 0)
        {
            int statValue = character.GetStat(choice.CheckStat.Value);
            rollResult = _random.RollDice(100);
            int effectiveRoll = rollResult.Value + (statValue / 10);
            checkSucceeded = effectiveRoll >= choice.CheckDifficulty;
        }

        string? grantedSkillName = null;

        if (checkSucceeded == false)
        {
            resultDescription = choice.FailureDescription ?? "Your attempt failed.";
            ApplyStatChanges(character, choice, false, statChanges);

            // Grant failure skill if specified
            if (choice.FailureGrantSkillId.HasValue)
            {
                grantedSkillName = await TryGrantSkillAsync(characterId, choice.FailureGrantSkillId.Value, randomEvent.Title, character.TotalTurns);
            }
        }
        else
        {
            resultDescription = choice.ResultDescription;
            ApplyStatChanges(character, choice, true, statChanges);

            // Grant success skill if specified
            if (choice.GrantSkillId.HasValue)
            {
                grantedSkillName = await TryGrantSkillAsync(characterId, choice.GrantSkillId.Value, randomEvent.Title, character.TotalTurns);
            }
        }

        // Append skill acquisition to result description
        if (!string.IsNullOrEmpty(grantedSkillName))
        {
            resultDescription += $" You learned the skill: {grantedSkillName}!";
        }

        var gameEvent = new GameEvent
        {
            CharacterId = characterId,
            RandomEventId = eventId,
            ChosenOptionId = choiceId,
            Year = character.CurrentYear,
            Month = character.CurrentMonth,
            Turn = character.TotalTurns,
            CheckSucceeded = checkSucceeded,
            RollResult = rollResult,
            ResultSummary = BuildResultSummary(statChanges)
        };

        await _unitOfWork.GameEvents.AddAsync(gameEvent);
        await _unitOfWork.Characters.UpdateAsync(character);
        await _unitOfWork.SaveChangesAsync();

        RandomEventDto? followUpEvent = null;
        if (choice.FollowUpEventId.HasValue)
        {
            var followUp = await _unitOfWork.RandomEvents.GetWithChoicesAsync(choice.FollowUpEventId.Value);
            if (followUp != null)
            {
                followUpEvent = MapToEventDto(followUp, character);
            }
        }

        return new EventChoiceResultDto(
            true,
            checkSucceeded,
            rollResult,
            choice.CheckDifficulty,
            resultDescription,
            statChanges,
            followUpEvent,
            choice.TriggerBattleId
        );
    }

    public async Task<IEnumerable<RandomEventDto>> GetAvailableEventsAsync(
        int characterId,
        ActionType action)
    {
        var character = await _unitOfWork.Characters.GetWithStorybooksAsync(characterId)
            ?? throw new InvalidOperationException("Character not found");

        var storybookIds = character.EquippedStorybooks
            .Select(cs => cs.StorybookId)
            .ToList();

        var events = await _unitOfWork.RandomEvents.GetEligibleEventsAsync(
            action,
            character.CurrentSeason,
            storybookIds
        );

        return events
            .Where(e => MeetsStatRequirements(character, e.StatRequirements))
            .Select(e => MapToEventDto(e, character));
    }

    private void ApplyStatChanges(Character character, EventChoice choice, bool success, List<StatChangeDto> changes)
    {
        if (success)
        {
            ApplyChange(character, StatType.Strength, choice.StrengthChange, changes);
            ApplyChange(character, StatType.Agility, choice.AgilityChange, changes);
            ApplyChange(character, StatType.Intelligence, choice.IntelligenceChange, changes);
            ApplyChange(character, StatType.Endurance, choice.EnduranceChange, changes);
            ApplyChange(character, StatType.Charisma, choice.CharismaChange, changes);
            ApplyChange(character, StatType.Luck, choice.LuckChange, changes);
            ApplyResourceChange(character, "Energy", choice.EnergyChange, changes,
                () => character.CurrentEnergy, v => character.CurrentEnergy = Math.Clamp(v, 0, character.MaxEnergy));
            ApplyResourceChange(character, "Health", choice.HealthChange, changes,
                () => character.CurrentHealth, v => character.CurrentHealth = Math.Clamp(v, 1, character.MaxHealth));
            ApplyResourceChange(character, "Gold", choice.GoldChange, changes,
                () => character.Gold, v => character.Gold = Math.Max(0, v));
            ApplyResourceChange(character, "Reputation", choice.ReputationChange, changes,
                () => character.Reputation, v => character.Reputation = v);
            ApplyResourceChange(character, "Experience", choice.ExperienceChange, changes,
                () => character.Experience, v => character.Experience = Math.Max(0, v));
        }
        else
        {
            ApplyChange(character, StatType.Strength, choice.FailureStrengthChange, changes);
            ApplyChange(character, StatType.Agility, choice.FailureAgilityChange, changes);
            ApplyChange(character, StatType.Intelligence, choice.FailureIntelligenceChange, changes);
            ApplyChange(character, StatType.Endurance, choice.FailureEnduranceChange, changes);
            ApplyChange(character, StatType.Charisma, choice.FailureCharismaChange, changes);
            ApplyChange(character, StatType.Luck, choice.FailureLuckChange, changes);
            ApplyResourceChange(character, "Energy", choice.FailureEnergyChange, changes,
                () => character.CurrentEnergy, v => character.CurrentEnergy = Math.Clamp(v, 0, character.MaxEnergy));
            ApplyResourceChange(character, "Health", choice.FailureHealthChange, changes,
                () => character.CurrentHealth, v => character.CurrentHealth = Math.Clamp(v, 1, character.MaxHealth));
            ApplyResourceChange(character, "Gold", choice.FailureGoldChange, changes,
                () => character.Gold, v => character.Gold = Math.Max(0, v));
            ApplyResourceChange(character, "Reputation", choice.FailureReputationChange, changes,
                () => character.Reputation, v => character.Reputation = v);
        }
    }

    private void ApplyChange(Character character, StatType stat, int change, List<StatChangeDto> changes)
    {
        if (change == 0) return;

        int oldValue = character.GetStat(stat);
        character.AddStat(stat, change);
        changes.Add(new StatChangeDto(stat.ToString(), oldValue, character.GetStat(stat), change));
    }

    private void ApplyResourceChange(Character character, string name, int change, List<StatChangeDto> changes,
        Func<int> getter, Action<int> setter)
    {
        if (change == 0) return;

        int oldValue = getter();
        setter(oldValue + change);
        changes.Add(new StatChangeDto(name, oldValue, getter(), change));
    }

    private bool MeetsStatRequirements(Character character, string? requirements)
    {
        if (string.IsNullOrEmpty(requirements))
            return true;

        try
        {
            var reqs = JsonSerializer.Deserialize<Dictionary<string, int>>(requirements);
            if (reqs == null) return true;

            foreach (var req in reqs)
            {
                if (Enum.TryParse<StatType>(req.Key, out var stat))
                {
                    if (character.GetStat(stat) < req.Value)
                        return false;
                }
            }
            return true;
        }
        catch
        {
            return true;
        }
    }

    private string BuildResultSummary(List<StatChangeDto> changes)
    {
        if (!changes.Any()) return "No changes.";

        var parts = changes.Select(c => $"{c.StatName}: {(c.Change >= 0 ? "+" : "")}{c.Change}");
        return string.Join(", ", parts);
    }

    private RandomEventDto MapToEventDto(RandomEvent ev, Character character)
    {
        string? sourceStorybook = ev.Storybook?.Name;

        var choices = ev.Choices
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new EventChoiceDto(
                c.Id,
                c.Text,
                !MeetsStatRequirements(character, c.StatRequirements),
                c.CheckStat,
                c.CheckDifficulty,
                GetStatRequirementHint(c.StatRequirements),
                new ChoiceRewardsDto(
                    c.StrengthChange,
                    c.AgilityChange,
                    c.IntelligenceChange,
                    c.EnduranceChange,
                    c.CharismaChange,
                    c.LuckChange,
                    c.EnergyChange,
                    c.HealthChange,
                    c.GoldChange,
                    c.ReputationChange,
                    c.ExperienceChange,
                    c.GrantSkill?.Name
                ),
                c.CheckStat.HasValue ? new ChoiceRewardsDto(
                    c.FailureStrengthChange,
                    c.FailureAgilityChange,
                    c.FailureIntelligenceChange,
                    c.FailureEnduranceChange,
                    c.FailureCharismaChange,
                    c.FailureLuckChange,
                    c.FailureEnergyChange,
                    c.FailureHealthChange,
                    c.FailureGoldChange,
                    c.FailureReputationChange,
                    0,
                    c.FailureGrantSkill?.Name
                ) : null
            ))
            .ToList();

        return new RandomEventDto(
            ev.Id,
            ev.Title,
            ev.Description,
            ev.ImageUrl,
            ev.Rarity,
            ev.OutcomeType,
            sourceStorybook,
            choices
        );
    }

    private string? GetStatRequirementHint(string? requirements)
    {
        if (string.IsNullOrEmpty(requirements)) return null;

        try
        {
            var reqs = JsonSerializer.Deserialize<Dictionary<string, int>>(requirements);
            if (reqs == null || !reqs.Any()) return null;

            var hints = reqs.Select(r => $"{r.Key} {r.Value}+");
            return $"Requires: {string.Join(", ", hints)}";
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> TryGrantSkillAsync(int characterId, int skillId, string eventTitle, int turn)
    {
        // Check if character already has this skill
        if (await _unitOfWork.CharacterSkills.CharacterHasSkillAsync(characterId, skillId))
            return null;

        var skill = await _unitOfWork.Skills.GetByIdAsync(skillId);
        if (skill == null) return null;

        var characterSkill = await _unitOfWork.CharacterSkills.AddSkillToCharacterAsync(
            characterId,
            skillId,
            $"Event: {eventTitle}",
            turn
        );

        if (characterSkill != null)
        {
            await _unitOfWork.SaveChangesAsync();
            return skill.Name;
        }

        return null;
    }
}
