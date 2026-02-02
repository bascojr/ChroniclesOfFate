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
            // === STRENGTH-FOCUSED ===
            new() { Name = "Combat Training", Description = "Intense weapon drills and sparring to build raw combat power.",
                PrimaryStat = StatType.Strength, SecondaryStat = StatType.Endurance, TertiaryStat = StatType.Agility,
                BaseStatGain = 5, SecondaryStatGain = 3, TertiaryStatGain = 1, EnergyCost = 25, BonusChance = 0.1, BonusStatGain = 3,
                FailureChance = 0.05, FailureHealthPenalty = 10, ExperienceGain = 15, RequiredLevel = 1,
                TrainingNarrative = "You train with weapons and practice combat techniques.", IsActive = true },
            new() { Name = "Heavy Lifting", Description = "Move boulders and lift weights to build raw muscle.",
                PrimaryStat = StatType.Strength, SecondaryStat = StatType.Endurance,
                BaseStatGain = 6, SecondaryStatGain = 2, EnergyCost = 30, BonusChance = 0.08, BonusStatGain = 4,
                FailureChance = 0.07, FailureHealthPenalty = 15, ExperienceGain = 12, RequiredLevel = 1,
                TrainingNarrative = "You push your muscles to their limit lifting heavy stones.", IsActive = true },
            new() { Name = "Blacksmith Apprentice", Description = "Work the forge to build strength and learn craftsmanship.",
                PrimaryStat = StatType.Strength, SecondaryStat = StatType.Intelligence, TertiaryStat = StatType.Endurance,
                BaseStatGain = 4, SecondaryStatGain = 2, TertiaryStatGain = 2, EnergyCost = 25, BonusChance = 0.12, BonusStatGain = 3,
                FailureChance = 0.04, FailureHealthPenalty = 8, ExperienceGain = 18, RequiredLevel = 2,
                TrainingNarrative = "You hammer metal at the forge, learning the smith's craft.", IsActive = true },

            // === AGILITY-FOCUSED ===
            new() { Name = "Agility Course", Description = "Navigate obstacles to improve speed and reflexes.",
                PrimaryStat = StatType.Agility, SecondaryStat = StatType.Luck, TertiaryStat = StatType.Endurance,
                BaseStatGain = 5, SecondaryStatGain = 2, TertiaryStatGain = 1, EnergyCost = 20, BonusChance = 0.12, BonusStatGain = 3,
                FailureChance = 0.08, FailureHealthPenalty = 8, ExperienceGain = 12, RequiredLevel = 1,
                TrainingNarrative = "You dash through the agility course, leaping over obstacles.", IsActive = true },
            new() { Name = "Rooftop Running", Description = "Sprint across rooftops to master urban movement.",
                PrimaryStat = StatType.Agility, SecondaryStat = StatType.Endurance, TertiaryStat = StatType.Luck,
                BaseStatGain = 5, SecondaryStatGain = 2, TertiaryStatGain = 2, EnergyCost = 22, BonusChance = 0.15, BonusStatGain = 3,
                FailureChance = 0.1, FailureHealthPenalty = 12, ExperienceGain = 14, RequiredLevel = 2,
                TrainingNarrative = "You leap between buildings, the wind rushing past.", IsActive = true },
            new() { Name = "Dance Lessons", Description = "Learn graceful movements that enhance coordination.",
                PrimaryStat = StatType.Agility, SecondaryStat = StatType.Charisma, TertiaryStat = StatType.Luck,
                BaseStatGain = 4, SecondaryStatGain = 3, TertiaryStatGain = 1, EnergyCost = 15, BonusChance = 0.18, BonusStatGain = 2,
                FailureChance = 0.02, FailureHealthPenalty = 3, ExperienceGain = 10, RequiredLevel = 1,
                TrainingNarrative = "You practice elegant dance steps with a skilled instructor.", IsActive = true },

            // === INTELLIGENCE-FOCUSED ===
            new() { Name = "Arcane Studies", Description = "Study magical texts to expand your mystical knowledge.",
                PrimaryStat = StatType.Intelligence, SecondaryStat = StatType.Charisma, TertiaryStat = StatType.Luck,
                BaseStatGain = 5, SecondaryStatGain = 2, TertiaryStatGain = 1, EnergyCost = 20, BonusChance = 0.15, BonusStatGain = 4,
                FailureChance = 0.03, FailureHealthPenalty = 5, ExperienceGain = 18, RequiredLevel = 1,
                TrainingNarrative = "You immerse yourself in ancient tomes.", IsActive = true,
                BonusSeasons = "Autumn,Winter", SeasonalBonusMultiplier = 1.25 },
            new() { Name = "Strategy Games", Description = "Play chess and war games to sharpen tactical thinking.",
                PrimaryStat = StatType.Intelligence, SecondaryStat = StatType.Luck,
                BaseStatGain = 4, SecondaryStatGain = 2, EnergyCost = 15, BonusChance = 0.2, BonusStatGain = 3,
                FailureChance = 0.01, FailureHealthPenalty = 2, ExperienceGain = 12, RequiredLevel = 1,
                TrainingNarrative = "You engage opponents in games of strategy and wit.", IsActive = true },
            new() { Name = "Alchemy Practice", Description = "Mix potions and study reactions to understand the arcane sciences.",
                PrimaryStat = StatType.Intelligence, SecondaryStat = StatType.Luck, TertiaryStat = StatType.Endurance,
                BaseStatGain = 5, SecondaryStatGain = 2, TertiaryStatGain = 1, EnergyCost = 22, BonusChance = 0.14, BonusStatGain = 4,
                FailureChance = 0.06, FailureHealthPenalty = 10, ExperienceGain = 16, RequiredLevel = 2,
                TrainingNarrative = "You carefully combine ingredients, observing their reactions.", IsActive = true },

            // === ENDURANCE-FOCUSED ===
            new() { Name = "Endurance March", Description = "Long-distance travel to build stamina and willpower.",
                PrimaryStat = StatType.Endurance, SecondaryStat = StatType.Strength, TertiaryStat = StatType.Agility,
                BaseStatGain = 5, SecondaryStatGain = 2, TertiaryStatGain = 1, EnergyCost = 30, BonusChance = 0.1, BonusStatGain = 3,
                FailureChance = 0.04, FailureHealthPenalty = 12, ExperienceGain = 15, RequiredLevel = 1,
                TrainingNarrative = "You embark on a grueling journey across rough terrain.", IsActive = true,
                BonusSeasons = "Spring,Summer", SeasonalBonusMultiplier = 1.2 },
            new() { Name = "Cold Water Swimming", Description = "Swim in icy waters to harden body and mind.",
                PrimaryStat = StatType.Endurance, SecondaryStat = StatType.Strength,
                BaseStatGain = 5, SecondaryStatGain = 3, EnergyCost = 28, BonusChance = 0.1, BonusStatGain = 3,
                FailureChance = 0.08, FailureHealthPenalty = 15, ExperienceGain = 14, RequiredLevel = 2,
                TrainingNarrative = "You plunge into freezing waters, testing your limits.", IsActive = true,
                BonusSeasons = "Winter", SeasonalBonusMultiplier = 1.3 },
            new() { Name = "Meditation Retreat", Description = "Practice deep meditation to strengthen mental endurance.",
                PrimaryStat = StatType.Endurance, SecondaryStat = StatType.Intelligence, TertiaryStat = StatType.Charisma,
                BaseStatGain = 4, SecondaryStatGain = 2, TertiaryStatGain = 2, EnergyCost = 18, BonusChance = 0.15, BonusStatGain = 3,
                FailureChance = 0.02, FailureHealthPenalty = 3, ExperienceGain = 12, RequiredLevel = 1,
                TrainingNarrative = "You sit in silence, clearing your mind of distractions.", IsActive = true },

            // === CHARISMA-FOCUSED ===
            new() { Name = "Social Gatherings", Description = "Attend events to improve social skills and build connections.",
                PrimaryStat = StatType.Charisma, SecondaryStat = StatType.Intelligence, TertiaryStat = StatType.Luck,
                BaseStatGain = 5, SecondaryStatGain = 2, TertiaryStatGain = 1, EnergyCost = 15, BonusChance = 0.2, BonusStatGain = 3,
                FailureChance = 0.02, FailureHealthPenalty = 3, ExperienceGain = 10, RequiredLevel = 1,
                TrainingNarrative = "You mingle with nobles and commoners alike.", IsActive = true },
            new() { Name = "Public Speaking", Description = "Address crowds to build confidence and persuasion skills.",
                PrimaryStat = StatType.Charisma, SecondaryStat = StatType.Intelligence,
                BaseStatGain = 5, SecondaryStatGain = 2, EnergyCost = 18, BonusChance = 0.15, BonusStatGain = 4,
                FailureChance = 0.05, FailureHealthPenalty = 5, ExperienceGain = 14, RequiredLevel = 2,
                TrainingNarrative = "You stand before a crowd and speak with conviction.", IsActive = true },
            new() { Name = "Tavern Tales", Description = "Share stories and songs at the local tavern.",
                PrimaryStat = StatType.Charisma, SecondaryStat = StatType.Luck, TertiaryStat = StatType.Intelligence,
                BaseStatGain = 4, SecondaryStatGain = 2, TertiaryStatGain = 2, EnergyCost = 12, BonusChance = 0.22, BonusStatGain = 3,
                FailureChance = 0.03, FailureHealthPenalty = 2, ExperienceGain = 8, RequiredLevel = 1,
                TrainingNarrative = "You entertain the tavern crowd with your wit and charm.", IsActive = true },

            // === LUCK-FOCUSED ===
            new() { Name = "Fortune Games", Description = "Test your luck with games of chance and learn to read fate.",
                PrimaryStat = StatType.Luck, SecondaryStat = StatType.Charisma, TertiaryStat = StatType.Intelligence,
                BaseStatGain = 4, SecondaryStatGain = 2, TertiaryStatGain = 1, EnergyCost = 15, BonusChance = 0.25, BonusStatGain = 5,
                FailureChance = 0.1, FailureHealthPenalty = 5, ExperienceGain = 8, RequiredLevel = 1,
                TrainingNarrative = "You play games of chance, testing the whims of fortune.", IsActive = true },
            new() { Name = "Treasure Hunting", Description = "Search for hidden treasures, trusting your instincts.",
                PrimaryStat = StatType.Luck, SecondaryStat = StatType.Agility, TertiaryStat = StatType.Intelligence,
                BaseStatGain = 4, SecondaryStatGain = 2, TertiaryStatGain = 2, EnergyCost = 20, BonusChance = 0.2, BonusStatGain = 4,
                FailureChance = 0.08, FailureHealthPenalty = 6, ExperienceGain = 12, RequiredLevel = 1,
                TrainingNarrative = "You follow your instincts in search of hidden riches.", IsActive = true,
                BonusSeasons = "Spring,Autumn", SeasonalBonusMultiplier = 1.2 },
            new() { Name = "Stargazing", Description = "Study the stars to understand the patterns of fate.",
                PrimaryStat = StatType.Luck, SecondaryStat = StatType.Intelligence, TertiaryStat = StatType.Endurance,
                BaseStatGain = 3, SecondaryStatGain = 3, TertiaryStatGain = 1, EnergyCost = 12, BonusChance = 0.18, BonusStatGain = 3,
                FailureChance = 0.01, FailureHealthPenalty = 2, ExperienceGain = 10, RequiredLevel = 1,
                TrainingNarrative = "You gaze at the night sky, contemplating the cosmos.", IsActive = true,
                BonusSeasons = "Winter", SeasonalBonusMultiplier = 1.25 },

            // === MIXED/BALANCED ===
            new() { Name = "Adventurer's Guild Training", Description = "Complete diverse challenges at the guild hall.",
                PrimaryStat = StatType.Strength, SecondaryStat = StatType.Agility, TertiaryStat = StatType.Endurance,
                BaseStatGain = 4, SecondaryStatGain = 3, TertiaryStatGain = 2, EnergyCost = 28, BonusChance = 0.12, BonusStatGain = 3,
                FailureChance = 0.05, FailureHealthPenalty = 10, ExperienceGain = 20, RequiredLevel = 3,
                TrainingNarrative = "You tackle varied challenges alongside fellow adventurers.", IsActive = true },
            new() { Name = "Hunting Expedition", Description = "Track and hunt wild game in the wilderness.",
                PrimaryStat = StatType.Agility, SecondaryStat = StatType.Endurance, TertiaryStat = StatType.Luck,
                BaseStatGain = 4, SecondaryStatGain = 3, TertiaryStatGain = 2, EnergyCost = 25, BonusChance = 0.14, BonusStatGain = 3,
                FailureChance = 0.06, FailureHealthPenalty = 8, ExperienceGain = 16, RequiredLevel = 2,
                TrainingNarrative = "You track prey through the wilderness, honing your instincts.", IsActive = true,
                BonusSeasons = "Autumn", SeasonalBonusMultiplier = 1.2 },
            new() { Name = "Merchant Negotiations", Description = "Practice the art of trade and bargaining.",
                PrimaryStat = StatType.Charisma, SecondaryStat = StatType.Intelligence, TertiaryStat = StatType.Luck,
                BaseStatGain = 4, SecondaryStatGain = 3, TertiaryStatGain = 2, EnergyCost = 18, BonusChance = 0.16, BonusStatGain = 3,
                FailureChance = 0.03, FailureHealthPenalty = 3, ExperienceGain = 14, RequiredLevel = 2,
                TrainingNarrative = "You haggle with merchants, learning the art of the deal.", IsActive = true }
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
            // Storybooks contain events of various rarities - higher rarity events give bigger rewards
            new() { Name = "Tales of the Brave", Description = "Stories of legendary warriors inspire courage and strength.",
                Theme = "Combat", StrengthBonus = 5, EnduranceBonus = 3, EventTriggerChance = 0.80 },
            new() { Name = "Whispers of the Wind", Description = "Ancient legends of swift heroes who moved like the breeze.",
                Theme = "Speed", AgilityBonus = 5, LuckBonus = 3, EventTriggerChance = 0.80 },
            new() { Name = "Grimoire of Shadows", Description = "A mysterious tome filled with arcane secrets and dark knowledge.",
                Theme = "Magic", IntelligenceBonus = 8, CharismaBonus = 2, EventTriggerChance = 0.80 },
            new() { Name = "Chronicle of Fortune", Description = "Tales of adventurers blessed by fate itself.",
                Theme = "Luck", LuckBonus = 10, CharismaBonus = 5, EventTriggerChance = 0.80 },
            new() { Name = "Epic of the Dragon Slayer", Description = "The legendary tale of heroes who faced dragons and emerged victorious.",
                Theme = "Legendary", StrengthBonus = 10, AgilityBonus = 5, EnduranceBonus = 5, EventTriggerChance = 0.80 }
        };
        await context.Storybooks.AddRangeAsync(storybooks);
        await context.SaveChangesAsync();

        // Now add storybook-specific events for each book
        await SeedStorybookEventsAsync(context, storybooks);
    }

    private static async Task SeedStorybookEventsAsync(ApplicationDbContext context, List<Storybook> storybooks)
    {
        var storybookEvents = new List<RandomEvent>();

        // === TALES OF THE BRAVE EVENTS ===
        var braveBook = storybooks.First(s => s.Name == "Tales of the Brave");
        storybookEvents.AddRange(new[]
        {
            new RandomEvent { StorybookId = braveBook.Id, Title = "Warrior's Spirit", Description = "Reading tales of valor fills you with fighting spirit.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Train,Battle", BaseProbability = 0.12, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Channel the courage", ResultDescription = "You feel stronger!", StrengthChange = 2, EnduranceChange = 1, ExperienceChange = 10, DisplayOrder = 1 },
                    new() { Text = "Meditate on the lessons", ResultDescription = "Wisdom from warriors past.", IntelligenceChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = braveBook.Id, Title = "Battle Memory", Description = "A vivid memory of an ancient battle flashes before your eyes.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Neutral, TriggerActions = "Rest,Explore", BaseProbability = 0.10, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Study the tactics", ResultDescription = "You learn from the battle.", StrengthChange = 1, IntelligenceChange = 1, DisplayOrder = 1 },
                    new() { Text = "Feel the warriors' pain", ResultDescription = "Their sacrifice empowers you.", EnduranceChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = braveBook.Id, Title = "Hero's Echo", Description = "The spirit of a fallen hero speaks to you from the book.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest", BaseProbability = 0.08, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept his blessing", ResultDescription = "The hero's strength flows into you!", StrengthChange = 4, EnduranceChange = 2, ExperienceChange = 20, DisplayOrder = 1 },
                    new() { Text = "Ask for wisdom", ResultDescription = "He shares secrets of combat.", StrengthChange = 2, IntelligenceChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = braveBook.Id, Title = "Legendary Weapon Vision", Description = "You dream of a legendary weapon waiting to be claimed.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Explore", BaseProbability = 0.05, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Reach for the weapon", ResultDescription = "Its power surges through you!", CheckStat = StatType.Strength, CheckDifficulty = 50,
                        StrengthChange = 6, EnduranceChange = 3, ExperienceChange = 35, FailureDescription = "The weapon fades away.", FailureStrengthChange = 1, DisplayOrder = 1 },
                    new() { Text = "Bow before it", ResultDescription = "The weapon acknowledges your respect.", StrengthChange = 3, CharismaChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = braveBook.Id, Title = "Trial of the Champion", Description = "A spectral arena materializes, challenging you to prove your worth.",
                Rarity = Rarity.Epic, OutcomeType = EventOutcome.Positive, TriggerActions = "Train,Battle", BaseProbability = 0.03, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the trial", ResultDescription = "You emerge victorious, transformed!", CheckStat = StatType.Strength, CheckDifficulty = 60,
                        StrengthChange = 10, EnduranceChange = 5, AgilityChange = 3, ExperienceChange = 50, ReputationChange = 15,
                        FailureDescription = "You fall, but learn from defeat.", FailureStrengthChange = 2, FailureEnduranceChange = 2, DisplayOrder = 1 },
                    new() { Text = "Observe and learn", ResultDescription = "Watching teaches much.", StrengthChange = 4, IntelligenceChange = 4, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = braveBook.Id, Title = "Blessing of the War God", Description = "A divine presence notices your courage and offers a gift.",
                Rarity = Rarity.Legendary, OutcomeType = EventOutcome.Positive, TriggerActions = "Battle", BaseProbability = 0.01, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the blessing", ResultDescription = "Divine power courses through your veins!",
                        StrengthChange = 15, EnduranceChange = 10, AgilityChange = 5, ExperienceChange = 100, ReputationChange = 25, DisplayOrder = 1 },
                    new() { Text = "Request wisdom instead", ResultDescription = "The god shares ancient battle knowledge.",
                        StrengthChange = 8, IntelligenceChange = 10, CharismaChange = 5, ExperienceChange = 75, DisplayOrder = 2 }
                }}
        });

        // === WHISPERS OF THE WIND EVENTS ===
        var windBook = storybooks.First(s => s.Name == "Whispers of the Wind");
        storybookEvents.AddRange(new[]
        {
            new RandomEvent { StorybookId = windBook.Id, Title = "Swift Breeze", Description = "A gentle wind carries whispers of speed and grace.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Train,Explore", BaseProbability = 0.12, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Run with the wind", ResultDescription = "You feel lighter and faster!", AgilityChange = 2, LuckChange = 1, ExperienceChange = 10, DisplayOrder = 1 },
                    new() { Text = "Listen to the whispers", ResultDescription = "Ancient secrets of movement.", AgilityChange = 1, IntelligenceChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = windBook.Id, Title = "Phantom Footsteps", Description = "You see ghostly footprints showing a perfect path.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Neutral, TriggerActions = "Explore", BaseProbability = 0.10, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Follow the path", ResultDescription = "The path leads to treasure!", AgilityChange = 1, GoldChange = 20, DisplayOrder = 1 },
                    new() { Text = "Study the technique", ResultDescription = "You learn to move more efficiently.", AgilityChange = 2, EnduranceChange = 1, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = windBook.Id, Title = "Wind Dancer's Gift", Description = "A spectral dancer offers to teach you their art.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Train,Rest", BaseProbability = 0.08, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Learn to dance", ResultDescription = "Your movements become fluid!", AgilityChange = 4, CharismaChange = 2, ExperienceChange = 20, DisplayOrder = 1 },
                    new() { Text = "Watch and adapt", ResultDescription = "You incorporate elements into combat.", AgilityChange = 3, StrengthChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = windBook.Id, Title = "Storm Rider's Secret", Description = "You glimpse a legendary technique for riding the wind itself.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore,Train", BaseProbability = 0.05, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Attempt the technique", ResultDescription = "You briefly soar through the air!", CheckStat = StatType.Agility, CheckDifficulty = 50,
                        AgilityChange = 6, LuckChange = 4, ExperienceChange = 35, FailureDescription = "You stumble but learn.", FailureAgilityChange = 2, DisplayOrder = 1 },
                    new() { Text = "Memorize for later", ResultDescription = "The knowledge stays with you.", AgilityChange = 3, IntelligenceChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = windBook.Id, Title = "Avatar of the Wind", Description = "The wind itself takes form and challenges you to a race.",
                Rarity = Rarity.Epic, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore,Battle", BaseProbability = 0.03, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Race the wind", ResultDescription = "You match its speed!", CheckStat = StatType.Agility, CheckDifficulty = 65,
                        AgilityChange = 10, LuckChange = 5, EnduranceChange = 3, ExperienceChange = 50, ReputationChange = 15,
                        FailureDescription = "You lose but improve.", FailureAgilityChange = 3, FailureLuckChange = 2, DisplayOrder = 1 },
                    new() { Text = "Ask for teachings", ResultDescription = "The wind shares its secrets.", AgilityChange = 5, IntelligenceChange = 4, LuckChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = windBook.Id, Title = "Blessing of the Sky Spirit", Description = "An ancient sky deity recognizes your potential.",
                Rarity = Rarity.Legendary, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore", BaseProbability = 0.01, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the gift of flight", ResultDescription = "You become one with the wind!",
                        AgilityChange = 15, LuckChange = 10, EnduranceChange = 5, ExperienceChange = 100, ReputationChange = 25, DisplayOrder = 1 },
                    new() { Text = "Request foresight", ResultDescription = "You can sense what the wind knows.",
                        AgilityChange = 8, IntelligenceChange = 8, LuckChange = 8, ExperienceChange = 75, DisplayOrder = 2 }
                }}
        });

        // === GRIMOIRE OF SHADOWS EVENTS ===
        var shadowBook = storybooks.First(s => s.Name == "Grimoire of Shadows");
        storybookEvents.AddRange(new[]
        {
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Whispered Secret", Description = "The grimoire reveals a minor arcane secret.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Train", BaseProbability = 0.12, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Study the secret", ResultDescription = "Your mind expands!", IntelligenceChange = 2, CharismaChange = 1, ExperienceChange = 10, DisplayOrder = 1 },
                    new() { Text = "Practice the cantrip", ResultDescription = "A small spell, but useful.", IntelligenceChange = 1, LuckChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Shadow's Warning", Description = "The shadows around you flicker with a message.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Neutral, TriggerActions = "Explore,Battle", BaseProbability = 0.10, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Heed the warning", ResultDescription = "You avoid danger and find opportunity.", IntelligenceChange = 1, GoldChange = 15, DisplayOrder = 1 },
                    new() { Text = "Investigate the source", ResultDescription = "You learn to read the shadows.", IntelligenceChange = 2, AgilityChange = 1, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Dark Knowledge", Description = "A page reveals itself, showing forbidden knowledge.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest", BaseProbability = 0.08, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Absorb the knowledge", ResultDescription = "Power floods your mind!", IntelligenceChange = 5, CharismaChange = 2, ExperienceChange = 25, DisplayOrder = 1 },
                    new() { Text = "Resist and close the book", ResultDescription = "You maintain your purity.", EnduranceChange = 3, CharismaChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Shadow Familiar", Description = "A creature of darkness offers to serve you.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Explore", BaseProbability = 0.05, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the familiar", ResultDescription = "It enhances your magical abilities!", CheckStat = StatType.Intelligence, CheckDifficulty = 50,
                        IntelligenceChange = 7, AgilityChange = 3, ExperienceChange = 40, FailureDescription = "It's too powerful to control.", FailureIntelligenceChange = 2, DisplayOrder = 1 },
                    new() { Text = "Bind it to a task", ResultDescription = "It brings you treasures.", IntelligenceChange = 3, GoldChange = 50, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Gate to the Shadow Realm", Description = "A portal opens to a dimension of pure magic.",
                Rarity = Rarity.Epic, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest", BaseProbability = 0.03, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Enter the realm", ResultDescription = "You return transformed!", CheckStat = StatType.Intelligence, CheckDifficulty = 60,
                        IntelligenceChange = 12, CharismaChange = 5, AgilityChange = 3, ExperienceChange = 60, ReputationChange = 20,
                        FailureDescription = "The realm tests you harshly.", FailureIntelligenceChange = 3, FailureHealthChange = -15, DisplayOrder = 1 },
                    new() { Text = "Draw power from afar", ResultDescription = "Safer, but still powerful.", IntelligenceChange = 6, LuckChange = 4, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = shadowBook.Id, Title = "The Archmage's Legacy", Description = "The original author's spirit awakens within the grimoire.",
                Rarity = Rarity.Legendary, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest", BaseProbability = 0.01, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the legacy", ResultDescription = "Centuries of knowledge become yours!",
                        IntelligenceChange = 18, CharismaChange = 8, LuckChange = 5, ExperienceChange = 120, ReputationChange = 30, DisplayOrder = 1 },
                    new() { Text = "Request a specific teaching", ResultDescription = "You master one school of magic.",
                        IntelligenceChange = 12, StrengthChange = 6, EnduranceChange = 6, ExperienceChange = 80, DisplayOrder = 2 }
                }}
        });

        // === CHRONICLE OF FORTUNE EVENTS ===
        var fortuneBook = storybooks.First(s => s.Name == "Chronicle of Fortune");
        storybookEvents.AddRange(new[]
        {
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Lucky Find", Description = "You stumble upon something valuable.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore", BaseProbability = 0.14, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Pocket the treasure", ResultDescription = "Gold coins!", LuckChange = 1, GoldChange = 25, DisplayOrder = 1 },
                    new() { Text = "Look for more", ResultDescription = "Your luck continues!", LuckChange = 2, GoldChange = 15, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Fortunate Timing", Description = "You arrive at exactly the right moment.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore,Battle", BaseProbability = 0.12, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Seize the opportunity", ResultDescription = "Perfect timing!", LuckChange = 2, ExperienceChange = 15, DisplayOrder = 1 },
                    new() { Text = "Help others first", ResultDescription = "Your kindness is rewarded.", LuckChange = 1, CharismaChange = 2, ReputationChange = 5, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Gambler's Intuition", Description = "You feel incredibly lucky.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Explore", BaseProbability = 0.09, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Test your luck", ResultDescription = "Fortune favors you!", CheckStat = StatType.Luck, CheckDifficulty = 40,
                        LuckChange = 5, GoldChange = 40, ExperienceChange = 20, FailureDescription = "Not this time.", FailureLuckChange = 1, DisplayOrder = 1 },
                    new() { Text = "Save it for later", ResultDescription = "The feeling lingers.", LuckChange = 3, EnduranceChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Twist of Fate", Description = "Destiny itself seems to bend around you.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Battle,Explore", BaseProbability = 0.06, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Embrace the twist", ResultDescription = "Reality shifts in your favor!", LuckChange = 8, CharismaChange = 4, ExperienceChange = 40, DisplayOrder = 1 },
                    new() { Text = "Redirect it to others", ResultDescription = "Your generosity impresses fate.", LuckChange = 4, ReputationChange = 15, CharismaChange = 4, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Fortune's Wheel", Description = "A golden wheel of fate appears before you.",
                Rarity = Rarity.Epic, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Explore", BaseProbability = 0.04, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Spin the wheel", ResultDescription = "The wheel grants incredible fortune!", CheckStat = StatType.Luck, CheckDifficulty = 55,
                        LuckChange = 12, GoldChange = 100, ExperienceChange = 60, ReputationChange = 20,
                        FailureDescription = "The wheel grants a smaller gift.", FailureLuckChange = 4, FailureGoldChange = 30, DisplayOrder = 1 },
                    new() { Text = "Ask for steady fortune", ResultDescription = "Consistent luck flows to you.", LuckChange = 7, CharismaChange = 5, EnduranceChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Avatar of Fortune", Description = "Lady Luck herself manifests before you.",
                Rarity = Rarity.Legendary, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest", BaseProbability = 0.01, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Request her blessing", ResultDescription = "Fortune will forever favor you!",
                        LuckChange = 20, CharismaChange = 10, GoldChange = 200, ExperienceChange = 150, ReputationChange = 30, DisplayOrder = 1 },
                    new() { Text = "Ask for others' fortune", ResultDescription = "Your selflessness moves her deeply.",
                        LuckChange = 12, CharismaChange = 15, ReputationChange = 50, ExperienceChange = 100, DisplayOrder = 2 }
                }}
        });

        // === EPIC OF THE DRAGON SLAYER EVENTS ===
        var dragonBook = storybooks.First(s => s.Name == "Epic of the Dragon Slayer");
        storybookEvents.AddRange(new[]
        {
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Dragon's Roar", Description = "You hear a distant roar that stirs your blood.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore,Battle", BaseProbability = 0.12, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Let it fuel your courage", ResultDescription = "Your resolve strengthens!", StrengthChange = 2, EnduranceChange = 1, ExperienceChange = 12, DisplayOrder = 1 },
                    new() { Text = "Study the direction", ResultDescription = "You learn to track dragons.", IntelligenceChange = 2, AgilityChange = 1, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Scale Fragment", Description = "You find a piece of dragon scale.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore", BaseProbability = 0.10, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Absorb its essence", ResultDescription = "Dragon power flows through you!", StrengthChange = 1, EnduranceChange = 2, DisplayOrder = 1 },
                    new() { Text = "Sell it", ResultDescription = "Dragon scales are valuable!", GoldChange = 35, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Slayer's Memory", Description = "You experience a dragon slayer's final battle.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Train", BaseProbability = 0.08, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Fight as they did", ResultDescription = "Their techniques become yours!", StrengthChange = 4, AgilityChange = 3, ExperienceChange = 25, DisplayOrder = 1 },
                    new() { Text = "Learn their strategy", ResultDescription = "You understand how to fight giants.", IntelligenceChange = 4, StrengthChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Dragon Blood Blessing", Description = "The book pulses with ancient dragon blood magic.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Battle", BaseProbability = 0.05, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the blood magic", ResultDescription = "Dragon fire burns in your veins!", CheckStat = StatType.Endurance, CheckDifficulty = 50,
                        StrengthChange = 6, EnduranceChange = 5, AgilityChange = 3, ExperienceChange = 45,
                        FailureDescription = "The power overwhelms briefly.", FailureStrengthChange = 2, FailureHealthChange = -10, DisplayOrder = 1 },
                    new() { Text = "Request protection only", ResultDescription = "Dragon scales form on your skin.", EnduranceChange = 7, StrengthChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Spirit of the Last Dragon", Description = "An ancient dragon's spirit emerges from the book.",
                Rarity = Rarity.Epic, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest", BaseProbability = 0.03, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Challenge the spirit", ResultDescription = "You prove your worth as a true slayer!", CheckStat = StatType.Strength, CheckDifficulty = 65,
                        StrengthChange = 12, EnduranceChange = 8, AgilityChange = 5, ExperienceChange = 70, ReputationChange = 25,
                        FailureDescription = "The dragon teaches through defeat.", FailureStrengthChange = 4, FailureEnduranceChange = 4, DisplayOrder = 1 },
                    new() { Text = "Seek wisdom, not battle", ResultDescription = "The dragon shares ancient knowledge.", IntelligenceChange = 8, StrengthChange = 6, CharismaChange = 4, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Ascension of the Dragon Knight", Description = "The greatest dragon slayer offers you their mantle.",
                Rarity = Rarity.Legendary, OutcomeType = EventOutcome.Positive, TriggerActions = "Battle", BaseProbability = 0.01, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the mantle", ResultDescription = "You become the legendary Dragon Knight!",
                        StrengthChange = 15, EnduranceChange = 12, AgilityChange = 8, ExperienceChange = 150, ReputationChange = 40, DisplayOrder = 1 },
                    new() { Text = "Forge your own legend", ResultDescription = "You create a new path of power.",
                        StrengthChange = 10, AgilityChange = 10, IntelligenceChange = 8, LuckChange = 8, ExperienceChange = 120, DisplayOrder = 2 }
                }}
        });

        await context.RandomEvents.AddRangeAsync(storybookEvents);
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
