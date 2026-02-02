using ChroniclesOfFate.Core.DTOs;
using ChroniclesOfFate.Core.Entities;
using ChroniclesOfFate.Core.Enums;
using ChroniclesOfFate.Core.Interfaces;

namespace ChroniclesOfFate.Core.Services;

/// <summary>
/// Handles training scenarios and stat progression
/// </summary>
public class TrainingService : ITrainingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRandomService _random;

    public TrainingService(IUnitOfWork unitOfWork, IRandomService random)
    {
        _unitOfWork = unitOfWork;
        _random = random;
    }

    public async Task<IEnumerable<TrainingScenarioDto>> GetAvailableScenariosAsync(int characterId)
    {
        var character = await _unitOfWork.Characters.GetByIdAsync(characterId)
            ?? throw new InvalidOperationException("Character not found");

        var scenarios = await _unitOfWork.TrainingScenarios.GetAvailableForLevelAsync(character.Level);

        return scenarios.Select(s => new TrainingScenarioDto(
            s.Id,
            s.Name,
            s.Description,
            s.ImageUrl,
            s.PrimaryStat,
            s.SecondaryStat,
            s.TertiaryStat,
            s.BaseStatGain,
            s.SecondaryStatGain,
            s.TertiaryStatGain,
            s.EnergyCost,
            s.BonusChance,
            s.ExperienceGain,
            HasSeasonalBonus(s, character.CurrentSeason)
        ));
    }

    public async Task<TrainingResultDto> ExecuteTrainingAsync(int characterId, int scenarioId)
    {
        var character = await _unitOfWork.Characters.GetWithStorybooksAsync(characterId)
            ?? throw new InvalidOperationException("Character not found");

        var scenario = await _unitOfWork.TrainingScenarios.GetByIdAsync(scenarioId)
            ?? throw new InvalidOperationException("Training scenario not found");

        // Check energy
        if (character.CurrentEnergy < scenario.EnergyCost)
        {
            return new TrainingResultDto(
                false,
                "Not enough energy to train. Rest to recover energy.",
                0, 0, 0, false, false, 0, 0,
                new List<StatChangeDto>()
            );
        }

        // Check level requirement
        if (character.Level < scenario.RequiredLevel)
        {
            return new TrainingResultDto(
                false,
                $"This training requires level {scenario.RequiredLevel}. Keep growing stronger!",
                0, 0, 0, false, false, 0, 0,
                new List<StatChangeDto>()
            );
        }

        var statChanges = new List<StatChangeDto>();
        var narrativeParts = new List<string>();
        
        // Spend energy
        character.CurrentEnergy -= scenario.EnergyCost;

        // Check for training failure
        bool failureOccurred = _random.RollChance(scenario.FailureChance);
        if (failureOccurred)
        {
            // Training accident
            int oldHealth = character.CurrentHealth;
            character.CurrentHealth = Math.Max(1, character.CurrentHealth - scenario.FailureHealthPenalty);
            
            statChanges.Add(new StatChangeDto("Health", oldHealth, character.CurrentHealth, -scenario.FailureHealthPenalty));
            
            narrativeParts.Add(GenerateFailureNarrative(scenario));
            
            await _unitOfWork.Characters.UpdateAsync(character);
            await _unitOfWork.SaveChangesAsync();

            return new TrainingResultDto(
                false,
                string.Join(" ", narrativeParts),
                0, 0, 0, false, true,
                scenario.EnergyCost,
                0,
                statChanges
            );
        }

        // Calculate seasonal bonus
        double seasonalMultiplier = CalculateSeasonalBonus(scenario, character.CurrentSeason);

        // Calculate storybook bonuses
        double storybookMultiplier = CalculateStorybookBonus(character, scenario.PrimaryStat);

        // Primary stat gain with random variance (base to +25%)
        double randomVariance = 1.0 + _random.NextDouble() * 0.25;
        int primaryGain = (int)(scenario.BaseStatGain * seasonalMultiplier * storybookMultiplier * randomVariance);
        int oldPrimary = character.GetStat(scenario.PrimaryStat);
        character.AddStat(scenario.PrimaryStat, primaryGain);
        statChanges.Add(new StatChangeDto(
            scenario.PrimaryStat.ToString(),
            oldPrimary,
            character.GetStat(scenario.PrimaryStat),
            primaryGain
        ));

        // Secondary stat gain (if applicable) with random variance
        int secondaryGain = 0;
        if (scenario.SecondaryStat.HasValue)
        {
            randomVariance = 1.0 + _random.NextDouble() * 0.25;
            secondaryGain = (int)(scenario.SecondaryStatGain * seasonalMultiplier * randomVariance);
            int oldSecondary = character.GetStat(scenario.SecondaryStat.Value);
            character.AddStat(scenario.SecondaryStat.Value, secondaryGain);
            statChanges.Add(new StatChangeDto(
                scenario.SecondaryStat.Value.ToString(),
                oldSecondary,
                character.GetStat(scenario.SecondaryStat.Value),
                secondaryGain
            ));
        }

        // Tertiary stat gain (if applicable) with random variance
        int tertiaryGain = 0;
        if (scenario.TertiaryStat.HasValue)
        {
            randomVariance = 1.0 + _random.NextDouble() * 0.25;
            tertiaryGain = (int)(scenario.TertiaryStatGain * seasonalMultiplier * randomVariance);
            int oldTertiary = character.GetStat(scenario.TertiaryStat.Value);
            character.AddStat(scenario.TertiaryStat.Value, tertiaryGain);
            statChanges.Add(new StatChangeDto(
                scenario.TertiaryStat.Value.ToString(),
                oldTertiary,
                character.GetStat(scenario.TertiaryStat.Value),
                tertiaryGain
            ));
        }

        // Check for bonus trigger
        bool bonusTriggered = _random.RollChance(scenario.BonusChance + character.Luck / 1000.0);
        if (bonusTriggered)
        {
            int bonusGain = scenario.BonusStatGain;
            int oldStat = character.GetStat(scenario.PrimaryStat);
            character.AddStat(scenario.PrimaryStat, bonusGain);
            primaryGain += bonusGain;

            // Update the stat change entry
            var existingChange = statChanges.First(c => c.StatName == scenario.PrimaryStat.ToString());
            statChanges.Remove(existingChange);
            statChanges.Insert(0, new StatChangeDto(
                scenario.PrimaryStat.ToString(),
                existingChange.OldValue,
                character.GetStat(scenario.PrimaryStat),
                existingChange.Change + bonusGain
            ));

            narrativeParts.Add(GenerateBonusNarrative(scenario));
        }

        // Experience gain
        int expGain = scenario.ExperienceGain;
        character.Experience += expGain;

        // Build narrative
        narrativeParts.Insert(0, GenerateTrainingNarrative(scenario, seasonalMultiplier > 1.0));

        await _unitOfWork.Characters.UpdateAsync(character);
        await _unitOfWork.SaveChangesAsync();

        return new TrainingResultDto(
            true,
            string.Join(" ", narrativeParts),
            primaryGain,
            secondaryGain,
            tertiaryGain,
            bonusTriggered,
            false,
            scenario.EnergyCost,
            expGain,
            statChanges
        );
    }

    public double CalculateSeasonalBonus(TrainingScenario scenario, Season season)
    {
        if (string.IsNullOrEmpty(scenario.BonusSeasons))
            return 1.0;

        var bonusSeasons = scenario.BonusSeasons.Split(',')
            .Select(s => Enum.Parse<Season>(s.Trim()))
            .ToList();

        return bonusSeasons.Contains(season) ? scenario.SeasonalBonusMultiplier : 1.0;
    }

    private bool HasSeasonalBonus(TrainingScenario scenario, Season season)
    {
        return CalculateSeasonalBonus(scenario, season) > 1.0;
    }

    private double CalculateStorybookBonus(Character character, StatType stat)
    {
        double bonus = 1.0;
        
        foreach (var equipped in character.EquippedStorybooks)
        {
            if (equipped.Storybook == null) continue;
            
            int statBonus = stat switch
            {
                StatType.Strength => equipped.Storybook.StrengthBonus,
                StatType.Agility => equipped.Storybook.AgilityBonus,
                StatType.Intelligence => equipped.Storybook.IntelligenceBonus,
                StatType.Endurance => equipped.Storybook.EnduranceBonus,
                StatType.Charisma => equipped.Storybook.CharismaBonus,
                StatType.Luck => equipped.Storybook.LuckBonus,
                _ => 0
            };

            bonus += statBonus / 100.0; // Each point = 1% bonus
        }

        return bonus;
    }

    private string GenerateTrainingNarrative(TrainingScenario scenario, bool hasSeasonalBonus)
    {
        string narrative = !string.IsNullOrEmpty(scenario.TrainingNarrative) 
            ? scenario.TrainingNarrative 
            : $"You complete your {scenario.Name} training.";

        if (hasSeasonalBonus)
        {
            narrative += " The favorable season boosts your training effectiveness!";
        }

        return narrative;
    }

    private string GenerateBonusNarrative(TrainingScenario scenario)
    {
        string[] bonusNarratives =
        {
            "A moment of clarity grants you additional insight!",
            "You push through your limits and achieve a breakthrough!",
            "Everything clicks into place - exceptional progress!",
            "Your dedication pays off with bonus gains!",
            "A surge of motivation drives you to train even harder!"
        };

        return bonusNarratives[_random.Next(bonusNarratives.Length)];
    }

    private string GenerateFailureNarrative(TrainingScenario scenario)
    {
        string[] failureNarratives =
        {
            $"An accident during {scenario.Name} training leaves you injured. Rest and recover.",
            $"You push too hard during training and hurt yourself. Take care of your health.",
            $"The training goes wrong and you sustain an injury. Sometimes setbacks happen.",
            $"Overexertion leads to an injury. Remember to pace yourself.",
            $"A training mishap leaves you battered. Even failures are lessons."
        };

        return failureNarratives[_random.Next(failureNarratives.Length)];
    }
}
