using ChroniclesOfFate.Core.Entities;
using ChroniclesOfFate.Core.Enums;
using ChroniclesOfFate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChroniclesOfFate.Infrastructure.Services;

/// <summary>
/// Seeds initial game data (training scenarios, enemies, storybooks, events)
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.MigrateAsync();

        if (!await context.TrainingScenarios.AnyAsync())
        {
            await SeedTrainingScenariosAsync(context);
        }

        if (!await context.Enemies.AnyAsync())
        {
            await SeedEnemiesAsync(context);
        }

        if (!await context.Storybooks.AnyAsync())
        {
            await SeedStorybooksAsync(context);
        }

        if (!await context.RandomEvents.AnyAsync())
        {
            await SeedRandomEventsAsync(context);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedTrainingScenariosAsync(ApplicationDbContext context)
    {
        var scenarios = new List<TrainingScenario>
        {
            new() { Name = "Combat Training", Description = "Intense physical training to build strength and combat prowess.",
                PrimaryStat = StatType.Strength, SecondaryStat = StatType.Endurance,
                BaseStatGain = 5, SecondaryStatGain = 2, EnergyCost = 25, BonusChance = 0.1, BonusStatGain = 3,
                FailureChance = 0.05, FailureHealthPenalty = 10, ExperienceGain = 15, RequiredLevel = 1,
                TrainingNarrative = "You train with weapons and practice combat techniques.", IsActive = true },
            new() { Name = "Agility Course", Description = "Navigate obstacles to improve speed and reflexes.",
                PrimaryStat = StatType.Agility, SecondaryStat = StatType.Luck,
                BaseStatGain = 5, SecondaryStatGain = 1, EnergyCost = 20, BonusChance = 0.12, BonusStatGain = 3,
                FailureChance = 0.08, FailureHealthPenalty = 8, ExperienceGain = 12, RequiredLevel = 1,
                TrainingNarrative = "You dash through the agility course, leaping over obstacles.", IsActive = true },
            new() { Name = "Arcane Studies", Description = "Study magical texts to expand your mystical knowledge.",
                PrimaryStat = StatType.Intelligence, SecondaryStat = StatType.Charisma,
                BaseStatGain = 5, SecondaryStatGain = 2, EnergyCost = 20, BonusChance = 0.15, BonusStatGain = 4,
                FailureChance = 0.03, FailureHealthPenalty = 5, ExperienceGain = 18, RequiredLevel = 1,
                TrainingNarrative = "You immerse yourself in ancient tomes.", IsActive = true,
                BonusSeasons = "Autumn,Winter", SeasonalBonusMultiplier = 1.25 },
            new() { Name = "Endurance March", Description = "Long-distance travel to build stamina.",
                PrimaryStat = StatType.Endurance, SecondaryStat = StatType.Strength,
                BaseStatGain = 5, SecondaryStatGain = 2, EnergyCost = 30, BonusChance = 0.1, BonusStatGain = 3,
                FailureChance = 0.04, FailureHealthPenalty = 12, ExperienceGain = 15, RequiredLevel = 1,
                TrainingNarrative = "You embark on a grueling journey.", IsActive = true,
                BonusSeasons = "Spring,Summer", SeasonalBonusMultiplier = 1.2 },
            new() { Name = "Social Gatherings", Description = "Attend events to improve social skills.",
                PrimaryStat = StatType.Charisma, SecondaryStat = StatType.Intelligence,
                BaseStatGain = 5, SecondaryStatGain = 2, EnergyCost = 15, BonusChance = 0.2, BonusStatGain = 3,
                FailureChance = 0.02, FailureHealthPenalty = 3, ExperienceGain = 10, RequiredLevel = 1,
                TrainingNarrative = "You mingle with nobles and commoners alike.", IsActive = true },
            new() { Name = "Fortune Games", Description = "Test your luck with games of chance.",
                PrimaryStat = StatType.Luck, SecondaryStat = StatType.Charisma,
                BaseStatGain = 4, SecondaryStatGain = 2, EnergyCost = 15, BonusChance = 0.25, BonusStatGain = 5,
                FailureChance = 0.1, FailureHealthPenalty = 5, ExperienceGain = 8, RequiredLevel = 1,
                TrainingNarrative = "You play games of chance.", IsActive = true }
        };
        await context.TrainingScenarios.AddRangeAsync(scenarios);
    }

    private static async Task SeedEnemiesAsync(ApplicationDbContext context)
    {
        var enemies = new List<Enemy>
        {
            new() { Name = "Forest Goblin", Description = "A mischievous creature lurking in the woods.",
                Strength = 20, Agility = 30, Intelligence = 10, Endurance = 15, Health = 50,
                DifficultyTier = 1, EnemyType = "Creature", ExperienceReward = 25, GoldReward = 10, ReputationReward = 5 },
            new() { Name = "Bandit Thug", Description = "A common criminal preying on travelers.",
                Strength = 35, Agility = 25, Intelligence = 15, Endurance = 30, Health = 70,
                DifficultyTier = 2, EnemyType = "Humanoid", ExperienceReward = 40, GoldReward = 25, ReputationReward = 10 },
            new() { Name = "Wild Wolf", Description = "A fierce predator of the wilderness.",
                Strength = 40, Agility = 50, Intelligence = 15, Endurance = 35, Health = 60,
                DifficultyTier = 2, EnemyType = "Beast", ExperienceReward = 35, GoldReward = 5, ReputationReward = 8 },
            new() { Name = "Orc Warrior", Description = "A brutal fighter from the mountain tribes.",
                Strength = 60, Agility = 30, Intelligence = 20, Endurance = 55, Health = 100,
                DifficultyTier = 3, EnemyType = "Humanoid", ExperienceReward = 60, GoldReward = 40, ReputationReward = 15 },
            new() { Name = "Dark Mage", Description = "A practitioner of forbidden magic.",
                Strength = 20, Agility = 35, Intelligence = 80, Endurance = 30, Health = 70,
                DifficultyTier = 4, EnemyType = "Humanoid", ExperienceReward = 80, GoldReward = 60, ReputationReward = 25 },
            new() { Name = "Cave Troll", Description = "A massive creature of immense strength.",
                Strength = 90, Agility = 15, Intelligence = 10, Endurance = 80, Health = 150,
                DifficultyTier = 5, EnemyType = "Giant", ExperienceReward = 100, GoldReward = 50, ReputationReward = 30 },
            new() { Name = "Shadow Assassin", Description = "A deadly killer from the shadows.",
                Strength = 50, Agility = 95, Intelligence = 60, Endurance = 40, Health = 80,
                DifficultyTier = 6, EnemyType = "Humanoid", ExperienceReward = 120, GoldReward = 80, ReputationReward = 40 },
            new() { Name = "Dragon Wyrmling", Description = "A young dragon, dangerous despite its age.",
                Strength = 100, Agility = 60, Intelligence = 50, Endurance = 90, Health = 200,
                DifficultyTier = 7, EnemyType = "Dragon", ExperienceReward = 200, GoldReward = 150, ReputationReward = 50 }
        };
        await context.Enemies.AddRangeAsync(enemies);
    }

    private static async Task SeedStorybooksAsync(ApplicationDbContext context)
    {
        var storybooks = new List<Storybook>
        {
            new() { Name = "Tales of the Brave", Description = "Stories of legendary warriors inspire courage.",
                Rarity = Rarity.Common, Theme = "Combat", StrengthBonus = 5, EnduranceBonus = 3, EventTriggerChance = 0.15 },
            new() { Name = "Whispers of the Wind", Description = "Ancient legends of swift heroes.",
                Rarity = Rarity.Common, Theme = "Speed", AgilityBonus = 5, LuckBonus = 3, EventTriggerChance = 0.15 },
            new() { Name = "Grimoire of Shadows", Description = "A mysterious tome filled with arcane secrets.",
                Rarity = Rarity.Uncommon, Theme = "Magic", IntelligenceBonus = 8, CharismaBonus = 2, EventTriggerChance = 0.18,
                IsUnlockable = true, UnlockCondition = "Reach Intelligence 100" },
            new() { Name = "Chronicle of Fortune", Description = "Tales of lucky adventurers.",
                Rarity = Rarity.Rare, Theme = "Luck", LuckBonus = 10, CharismaBonus = 5, EventTriggerChance = 0.2,
                IsUnlockable = true, UnlockCondition = "Win 10 battles" },
            new() { Name = "Epic of the Dragon Slayer", Description = "The legendary tale of heroes who faced dragons.",
                Rarity = Rarity.Epic, Theme = "Legendary", StrengthBonus = 10, AgilityBonus = 5, EnduranceBonus = 5, EventTriggerChance = 0.25,
                IsUnlockable = true, UnlockCondition = "Defeat a Dragon Wyrmling" }
        };
        await context.Storybooks.AddRangeAsync(storybooks);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRandomEventsAsync(ApplicationDbContext context)
    {
        // Global events
        var mysteriousStranger = new RandomEvent
        {
            Title = "Mysterious Stranger", Description = "A cloaked figure approaches you with an offer.",
            Rarity = Rarity.Common, OutcomeType = EventOutcome.Neutral,
            TriggerActions = "Explore,Rest", BaseProbability = 0.15, IsActive = true,
            Choices = new List<EventChoice>
            {
                new() { Text = "Accept the offer", ResultDescription = "The stranger teaches you a secret technique.",
                    StrengthChange = 3, IntelligenceChange = 2, ExperienceChange = 20, DisplayOrder = 1 },
                new() { Text = "Decline politely", ResultDescription = "You politely refuse. The stranger nods and vanishes.",
                    CharismaChange = 2, DisplayOrder = 2 },
                new() { Text = "Demand to know their identity", ResultDescription = "Your boldness impresses the stranger.",
                    CheckStat = StatType.Charisma, CheckDifficulty = 50, CharismaChange = 5, ReputationChange = 10,
                    FailureDescription = "The stranger is offended and disappears.", FailureCharismaChange = -2, DisplayOrder = 3 }
            }
        };

        var hiddenTreasure = new RandomEvent
        {
            Title = "Hidden Treasure", Description = "You discover signs of buried treasure nearby.",
            Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive,
            TriggerActions = "Explore", BaseProbability = 0.1, IsActive = true,
            Choices = new List<EventChoice>
            {
                new() { Text = "Dig for the treasure", ResultDescription = "You uncover a chest of gold!",
                    CheckStat = StatType.Luck, CheckDifficulty = 40, GoldChange = 50, ExperienceChange = 15,
                    FailureDescription = "You dig but find only rocks.", FailureEnergyChange = -10, DisplayOrder = 1 },
                new() { Text = "Mark the location for later", ResultDescription = "You note the location carefully.",
                    IntelligenceChange = 1, DisplayOrder = 2 }
            }
        };

        var ambush = new RandomEvent
        {
            Title = "Ambush!", Description = "Bandits leap from the shadows to attack!",
            Rarity = Rarity.Common, OutcomeType = EventOutcome.Negative,
            TriggerActions = "Explore,Battle", BaseProbability = 0.12, IsActive = true,
            Choices = new List<EventChoice>
            {
                new() { Text = "Fight back!", ResultDescription = "You defeat the bandits and claim their loot!",
                    CheckStat = StatType.Strength, CheckDifficulty = 45, StrengthChange = 2, GoldChange = 30, ExperienceChange = 25,
                    FailureDescription = "You're overwhelmed and barely escape.", FailureHealthChange = -20, FailureGoldChange = -15, DisplayOrder = 1 },
                new() { Text = "Try to escape", ResultDescription = "Your quick reflexes help you escape!",
                    CheckStat = StatType.Agility, CheckDifficulty = 40, AgilityChange = 2, ExperienceChange = 10,
                    FailureDescription = "You trip while fleeing.", FailureHealthChange = -10, DisplayOrder = 2 },
                new() { Text = "Negotiate", ResultDescription = "Your silver tongue convinces them to let you go.",
                    CheckStat = StatType.Charisma, CheckDifficulty = 55, CharismaChange = 3, ReputationChange = 5,
                    FailureDescription = "They laugh and attack anyway.", FailureHealthChange = -15, DisplayOrder = 3 }
            }
        };

        var trainingInsight = new RandomEvent
        {
            Title = "Training Insight", Description = "You have a moment of clarity during training.",
            Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive,
            TriggerActions = "Train", BaseProbability = 0.08, IsActive = true,
            Choices = new List<EventChoice>
            {
                new() { Text = "Focus on the insight", ResultDescription = "Your understanding deepens significantly!",
                    IntelligenceChange = 5, ExperienceChange = 30, DisplayOrder = 1 },
                new() { Text = "Apply it to your training", ResultDescription = "You immediately put the insight to use.",
                    StrengthChange = 2, AgilityChange = 2, EnduranceChange = 2, DisplayOrder = 2 }
            }
        };

        await context.RandomEvents.AddRangeAsync(new[] { mysteriousStranger, hiddenTreasure, ambush, trainingInsight });
    }
}
