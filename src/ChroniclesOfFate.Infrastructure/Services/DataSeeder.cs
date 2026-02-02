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

        // Skills must be seeded BEFORE events so events can grant skills
        if (!await context.Skills.AnyAsync())
        {
            await SeedSkillsAsync(context);
            await context.SaveChangesAsync();
        }

        if (!await context.RandomEvents.AnyAsync())
        {
            await SeedRandomEventsAsync(context);
        }

        // Link skills to events after both are seeded
        await LinkSkillsToEventsAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task LinkSkillsToEventsAsync(ApplicationDbContext context)
    {
        // Get skills by name for linking
        var skills = await context.Skills.ToListAsync();
        var skillByName = skills.ToDictionary(s => s.Name, s => s.Id);

        // Link specific events to skills based on event title and choice
        var events = await context.RandomEvents
            .Include(e => e.Choices)
            .Where(e => e.Choices.Any(c => c.GrantSkillId == null))
            .ToListAsync();

        foreach (var ev in events)
        {
            switch (ev.Title)
            {
                // Tales of the Brave - grants combat skills
                case "Trial of the Champion":
                    var trialChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Accept the trial"));
                    if (trialChoice != null && skillByName.TryGetValue("Counter Strike", out var counterId))
                        trialChoice.GrantSkillId = counterId;
                    break;

                case "Blessing of the War God":
                    var warGodChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Accept the blessing"));
                    if (warGodChoice != null && skillByName.TryGetValue("Berserker Rage", out var berserkerId))
                        warGodChoice.GrantSkillId = berserkerId;
                    break;

                case "Warlord's Inheritance":
                    var warlordChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Claim the inheritance"));
                    if (warlordChoice != null && skillByName.TryGetValue("Power Strike", out var powerStrikeId))
                        warlordChoice.GrantSkillId = powerStrikeId;
                    break;

                // Whispers of the Wind - grants agility/evasion skills
                case "Avatar of the Wind":
                    var windChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Race the wind"));
                    if (windChoice != null && skillByName.TryGetValue("Quick Reflexes", out var reflexesId))
                        windChoice.GrantSkillId = reflexesId;
                    break;

                case "Blessing of the Sky Spirit":
                    var skyChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Accept the gift"));
                    if (skyChoice != null && skillByName.TryGetValue("Shadow Step", out var shadowStepId))
                        skyChoice.GrantSkillId = shadowStepId;
                    break;

                case "Windwalker's Ascension":
                    var windwalkerChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Accept the mantle"));
                    if (windwalkerChoice != null && skillByName.TryGetValue("Swift Slash", out var swiftSlashId))
                        windwalkerChoice.GrantSkillId = swiftSlashId;
                    break;

                // Grimoire of Shadows - grants magic skills
                case "Gate to the Shadow Realm":
                    var gateChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Enter the realm"));
                    if (gateChoice != null && skillByName.TryGetValue("Arcane Bolt", out var arcaneBoltId))
                        gateChoice.GrantSkillId = arcaneBoltId;
                    break;

                case "The Archmage's Legacy":
                    var archmageChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Accept the legacy"));
                    if (archmageChoice != null && skillByName.TryGetValue("Divine Smite", out var divineSmiteId))
                        archmageChoice.GrantSkillId = divineSmiteId;
                    break;

                case "Library of Lost Souls":
                    var libraryChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Browse the collection"));
                    if (libraryChoice != null && skillByName.TryGetValue("Inferno Blast", out var infernoId))
                        libraryChoice.GrantSkillId = infernoId;
                    break;

                case "Shadow Familiar":
                    var familiarChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Accept the familiar"));
                    if (familiarChoice != null && skillByName.TryGetValue("Wisdom Seeker", out var wisdomId))
                        familiarChoice.GrantSkillId = wisdomId;
                    break;

                // Chronicle of Fortune - grants luck/bonus skills
                case "Fortune's Wheel":
                    var wheelChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Spin the wheel"));
                    if (wheelChoice != null && skillByName.TryGetValue("Fortune's Favor", out var fortuneId))
                        wheelChoice.GrantSkillId = fortuneId;
                    break;

                case "Avatar of Fortune":
                    var avatarChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Request her blessing"));
                    if (avatarChoice != null && skillByName.TryGetValue("Destiny's Champion", out var destinyId))
                        avatarChoice.GrantSkillId = destinyId;
                    break;

                case "Destiny Unbound":
                    var destinyChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Seize control"));
                    if (destinyChoice != null && skillByName.TryGetValue("Lucky Star", out var luckyStarId))
                        destinyChoice.GrantSkillId = luckyStarId;
                    break;

                // Epic of the Dragon Slayer - grants powerful combat skills
                case "Spirit of the Last Dragon":
                    var spiritChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Challenge the spirit"));
                    if (spiritChoice != null && skillByName.TryGetValue("Crushing Blow", out var crushingId))
                        spiritChoice.GrantSkillId = crushingId;
                    break;

                case "Ascension of the Dragon Knight":
                    var knightChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Accept the mantle"));
                    if (knightChoice != null && skillByName.TryGetValue("Assassin's Mark", out var assassinId))
                        knightChoice.GrantSkillId = assassinId;
                    break;

                case "Dragonlord's Throne":
                    var throneChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Claim the throne"));
                    if (throneChoice != null && skillByName.TryGetValue("Iron Skin", out var ironSkinId))
                        throneChoice.GrantSkillId = ironSkinId;
                    break;

                case "Dragon Blood Blessing":
                    var bloodChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Accept the blood"));
                    if (bloodChoice != null && skillByName.TryGetValue("Vampiric Touch", out var vampiricId))
                        bloodChoice.GrantSkillId = vampiricId;
                    break;

                case "Heart of the Dragon":
                    var heartChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Accept the power"));
                    if (heartChoice != null && skillByName.TryGetValue("Regeneration", out var regenId))
                        heartChoice.GrantSkillId = regenId;
                    break;

                // Global events
                case "Training Insight":
                    var insightChoice = ev.Choices.FirstOrDefault(c => c.Text.Contains("Focus on the insight"));
                    if (insightChoice != null && skillByName.TryGetValue("Master's Guidance", out var masterId))
                        insightChoice.GrantSkillId = masterId;
                    break;
            }
        }
    }

    private static async Task SeedTrainingScenariosAsync(ApplicationDbContext context)
    {
        var scenarios = new List<TrainingScenario>
        {
            // One training per stat, 15 energy cost each
            new() { Name = "Strength Training", Description = "Intense combat drills and weight training to build raw power.",
                PrimaryStat = StatType.Strength, SecondaryStat = StatType.Endurance,
                BaseStatGain = 5, SecondaryStatGain = 2, EnergyCost = 15, BonusChance = 0.15, BonusStatGain = 3,
                FailureChance = 0.05, FailureHealthPenalty = 8, ExperienceGain = 15, RequiredLevel = 1,
                TrainingNarrative = "You push your body to its limits with intense physical training.", IsActive = true },

            new() { Name = "Agility Training", Description = "Speed drills and acrobatics to enhance reflexes and coordination.",
                PrimaryStat = StatType.Agility, SecondaryStat = StatType.Luck,
                BaseStatGain = 5, SecondaryStatGain = 2, EnergyCost = 15, BonusChance = 0.15, BonusStatGain = 3,
                FailureChance = 0.06, FailureHealthPenalty = 6, ExperienceGain = 15, RequiredLevel = 1,
                TrainingNarrative = "You practice swift movements and lightning-fast reflexes.", IsActive = true },

            new() { Name = "Intelligence Training", Description = "Study arcane texts and solve complex puzzles to sharpen your mind.",
                PrimaryStat = StatType.Intelligence, SecondaryStat = StatType.Charisma,
                BaseStatGain = 5, SecondaryStatGain = 2, EnergyCost = 15, BonusChance = 0.15, BonusStatGain = 3,
                FailureChance = 0.03, FailureHealthPenalty = 3, ExperienceGain = 15, RequiredLevel = 1,
                TrainingNarrative = "You immerse yourself in knowledge and mental exercises.", IsActive = true,
                BonusSeasons = "Autumn,Winter", SeasonalBonusMultiplier = 1.25 },

            new() { Name = "Endurance Training", Description = "Long-distance running and stamina exercises to build resilience.",
                PrimaryStat = StatType.Endurance, SecondaryStat = StatType.Strength,
                BaseStatGain = 5, SecondaryStatGain = 2, EnergyCost = 15, BonusChance = 0.15, BonusStatGain = 3,
                FailureChance = 0.04, FailureHealthPenalty = 5, ExperienceGain = 15, RequiredLevel = 1,
                TrainingNarrative = "You test your limits with grueling endurance exercises.", IsActive = true,
                BonusSeasons = "Spring,Summer", SeasonalBonusMultiplier = 1.2 },

            new() { Name = "Charisma Training", Description = "Practice public speaking and social interactions to improve your presence.",
                PrimaryStat = StatType.Charisma, SecondaryStat = StatType.Intelligence,
                BaseStatGain = 5, SecondaryStatGain = 2, EnergyCost = 15, BonusChance = 0.15, BonusStatGain = 3,
                FailureChance = 0.03, FailureHealthPenalty = 2, ExperienceGain = 15, RequiredLevel = 1,
                TrainingNarrative = "You work on your social skills and learn to influence others.", IsActive = true },

            new() { Name = "Luck Training", Description = "Test fate with games of chance and learn to read fortune's signs.",
                PrimaryStat = StatType.Luck, SecondaryStat = StatType.Agility,
                BaseStatGain = 5, SecondaryStatGain = 2, EnergyCost = 15, BonusChance = 0.20, BonusStatGain = 4,
                FailureChance = 0.08, FailureHealthPenalty = 4, ExperienceGain = 15, RequiredLevel = 1,
                TrainingNarrative = "You tempt fate and learn to recognize fortune's favor.", IsActive = true }
        };
        await context.TrainingScenarios.AddRangeAsync(scenarios);
    }

    private static async Task SeedEnemiesAsync(ApplicationDbContext context)
    {
        var enemies = new List<Enemy>
        {
            // === TIER 1-5: EARLY GAME (Level 1-10) ===
            new() { Name = "Forest Goblin", Description = "A mischievous creature lurking in the woods.",
                Strength = 20, Agility = 30, Intelligence = 10, Endurance = 15, Health = 50,
                DifficultyTier = 1, EnemyType = "Creature", ExperienceReward = 25, GoldReward = 10, ReputationReward = 5 },
            new() { Name = "Giant Rat", Description = "An oversized rodent with sharp teeth.",
                Strength = 15, Agility = 35, Intelligence = 5, Endurance = 10, Health = 40,
                DifficultyTier = 1, EnemyType = "Beast", ExperienceReward = 20, GoldReward = 5, ReputationReward = 3 },
            new() { Name = "Bandit Thug", Description = "A common criminal preying on travelers.",
                Strength = 35, Agility = 25, Intelligence = 15, Endurance = 30, Health = 70,
                DifficultyTier = 2, EnemyType = "Humanoid", ExperienceReward = 40, GoldReward = 25, ReputationReward = 10 },
            new() { Name = "Wild Wolf", Description = "A fierce predator of the wilderness.",
                Strength = 40, Agility = 50, Intelligence = 15, Endurance = 35, Health = 60,
                DifficultyTier = 2, EnemyType = "Beast", ExperienceReward = 35, GoldReward = 5, ReputationReward = 8 },
            new() { Name = "Skeleton Warrior", Description = "An undead soldier animated by dark magic.",
                Strength = 45, Agility = 30, Intelligence = 10, Endurance = 40, Health = 65,
                DifficultyTier = 3, EnemyType = "Undead", ExperienceReward = 45, GoldReward = 20, ReputationReward = 12 },
            new() { Name = "Orc Scout", Description = "A quick and cunning orc tracker.",
                Strength = 50, Agility = 45, Intelligence = 25, Endurance = 40, Health = 80,
                DifficultyTier = 3, EnemyType = "Humanoid", ExperienceReward = 50, GoldReward = 30, ReputationReward = 12 },
            new() { Name = "Dire Wolf", Description = "A massive wolf with supernatural strength.",
                Strength = 55, Agility = 60, Intelligence = 20, Endurance = 45, Health = 90,
                DifficultyTier = 4, EnemyType = "Beast", ExperienceReward = 60, GoldReward = 15, ReputationReward = 15 },
            new() { Name = "Bandit Leader", Description = "A ruthless commander of highway robbers.",
                Strength = 60, Agility = 50, Intelligence = 40, Endurance = 55, Health = 100,
                DifficultyTier = 5, EnemyType = "Humanoid", ExperienceReward = 75, GoldReward = 60, ReputationReward = 20 },

            // === TIER 6-10: EARLY-MID GAME (Level 11-20) ===
            new() { Name = "Orc Warrior", Description = "A brutal fighter from the mountain tribes.",
                Strength = 70, Agility = 35, Intelligence = 20, Endurance = 65, Health = 120,
                DifficultyTier = 6, EnemyType = "Humanoid", ExperienceReward = 90, GoldReward = 50, ReputationReward = 25 },
            new() { Name = "Zombie Brute", Description = "A powerful undead monstrosity.",
                Strength = 80, Agility = 20, Intelligence = 5, Endurance = 90, Health = 150,
                DifficultyTier = 7, EnemyType = "Undead", ExperienceReward = 100, GoldReward = 30, ReputationReward = 28 },
            new() { Name = "Dark Mage", Description = "A practitioner of forbidden magic.",
                Strength = 30, Agility = 40, Intelligence = 90, Endurance = 35, Health = 80,
                DifficultyTier = 8, EnemyType = "Humanoid", ExperienceReward = 110, GoldReward = 70, ReputationReward = 30 },
            new() { Name = "Werewolf", Description = "A cursed human transformed into a beast.",
                Strength = 85, Agility = 75, Intelligence = 30, Endurance = 70, Health = 130,
                DifficultyTier = 9, EnemyType = "Beast", ExperienceReward = 130, GoldReward = 45, ReputationReward = 35 },
            new() { Name = "Cave Troll", Description = "A massive creature of immense strength.",
                Strength = 100, Agility = 20, Intelligence = 10, Endurance = 95, Health = 180,
                DifficultyTier = 10, EnemyType = "Giant", ExperienceReward = 150, GoldReward = 60, ReputationReward = 40 },

            // === TIER 11-15: MID GAME (Level 21-30) ===
            new() { Name = "Shadow Assassin", Description = "A deadly killer from the shadows.",
                Strength = 70, Agility = 110, Intelligence = 70, Endurance = 50, Health = 100,
                DifficultyTier = 11, EnemyType = "Humanoid", ExperienceReward = 170, GoldReward = 90, ReputationReward = 45 },
            new() { Name = "Orc Warchief", Description = "A fearsome leader of orc war bands.",
                Strength = 110, Agility = 50, Intelligence = 45, Endurance = 100, Health = 200,
                DifficultyTier = 12, EnemyType = "Humanoid", ExperienceReward = 190, GoldReward = 100, ReputationReward = 50 },
            new() { Name = "Vampire Spawn", Description = "A lesser vampire with insatiable hunger.",
                Strength = 90, Agility = 95, Intelligence = 80, Endurance = 75, Health = 150,
                DifficultyTier = 13, EnemyType = "Undead", ExperienceReward = 210, GoldReward = 80, ReputationReward = 55 },
            new() { Name = "Stone Golem", Description = "An animated construct of solid rock.",
                Strength = 130, Agility = 15, Intelligence = 5, Endurance = 150, Health = 250,
                DifficultyTier = 14, EnemyType = "Construct", ExperienceReward = 230, GoldReward = 50, ReputationReward = 60 },
            new() { Name = "Dragon Wyrmling", Description = "A young dragon, dangerous despite its age.",
                Strength = 120, Agility = 70, Intelligence = 60, Endurance = 110, Health = 220,
                DifficultyTier = 15, EnemyType = "Dragon", ExperienceReward = 250, GoldReward = 150, ReputationReward = 65 },

            // === TIER 16-20: MID-LATE GAME (Level 31-40) ===
            new() { Name = "Lich Acolyte", Description = "An undead spellcaster serving a greater power.",
                Strength = 50, Agility = 60, Intelligence = 140, Endurance = 80, Health = 180,
                DifficultyTier = 16, EnemyType = "Undead", ExperienceReward = 280, GoldReward = 120, ReputationReward = 70 },
            new() { Name = "Minotaur Champion", Description = "A legendary beast-warrior of the labyrinth.",
                Strength = 150, Agility = 70, Intelligence = 40, Endurance = 130, Health = 280,
                DifficultyTier = 17, EnemyType = "Beast", ExperienceReward = 310, GoldReward = 100, ReputationReward = 75 },
            new() { Name = "Demon Soldier", Description = "A fiendish warrior from the lower planes.",
                Strength = 140, Agility = 90, Intelligence = 70, Endurance = 120, Health = 260,
                DifficultyTier = 18, EnemyType = "Demon", ExperienceReward = 340, GoldReward = 130, ReputationReward = 80 },
            new() { Name = "Frost Giant", Description = "A towering giant from the frozen wastes.",
                Strength = 170, Agility = 40, Intelligence = 50, Endurance = 160, Health = 350,
                DifficultyTier = 19, EnemyType = "Giant", ExperienceReward = 370, GoldReward = 140, ReputationReward = 85 },
            new() { Name = "Adult Dragon", Description = "A fully grown dragon of terrible power.",
                Strength = 180, Agility = 100, Intelligence = 120, Endurance = 170, Health = 400,
                DifficultyTier = 20, EnemyType = "Dragon", ExperienceReward = 400, GoldReward = 250, ReputationReward = 90 },

            // === TIER 21-30: LATE GAME (Level 41-50) ===
            new() { Name = "Death Knight", Description = "A fallen paladin corrupted by darkness.",
                Strength = 160, Agility = 110, Intelligence = 100, Endurance = 150, Health = 320,
                DifficultyTier = 21, EnemyType = "Undead", ExperienceReward = 430, GoldReward = 180, ReputationReward = 95 },
            new() { Name = "Vampire Lord", Description = "An ancient vampire of immense power.",
                Strength = 150, Agility = 140, Intelligence = 150, Endurance = 130, Health = 300,
                DifficultyTier = 22, EnemyType = "Undead", ExperienceReward = 460, GoldReward = 200, ReputationReward = 100 },
            new() { Name = "Iron Golem", Description = "A massive construct of enchanted metal.",
                Strength = 200, Agility = 30, Intelligence = 10, Endurance = 220, Health = 450,
                DifficultyTier = 23, EnemyType = "Construct", ExperienceReward = 490, GoldReward = 150, ReputationReward = 105 },
            new() { Name = "Pit Fiend", Description = "A greater demon of the infernal hierarchy.",
                Strength = 190, Agility = 120, Intelligence = 140, Endurance = 180, Health = 420,
                DifficultyTier = 24, EnemyType = "Demon", ExperienceReward = 520, GoldReward = 220, ReputationReward = 110 },
            new() { Name = "Storm Giant", Description = "A giant commanding the power of storms.",
                Strength = 210, Agility = 80, Intelligence = 100, Endurance = 200, Health = 480,
                DifficultyTier = 25, EnemyType = "Giant", ExperienceReward = 550, GoldReward = 200, ReputationReward = 115 },
            new() { Name = "Lich Lord", Description = "An ancient undead sorcerer of terrible power.",
                Strength = 100, Agility = 90, Intelligence = 220, Endurance = 150, Health = 350,
                DifficultyTier = 26, EnemyType = "Undead", ExperienceReward = 580, GoldReward = 250, ReputationReward = 120 },
            new() { Name = "Ancient Dragon", Description = "A dragon of immense age and wisdom.",
                Strength = 230, Agility = 130, Intelligence = 180, Endurance = 220, Health = 550,
                DifficultyTier = 27, EnemyType = "Dragon", ExperienceReward = 620, GoldReward = 350, ReputationReward = 125 },
            new() { Name = "Demon Prince", Description = "A ruler of a demonic domain.",
                Strength = 240, Agility = 150, Intelligence = 170, Endurance = 230, Health = 520,
                DifficultyTier = 28, EnemyType = "Demon", ExperienceReward = 660, GoldReward = 300, ReputationReward = 130 },
            new() { Name = "Titan", Description = "A primordial being of godlike power.",
                Strength = 280, Agility = 100, Intelligence = 150, Endurance = 280, Health = 650,
                DifficultyTier = 29, EnemyType = "Giant", ExperienceReward = 700, GoldReward = 350, ReputationReward = 140 },
            new() { Name = "Elder Dragon", Description = "The eldest and most powerful of dragonkind.",
                Strength = 300, Agility = 160, Intelligence = 250, Endurance = 290, Health = 700,
                DifficultyTier = 30, EnemyType = "Dragon", ExperienceReward = 750, GoldReward = 500, ReputationReward = 150 },

            // === TIER 31-40: ENDGAME (Legendary) ===
            new() { Name = "Archlich", Description = "A lich who has transcended normal undeath.",
                Strength = 150, Agility = 120, Intelligence = 300, Endurance = 200, Health = 500,
                DifficultyTier = 32, EnemyType = "Undead", ExperienceReward = 820, GoldReward = 400, ReputationReward = 160 },
            new() { Name = "Void Walker", Description = "A being from beyond reality itself.",
                Strength = 250, Agility = 200, Intelligence = 220, Endurance = 240, Health = 580,
                DifficultyTier = 34, EnemyType = "Aberration", ExperienceReward = 900, GoldReward = 400, ReputationReward = 175 },
            new() { Name = "Primordial Elemental", Description = "A manifestation of pure elemental chaos.",
                Strength = 280, Agility = 180, Intelligence = 100, Endurance = 300, Health = 700,
                DifficultyTier = 36, EnemyType = "Elemental", ExperienceReward = 980, GoldReward = 350, ReputationReward = 190 },
            new() { Name = "Abyssal Lord", Description = "A supreme ruler of the demonic abyss.",
                Strength = 320, Agility = 200, Intelligence = 250, Endurance = 310, Health = 750,
                DifficultyTier = 38, EnemyType = "Demon", ExperienceReward = 1060, GoldReward = 500, ReputationReward = 200 },
            new() { Name = "Aspect of Death", Description = "A physical manifestation of mortality itself.",
                Strength = 350, Agility = 250, Intelligence = 300, Endurance = 340, Health = 800,
                DifficultyTier = 40, EnemyType = "Divine", ExperienceReward = 1150, GoldReward = 600, ReputationReward = 220 },

            // === TIER 41-50: MYTHIC (World Bosses) ===
            new() { Name = "World Serpent", Description = "A serpent large enough to encircle the world.",
                Strength = 380, Agility = 280, Intelligence = 200, Endurance = 400, Health = 900,
                DifficultyTier = 42, EnemyType = "Mythic", ExperienceReward = 1250, GoldReward = 700, ReputationReward = 240 },
            new() { Name = "God-Emperor's Shadow", Description = "The dark reflection of a fallen deity.",
                Strength = 400, Agility = 300, Intelligence = 350, Endurance = 380, Health = 950,
                DifficultyTier = 44, EnemyType = "Divine", ExperienceReward = 1350, GoldReward = 800, ReputationReward = 260 },
            new() { Name = "Chaos Dragon", Description = "A dragon corrupted by primordial chaos.",
                Strength = 450, Agility = 320, Intelligence = 400, Endurance = 430, Health = 1000,
                DifficultyTier = 46, EnemyType = "Dragon", ExperienceReward = 1450, GoldReward = 900, ReputationReward = 280 },
            new() { Name = "The Nameless One", Description = "An entity whose true name was erased from existence.",
                Strength = 480, Agility = 350, Intelligence = 450, Endurance = 470, Health = 1100,
                DifficultyTier = 48, EnemyType = "Aberration", ExperienceReward = 1550, GoldReward = 1000, ReputationReward = 300 },
            new() { Name = "Avatar of Destruction", Description = "The physical form of entropy itself.",
                Strength = 500, Agility = 400, Intelligence = 500, Endurance = 500, Health = 1200,
                DifficultyTier = 50, EnemyType = "Divine", ExperienceReward = 2000, GoldReward = 1500, ReputationReward = 500 }
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

        // === TALES OF THE BRAVE EVENTS (15 events) ===
        var braveBook = storybooks.First(s => s.Name == "Tales of the Brave");
        storybookEvents.AddRange(new[]
        {
            // Common events (5)
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
            new RandomEvent { StorybookId = braveBook.Id, Title = "Shield Brother's Tale", Description = "The story of a soldier who never let his allies fall inspires you.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Train", BaseProbability = 0.11, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Train defensive stances", ResultDescription = "You learn to protect yourself and others.", EnduranceChange = 2, StrengthChange = 1, DisplayOrder = 1 },
                    new() { Text = "Honor his memory", ResultDescription = "His spirit guides your blade.", StrengthChange = 2, ReputationChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = braveBook.Id, Title = "War Cry Echo", Description = "A mighty war cry resonates from the pages, stirring your blood.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Battle,Explore", BaseProbability = 0.10, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Add your voice to the cry", ResultDescription = "Your shout shakes the very air!", StrengthChange = 2, CharismaChange = 1, DisplayOrder = 1 },
                    new() { Text = "Let it fuel your resolve", ResultDescription = "Silent determination fills you.", EnduranceChange = 2, StrengthChange = 1, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = braveBook.Id, Title = "Veteran's Scar", Description = "You notice a detailed illustration of battle wounds and their stories.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Neutral, TriggerActions = "Rest", BaseProbability = 0.09, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Learn from their pain", ResultDescription = "Every wound is a lesson.", EnduranceChange = 2, IntelligenceChange = 1, DisplayOrder = 1 },
                    new() { Text = "Vow to fight harder", ResultDescription = "You refuse to fall.", StrengthChange = 2, DisplayOrder = 2 }
                }},
            // Uncommon events (4)
            new RandomEvent { StorybookId = braveBook.Id, Title = "Hero's Echo", Description = "The spirit of a fallen hero speaks to you from the book.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest", BaseProbability = 0.08, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept his blessing", ResultDescription = "The hero's strength flows into you!", StrengthChange = 4, EnduranceChange = 2, ExperienceChange = 20, DisplayOrder = 1 },
                    new() { Text = "Ask for wisdom", ResultDescription = "He shares secrets of combat.", StrengthChange = 2, IntelligenceChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = braveBook.Id, Title = "Battlefield Apparition", Description = "Ghostly warriors clash around you, reenacting an ancient battle.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore,Battle", BaseProbability = 0.07, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Join the fight", ResultDescription = "You battle alongside legends!", StrengthChange = 3, AgilityChange = 2, ExperienceChange = 25, DisplayOrder = 1 },
                    new() { Text = "Observe their techniques", ResultDescription = "Ancient tactics reveal themselves.", IntelligenceChange = 3, StrengthChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = braveBook.Id, Title = "Commander's Mantle", Description = "A spectral general offers to teach you leadership.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Train", BaseProbability = 0.07, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Learn to lead", ResultDescription = "You understand the weight of command.", CharismaChange = 4, IntelligenceChange = 2, ReputationChange = 10, DisplayOrder = 1 },
                    new() { Text = "Focus on personal strength", ResultDescription = "A leader must be strong.", StrengthChange = 4, EnduranceChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = braveBook.Id, Title = "Forged in Fire", Description = "Visions of warriors tempering themselves in flame consume you.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Train", BaseProbability = 0.08, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Embrace the flames", ResultDescription = "Pain becomes power!", CheckStat = StatType.Endurance, CheckDifficulty = 35,
                        StrengthChange = 4, EnduranceChange = 3, ExperienceChange = 20, FailureDescription = "The heat is too much.", FailureEnduranceChange = 2, DisplayOrder = 1 },
                    new() { Text = "Temper your spirit slowly", ResultDescription = "Patience builds lasting strength.", EnduranceChange = 4, StrengthChange = 2, DisplayOrder = 2 }
                }},
            // Rare events (3)
            new RandomEvent { StorybookId = braveBook.Id, Title = "Legendary Weapon Vision", Description = "You dream of a legendary weapon waiting to be claimed.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Explore", BaseProbability = 0.05, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Reach for the weapon", ResultDescription = "Its power surges through you!", CheckStat = StatType.Strength, CheckDifficulty = 50,
                        StrengthChange = 6, EnduranceChange = 3, ExperienceChange = 35, FailureDescription = "The weapon fades away.", FailureStrengthChange = 1, DisplayOrder = 1 },
                    new() { Text = "Bow before it", ResultDescription = "The weapon acknowledges your respect.", StrengthChange = 3, CharismaChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = braveBook.Id, Title = "Blood of Heroes", Description = "Ancient blood magic from warrior kings pulses through the pages.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Battle,Train", BaseProbability = 0.04, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the blood oath", ResultDescription = "Their strength becomes yours!", CheckStat = StatType.Endurance, CheckDifficulty = 45,
                        StrengthChange = 5, EnduranceChange = 5, AgilityChange = 2, ExperienceChange = 40, FailureDescription = "The power overwhelms you briefly.", FailureStrengthChange = 2, FailureHealthChange = -10, DisplayOrder = 1 },
                    new() { Text = "Honor without binding", ResultDescription = "You gain power without the price.", StrengthChange = 4, EnduranceChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = braveBook.Id, Title = "Arena of Ancients", Description = "You're transported to a legendary colosseum of old.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Battle", BaseProbability = 0.05, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Fight for glory", ResultDescription = "The crowd roars your name!", CheckStat = StatType.Strength, CheckDifficulty = 55,
                        StrengthChange = 6, AgilityChange = 3, ReputationChange = 15, ExperienceChange = 35, GoldChange = 30, FailureDescription = "You fall, but with honor.", FailureStrengthChange = 2, FailureReputationChange = 5, DisplayOrder = 1 },
                    new() { Text = "Train with the champions", ResultDescription = "Legends teach you their secrets.", StrengthChange = 4, AgilityChange = 3, IntelligenceChange = 2, DisplayOrder = 2 }
                }},
            // Epic events (2)
            new RandomEvent { StorybookId = braveBook.Id, Title = "Trial of the Champion", Description = "A spectral arena materializes, challenging you to prove your worth.",
                Rarity = Rarity.Epic, OutcomeType = EventOutcome.Positive, TriggerActions = "Train,Battle", BaseProbability = 0.03, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the trial", ResultDescription = "You emerge victorious, transformed!", CheckStat = StatType.Strength, CheckDifficulty = 60,
                        StrengthChange = 10, EnduranceChange = 5, AgilityChange = 3, ExperienceChange = 50, ReputationChange = 15,
                        FailureDescription = "You fall, but learn from defeat.", FailureStrengthChange = 2, FailureEnduranceChange = 2, DisplayOrder = 1 },
                    new() { Text = "Observe and learn", ResultDescription = "Watching teaches much.", StrengthChange = 4, IntelligenceChange = 4, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = braveBook.Id, Title = "Warlord's Inheritance", Description = "The last great warlord's essence seeks a worthy heir.",
                Rarity = Rarity.Epic, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Battle", BaseProbability = 0.025, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Claim the inheritance", ResultDescription = "His power flows into you!", CheckStat = StatType.Strength, CheckDifficulty = 65,
                        StrengthChange = 10, EnduranceChange = 6, CharismaChange = 4, ExperienceChange = 60, ReputationChange = 20,
                        FailureDescription = "You're not ready... yet.", FailureStrengthChange = 3, FailureCharismaChange = 2, DisplayOrder = 1 },
                    new() { Text = "Ask for his knowledge only", ResultDescription = "Wisdom without the burden.", IntelligenceChange = 6, StrengthChange = 5, CharismaChange = 3, DisplayOrder = 2 }
                }},
            // Legendary event (1)
            new RandomEvent { StorybookId = braveBook.Id, Title = "Blessing of the War God", Description = "A divine presence notices your courage and offers a gift.",
                Rarity = Rarity.Legendary, OutcomeType = EventOutcome.Positive, TriggerActions = "Battle", BaseProbability = 0.01, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the blessing", ResultDescription = "Divine power courses through your veins!",
                        StrengthChange = 15, EnduranceChange = 10, AgilityChange = 5, ExperienceChange = 100, ReputationChange = 25, DisplayOrder = 1 },
                    new() { Text = "Request wisdom instead", ResultDescription = "The god shares ancient battle knowledge.",
                        StrengthChange = 8, IntelligenceChange = 10, CharismaChange = 5, ExperienceChange = 75, DisplayOrder = 2 }
                }}
        });

        // === WHISPERS OF THE WIND EVENTS (15 events) ===
        var windBook = storybooks.First(s => s.Name == "Whispers of the Wind");
        storybookEvents.AddRange(new[]
        {
            // Common events (5)
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
            new RandomEvent { StorybookId = windBook.Id, Title = "Feather's Fall", Description = "A magical feather drifts past, defying gravity.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Explore", BaseProbability = 0.11, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Catch the feather", ResultDescription = "Its lightness transfers to you!", AgilityChange = 2, LuckChange = 1, DisplayOrder = 1 },
                    new() { Text = "Let it guide you", ResultDescription = "It leads to a hidden path.", AgilityChange = 1, GoldChange = 15, ExperienceChange = 8, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = windBook.Id, Title = "Zephyr's Whisper", Description = "The wind speaks your name, sharing a secret.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest", BaseProbability = 0.10, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Heed the warning", ResultDescription = "You avoid future danger.", AgilityChange = 1, LuckChange = 2, DisplayOrder = 1 },
                    new() { Text = "Ask for more", ResultDescription = "The wind teaches you its ways.", AgilityChange = 2, IntelligenceChange = 1, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = windBook.Id, Title = "Leaf on the Wind", Description = "You observe a leaf's perfect dance through the air.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Train,Rest", BaseProbability = 0.09, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Mimic its movement", ResultDescription = "Your evasion improves!", AgilityChange = 2, EnduranceChange = 1, DisplayOrder = 1 },
                    new() { Text = "Meditate on its path", ResultDescription = "Inner peace brings clarity.", IntelligenceChange = 2, LuckChange = 1, DisplayOrder = 2 }
                }},
            // Uncommon events (4)
            new RandomEvent { StorybookId = windBook.Id, Title = "Wind Dancer's Gift", Description = "A spectral dancer offers to teach you their art.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Train,Rest", BaseProbability = 0.08, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Learn to dance", ResultDescription = "Your movements become fluid!", AgilityChange = 4, CharismaChange = 2, ExperienceChange = 20, DisplayOrder = 1 },
                    new() { Text = "Watch and adapt", ResultDescription = "You incorporate elements into combat.", AgilityChange = 3, StrengthChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = windBook.Id, Title = "Tailwind's Blessing", Description = "An invisible force pushes you forward, urging speed.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore,Battle", BaseProbability = 0.07, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Sprint with the wind", ResultDescription = "You move faster than ever!", AgilityChange = 4, EnduranceChange = 2, ExperienceChange = 18, DisplayOrder = 1 },
                    new() { Text = "Let it carry you far", ResultDescription = "You discover a hidden area.", AgilityChange = 2, GoldChange = 30, ExperienceChange = 15, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = windBook.Id, Title = "Echo of the Swift", Description = "The story of the fastest runner ever born plays before you.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Train", BaseProbability = 0.07, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Race their ghost", ResultDescription = "You push your limits!", CheckStat = StatType.Agility, CheckDifficulty = 35,
                        AgilityChange = 5, EnduranceChange = 2, ExperienceChange = 25, FailureDescription = "Too fast to follow, but you improve.", FailureAgilityChange = 2, DisplayOrder = 1 },
                    new() { Text = "Study their form", ResultDescription = "Perfect technique is revealed.", AgilityChange = 3, IntelligenceChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = windBook.Id, Title = "Gale Force Memory", Description = "You experience a memory of riding a great storm.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Explore", BaseProbability = 0.08, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Embrace the chaos", ResultDescription = "The storm's energy flows through you!", AgilityChange = 3, LuckChange = 3, ExperienceChange = 20, DisplayOrder = 1 },
                    new() { Text = "Find calm in the eye", ResultDescription = "Peace within the storm.", AgilityChange = 2, EnduranceChange = 2, IntelligenceChange = 2, DisplayOrder = 2 }
                }},
            // Rare events (3)
            new RandomEvent { StorybookId = windBook.Id, Title = "Storm Rider's Secret", Description = "You glimpse a legendary technique for riding the wind itself.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore,Train", BaseProbability = 0.05, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Attempt the technique", ResultDescription = "You briefly soar through the air!", CheckStat = StatType.Agility, CheckDifficulty = 50,
                        AgilityChange = 6, LuckChange = 4, ExperienceChange = 35, FailureDescription = "You stumble but learn.", FailureAgilityChange = 2, DisplayOrder = 1 },
                    new() { Text = "Memorize for later", ResultDescription = "The knowledge stays with you.", AgilityChange = 3, IntelligenceChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = windBook.Id, Title = "Shadowstep Mastery", Description = "An assassin's ghost demonstrates the art of moving unseen.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Battle,Train", BaseProbability = 0.04, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Learn the shadow step", ResultDescription = "You can slip through shadows!", CheckStat = StatType.Agility, CheckDifficulty = 50,
                        AgilityChange = 6, StrengthChange = 2, ExperienceChange = 40, FailureDescription = "The shadows reject you... for now.", FailureAgilityChange = 2, DisplayOrder = 1 },
                    new() { Text = "Focus on evasion only", ResultDescription = "Dodging becomes second nature.", AgilityChange = 5, EnduranceChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = windBook.Id, Title = "Tempest's Eye", Description = "You stand in the calm center of a magical storm.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Explore", BaseProbability = 0.05, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Absorb the storm's power", ResultDescription = "Lightning quickens your reflexes!", CheckStat = StatType.Endurance, CheckDifficulty = 45,
                        AgilityChange = 5, LuckChange = 4, EnduranceChange = 3, ExperienceChange = 35, FailureDescription = "The power overwhelms you.", FailureAgilityChange = 2, FailureHealthChange = -8, DisplayOrder = 1 },
                    new() { Text = "Meditate in the calm", ResultDescription = "Perfect stillness grants clarity.", AgilityChange = 4, IntelligenceChange = 3, LuckChange = 2, DisplayOrder = 2 }
                }},
            // Epic events (2)
            new RandomEvent { StorybookId = windBook.Id, Title = "Avatar of the Wind", Description = "The wind itself takes form and challenges you to a race.",
                Rarity = Rarity.Epic, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore,Battle", BaseProbability = 0.03, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Race the wind", ResultDescription = "You match its speed!", CheckStat = StatType.Agility, CheckDifficulty = 65,
                        AgilityChange = 10, LuckChange = 5, EnduranceChange = 3, ExperienceChange = 50, ReputationChange = 15,
                        FailureDescription = "You lose but improve.", FailureAgilityChange = 3, FailureLuckChange = 2, DisplayOrder = 1 },
                    new() { Text = "Ask for teachings", ResultDescription = "The wind shares its secrets.", AgilityChange = 5, IntelligenceChange = 4, LuckChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = windBook.Id, Title = "Windwalker's Ascension", Description = "The greatest speedster of legend offers you their power.",
                Rarity = Rarity.Epic, OutcomeType = EventOutcome.Positive, TriggerActions = "Train,Rest", BaseProbability = 0.025, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the mantle", ResultDescription = "You become the new Windwalker!", CheckStat = StatType.Agility, CheckDifficulty = 60,
                        AgilityChange = 10, EnduranceChange = 5, LuckChange = 4, ExperienceChange = 55, ReputationChange = 18,
                        FailureDescription = "Not yet worthy, but closer.", FailureAgilityChange = 4, FailureEnduranceChange = 2, DisplayOrder = 1 },
                    new() { Text = "Request partial blessing", ResultDescription = "A portion of their speed is yours.", AgilityChange = 6, LuckChange = 4, EnduranceChange = 3, DisplayOrder = 2 }
                }},
            // Legendary event (1)
            new RandomEvent { StorybookId = windBook.Id, Title = "Blessing of the Sky Spirit", Description = "An ancient sky deity recognizes your potential.",
                Rarity = Rarity.Legendary, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore", BaseProbability = 0.01, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the gift of flight", ResultDescription = "You become one with the wind!",
                        AgilityChange = 15, LuckChange = 10, EnduranceChange = 5, ExperienceChange = 100, ReputationChange = 25, DisplayOrder = 1 },
                    new() { Text = "Request foresight", ResultDescription = "You can sense what the wind knows.",
                        AgilityChange = 8, IntelligenceChange = 8, LuckChange = 8, ExperienceChange = 75, DisplayOrder = 2 }
                }}
        });

        // === GRIMOIRE OF SHADOWS EVENTS (15 events) ===
        var shadowBook = storybooks.First(s => s.Name == "Grimoire of Shadows");
        storybookEvents.AddRange(new[]
        {
            // Common events (5)
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
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Candlelight Revelation", Description = "By flickering candlelight, hidden text becomes visible.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest", BaseProbability = 0.11, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Read the hidden text", ResultDescription = "A simple but useful spell is revealed.", IntelligenceChange = 2, ExperienceChange = 8, DisplayOrder = 1 },
                    new() { Text = "Copy it for later", ResultDescription = "You preserve the knowledge.", IntelligenceChange = 1, LuckChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Ink Stain Prophecy", Description = "An ink stain seems to form meaningful shapes.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Neutral, TriggerActions = "Rest,Explore", BaseProbability = 0.10, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Interpret the shapes", ResultDescription = "A glimpse of the future aids you.", LuckChange = 2, IntelligenceChange = 1, DisplayOrder = 1 },
                    new() { Text = "Dismiss it as coincidence", ResultDescription = "But the image stays with you.", IntelligenceChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Rune of Warding", Description = "A protective rune glows faintly on a page.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Train,Battle", BaseProbability = 0.09, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Memorize the rune", ResultDescription = "Protection magic becomes familiar.", IntelligenceChange = 1, EnduranceChange = 2, DisplayOrder = 1 },
                    new() { Text = "Draw it on yourself", ResultDescription = "A ward protects you.", EnduranceChange = 2, LuckChange = 1, DisplayOrder = 2 }
                }},
            // Uncommon events (4)
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Dark Knowledge", Description = "A page reveals itself, showing forbidden knowledge.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest", BaseProbability = 0.08, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Absorb the knowledge", ResultDescription = "Power floods your mind!", IntelligenceChange = 5, CharismaChange = 2, ExperienceChange = 25, DisplayOrder = 1 },
                    new() { Text = "Resist and close the book", ResultDescription = "You maintain your purity.", EnduranceChange = 3, CharismaChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Echoes of Incantations", Description = "You hear whispers of spells cast long ago.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Train,Rest", BaseProbability = 0.07, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Repeat the words", ResultDescription = "The magic responds to you!", IntelligenceChange = 4, LuckChange = 2, ExperienceChange = 20, DisplayOrder = 1 },
                    new() { Text = "Analyze the structure", ResultDescription = "You understand magical theory better.", IntelligenceChange = 4, CharismaChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Midnight Revelation", Description = "At the stroke of midnight, the grimoire opens itself.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest", BaseProbability = 0.07, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Read what it shows", ResultDescription = "Secrets meant only for you!", IntelligenceChange = 4, AgilityChange = 2, ExperienceChange = 22, DisplayOrder = 1 },
                    new() { Text = "Demand more knowledge", ResultDescription = "The book respects your boldness.", CheckStat = StatType.Charisma, CheckDifficulty = 40,
                        IntelligenceChange = 5, CharismaChange = 3, ExperienceChange = 25, FailureDescription = "The book snaps shut.", FailureIntelligenceChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Spectral Tutor", Description = "A ghostly mage offers to teach you.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Train", BaseProbability = 0.08, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the lessons", ResultDescription = "Ancient wisdom flows into you!", IntelligenceChange = 4, EnduranceChange = 2, ExperienceChange = 20, DisplayOrder = 1 },
                    new() { Text = "Ask about combat magic", ResultDescription = "Destruction spells are revealed.", IntelligenceChange = 3, StrengthChange = 3, DisplayOrder = 2 }
                }},
            // Rare events (3)
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Shadow Familiar", Description = "A creature of darkness offers to serve you.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Explore", BaseProbability = 0.05, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the familiar", ResultDescription = "It enhances your magical abilities!", CheckStat = StatType.Intelligence, CheckDifficulty = 50,
                        IntelligenceChange = 7, AgilityChange = 3, ExperienceChange = 40, FailureDescription = "It's too powerful to control.", FailureIntelligenceChange = 2, DisplayOrder = 1 },
                    new() { Text = "Bind it to a task", ResultDescription = "It brings you treasures.", IntelligenceChange = 3, GoldChange = 50, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Tome of Binding", Description = "You discover how to bind magical energies.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Train,Rest", BaseProbability = 0.04, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Bind power to yourself", ResultDescription = "Magical energy infuses your being!", CheckStat = StatType.Intelligence, CheckDifficulty = 50,
                        IntelligenceChange = 6, EnduranceChange = 4, ExperienceChange = 38, FailureDescription = "The binding slips.", FailureIntelligenceChange = 2, FailureEnergyChange = -10, DisplayOrder = 1 },
                    new() { Text = "Bind power to an object", ResultDescription = "A lucky charm is created.", IntelligenceChange = 4, LuckChange = 4, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Demon's Whisper", Description = "Something from beyond offers forbidden knowledge.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Neutral, TriggerActions = "Rest,Battle", BaseProbability = 0.05, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Listen to the demon", ResultDescription = "Dark power is yours!", CheckStat = StatType.Intelligence, CheckDifficulty = 55,
                        IntelligenceChange = 7, StrengthChange = 3, ExperienceChange = 45, FailureDescription = "The demon laughs and vanishes.", FailureIntelligenceChange = 2, FailureHealthChange = -5, DisplayOrder = 1 },
                    new() { Text = "Banish the creature", ResultDescription = "Your will proves stronger.", IntelligenceChange = 4, CharismaChange = 3, EnduranceChange = 2, DisplayOrder = 2 }
                }},
            // Epic events (2)
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Gate to the Shadow Realm", Description = "A portal opens to a dimension of pure magic.",
                Rarity = Rarity.Epic, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest", BaseProbability = 0.03, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Enter the realm", ResultDescription = "You return transformed!", CheckStat = StatType.Intelligence, CheckDifficulty = 60,
                        IntelligenceChange = 12, CharismaChange = 5, AgilityChange = 3, ExperienceChange = 60, ReputationChange = 20,
                        FailureDescription = "The realm tests you harshly.", FailureIntelligenceChange = 3, FailureHealthChange = -15, DisplayOrder = 1 },
                    new() { Text = "Draw power from afar", ResultDescription = "Safer, but still powerful.", IntelligenceChange = 6, LuckChange = 4, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = shadowBook.Id, Title = "Library of Lost Souls", Description = "The grimoire connects to a vast ethereal library.",
                Rarity = Rarity.Epic, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Train", BaseProbability = 0.025, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Browse the collection", ResultDescription = "Countless secrets become yours!", CheckStat = StatType.Intelligence, CheckDifficulty = 55,
                        IntelligenceChange = 10, CharismaChange = 4, LuckChange = 4, ExperienceChange = 55, ReputationChange = 15,
                        FailureDescription = "The library fades before you can learn much.", FailureIntelligenceChange = 4, DisplayOrder = 1 },
                    new() { Text = "Seek specific knowledge", ResultDescription = "You find exactly what you need.", IntelligenceChange = 7, StrengthChange = 3, EnduranceChange = 3, DisplayOrder = 2 }
                }},
            // Legendary event (1)
            new RandomEvent { StorybookId = shadowBook.Id, Title = "The Archmage's Legacy", Description = "The original author's spirit awakens within the grimoire.",
                Rarity = Rarity.Legendary, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest", BaseProbability = 0.01, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the legacy", ResultDescription = "Centuries of knowledge become yours!",
                        IntelligenceChange = 18, CharismaChange = 8, LuckChange = 5, ExperienceChange = 120, ReputationChange = 30, DisplayOrder = 1 },
                    new() { Text = "Request a specific teaching", ResultDescription = "You master one school of magic.",
                        IntelligenceChange = 12, StrengthChange = 6, EnduranceChange = 6, ExperienceChange = 80, DisplayOrder = 2 }
                }}
        });

        // === CHRONICLE OF FORTUNE EVENTS (15 events) ===
        var fortuneBook = storybooks.First(s => s.Name == "Chronicle of Fortune");
        storybookEvents.AddRange(new[]
        {
            // Common events (5)
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
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Four-Leaf Clover", Description = "A rare clover appears at your feet.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore,Rest", BaseProbability = 0.11, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Keep it as a charm", ResultDescription = "Luck favors the prepared!", LuckChange = 2, EnduranceChange = 1, DisplayOrder = 1 },
                    new() { Text = "Make a wish", ResultDescription = "Your wish echoes in fate.", LuckChange = 2, ExperienceChange = 10, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Shooting Star", Description = "A star streaks across the sky just for you.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Explore", BaseProbability = 0.10, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Wish upon it", ResultDescription = "The universe listens!", LuckChange = 2, CharismaChange = 1, DisplayOrder = 1 },
                    new() { Text = "Follow its path", ResultDescription = "It leads to a small treasure.", LuckChange = 1, GoldChange = 20, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Coin Toss", Description = "A mysterious coin lands at your feet, always on edge.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Neutral, TriggerActions = "Rest,Battle", BaseProbability = 0.09, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Call heads", ResultDescription = "You called it!", LuckChange = 2, GoldChange = 10, DisplayOrder = 1 },
                    new() { Text = "Call tails", ResultDescription = "Fortune smiles!", LuckChange = 2, ExperienceChange = 8, DisplayOrder = 2 }
                }},
            // Uncommon events (4)
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Gambler's Intuition", Description = "You feel incredibly lucky.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Explore", BaseProbability = 0.09, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Test your luck", ResultDescription = "Fortune favors you!", CheckStat = StatType.Luck, CheckDifficulty = 40,
                        LuckChange = 5, GoldChange = 40, ExperienceChange = 20, FailureDescription = "Not this time.", FailureLuckChange = 1, DisplayOrder = 1 },
                    new() { Text = "Save it for later", ResultDescription = "The feeling lingers.", LuckChange = 3, EnduranceChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Golden Opportunity", Description = "A once-in-a-lifetime chance presents itself.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore,Battle", BaseProbability = 0.08, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Take the risk", ResultDescription = "Fortune favors the bold!", CheckStat = StatType.Luck, CheckDifficulty = 35,
                        LuckChange = 4, GoldChange = 35, ExperienceChange = 22, FailureDescription = "The opportunity passes.", FailureLuckChange = 2, DisplayOrder = 1 },
                    new() { Text = "Play it safe", ResultDescription = "Cautious gains are still gains.", LuckChange = 3, GoldChange = 20, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Lucky Streak", Description = "Everything seems to go your way.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Train,Battle", BaseProbability = 0.07, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Push your luck further", ResultDescription = "The streak continues!", LuckChange = 4, StrengthChange = 2, ExperienceChange = 20, DisplayOrder = 1 },
                    new() { Text = "Quit while ahead", ResultDescription = "Wisdom preserves fortune.", LuckChange = 3, IntelligenceChange = 2, CharismaChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Fortune Teller's Gift", Description = "A mystic sees greatness in your future.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Explore", BaseProbability = 0.08, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the prophecy", ResultDescription = "Fate aligns with the prediction!", LuckChange = 4, CharismaChange = 2, ExperienceChange = 18, DisplayOrder = 1 },
                    new() { Text = "Make your own future", ResultDescription = "Your defiance impresses fate.", LuckChange = 3, StrengthChange = 2, EnduranceChange = 2, DisplayOrder = 2 }
                }},
            // Rare events (3)
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Twist of Fate", Description = "Destiny itself seems to bend around you.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Battle,Explore", BaseProbability = 0.06, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Embrace the twist", ResultDescription = "Reality shifts in your favor!", LuckChange = 8, CharismaChange = 4, ExperienceChange = 40, DisplayOrder = 1 },
                    new() { Text = "Redirect it to others", ResultDescription = "Your generosity impresses fate.", LuckChange = 4, ReputationChange = 15, CharismaChange = 4, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Jackpot!", Description = "Against all odds, you hit the jackpot.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore", BaseProbability = 0.05, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Celebrate your fortune", ResultDescription = "Incredible luck!", LuckChange = 6, GoldChange = 75, ExperienceChange = 35, DisplayOrder = 1 },
                    new() { Text = "Share the wealth", ResultDescription = "Generosity breeds more luck.", LuckChange = 5, CharismaChange = 4, ReputationChange = 12, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Fated Meeting", Description = "You meet someone who was destined to help you.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Explore", BaseProbability = 0.05, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept their aid", ResultDescription = "Fate brought you together!", LuckChange = 5, CharismaChange = 4, IntelligenceChange = 3, ExperienceChange = 35, DisplayOrder = 1 },
                    new() { Text = "Travel together briefly", ResultDescription = "A fortunate companion.", LuckChange = 4, EnduranceChange = 3, StrengthChange = 3, DisplayOrder = 2 }
                }},
            // Epic events (2)
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Fortune's Wheel", Description = "A golden wheel of fate appears before you.",
                Rarity = Rarity.Epic, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Explore", BaseProbability = 0.04, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Spin the wheel", ResultDescription = "The wheel grants incredible fortune!", CheckStat = StatType.Luck, CheckDifficulty = 55,
                        LuckChange = 12, GoldChange = 100, ExperienceChange = 60, ReputationChange = 20,
                        FailureDescription = "The wheel grants a smaller gift.", FailureLuckChange = 4, FailureGoldChange = 30, DisplayOrder = 1 },
                    new() { Text = "Ask for steady fortune", ResultDescription = "Consistent luck flows to you.", LuckChange = 7, CharismaChange = 5, EnduranceChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Destiny Unbound", Description = "The chains of fate loosen around you.",
                Rarity = Rarity.Epic, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Battle", BaseProbability = 0.03, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Seize control of fate", ResultDescription = "You become master of your destiny!", CheckStat = StatType.Luck, CheckDifficulty = 60,
                        LuckChange = 10, StrengthChange = 4, AgilityChange = 4, ExperienceChange = 55, ReputationChange = 18,
                        FailureDescription = "Fate resists, but you gain insight.", FailureLuckChange = 4, FailureIntelligenceChange = 3, DisplayOrder = 1 },
                    new() { Text = "Flow with destiny", ResultDescription = "Harmony with fate brings peace.", LuckChange = 7, EnduranceChange = 4, CharismaChange = 4, DisplayOrder = 2 }
                }},
            // Legendary event (1)
            new RandomEvent { StorybookId = fortuneBook.Id, Title = "Avatar of Fortune", Description = "Lady Luck herself manifests before you.",
                Rarity = Rarity.Legendary, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest", BaseProbability = 0.01, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Request her blessing", ResultDescription = "Fortune will forever favor you!",
                        LuckChange = 20, CharismaChange = 10, GoldChange = 200, ExperienceChange = 150, ReputationChange = 30, DisplayOrder = 1 },
                    new() { Text = "Ask for others' fortune", ResultDescription = "Your selflessness moves her deeply.",
                        LuckChange = 12, CharismaChange = 15, ReputationChange = 50, ExperienceChange = 100, DisplayOrder = 2 }
                }}
        });

        // === EPIC OF THE DRAGON SLAYER EVENTS (15 events) ===
        var dragonBook = storybooks.First(s => s.Name == "Epic of the Dragon Slayer");
        storybookEvents.AddRange(new[]
        {
            // Common events (5)
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
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Fire Breath Vision", Description = "You experience the sensation of breathing fire.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Train", BaseProbability = 0.11, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Embrace the flames", ResultDescription = "Inner fire awakens!", StrengthChange = 2, IntelligenceChange = 1, DisplayOrder = 1 },
                    new() { Text = "Control the burn", ResultDescription = "Discipline over power.", EnduranceChange = 2, StrengthChange = 1, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Slayer's Training", Description = "Visions of ancient training methods fill your mind.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Positive, TriggerActions = "Train", BaseProbability = 0.10, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Mimic the exercises", ResultDescription = "Old techniques prove effective!", StrengthChange = 2, AgilityChange = 1, DisplayOrder = 1 },
                    new() { Text = "Study the form", ResultDescription = "Understanding improves execution.", IntelligenceChange = 2, StrengthChange = 1, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Dragon Tooth", Description = "A massive dragon tooth appears from the book's pages.",
                Rarity = Rarity.Common, OutcomeType = EventOutcome.Neutral, TriggerActions = "Explore,Rest", BaseProbability = 0.09, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Fashion a weapon", ResultDescription = "A dragon-slaying dagger!", StrengthChange = 2, EnduranceChange = 1, DisplayOrder = 1 },
                    new() { Text = "Sell to a collector", ResultDescription = "Rare artifacts fetch good prices.", GoldChange = 40, CharismaChange = 1, DisplayOrder = 2 }
                }},
            // Uncommon events (4)
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Slayer's Memory", Description = "You experience a dragon slayer's final battle.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Train", BaseProbability = 0.08, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Fight as they did", ResultDescription = "Their techniques become yours!", StrengthChange = 4, AgilityChange = 3, ExperienceChange = 25, DisplayOrder = 1 },
                    new() { Text = "Learn their strategy", ResultDescription = "You understand how to fight giants.", IntelligenceChange = 4, StrengthChange = 2, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Dragon's Hoard", Description = "You glimpse the location of a dragon's treasure.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Explore", BaseProbability = 0.07, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Follow the map", ResultDescription = "A small portion of the hoard!", GoldChange = 50, LuckChange = 2, ExperienceChange = 20, DisplayOrder = 1 },
                    new() { Text = "Study the location", ResultDescription = "Knowledge for later.", IntelligenceChange = 3, LuckChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Wings of Legend", Description = "You feel phantom wings upon your back.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Explore", BaseProbability = 0.07, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Spread your wings", ResultDescription = "Dragon's grace flows through you!", AgilityChange = 4, StrengthChange = 2, ExperienceChange = 22, DisplayOrder = 1 },
                    new() { Text = "Channel into strength", ResultDescription = "Power instead of flight.", StrengthChange = 4, EnduranceChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Dragonfire Forge", Description = "You witness weapons being forged in dragon flame.",
                Rarity = Rarity.Uncommon, OutcomeType = EventOutcome.Positive, TriggerActions = "Train,Battle", BaseProbability = 0.08, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Learn the technique", ResultDescription = "Forging knowledge is power!", StrengthChange = 3, IntelligenceChange = 3, ExperienceChange = 20, DisplayOrder = 1 },
                    new() { Text = "Claim a weapon", ResultDescription = "A blade of dragonfire steel!", StrengthChange = 4, AgilityChange = 2, DisplayOrder = 2 }
                }},
            // Rare events (3)
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Dragon Blood Blessing", Description = "The book pulses with ancient dragon blood magic.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Battle", BaseProbability = 0.05, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the blood magic", ResultDescription = "Dragon fire burns in your veins!", CheckStat = StatType.Endurance, CheckDifficulty = 50,
                        StrengthChange = 6, EnduranceChange = 5, AgilityChange = 3, ExperienceChange = 45,
                        FailureDescription = "The power overwhelms briefly.", FailureStrengthChange = 2, FailureHealthChange = -10, DisplayOrder = 1 },
                    new() { Text = "Request protection only", ResultDescription = "Dragon scales form on your skin.", EnduranceChange = 7, StrengthChange = 3, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Wyrm's Wisdom", Description = "An ancient wyrm shares knowledge across time.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Train", BaseProbability = 0.04, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the knowledge", ResultDescription = "Centuries of wisdom flow into you!", CheckStat = StatType.Intelligence, CheckDifficulty = 45,
                        IntelligenceChange = 6, StrengthChange = 3, CharismaChange = 3, ExperienceChange = 40, FailureDescription = "Too much to comprehend.", FailureIntelligenceChange = 3, DisplayOrder = 1 },
                    new() { Text = "Ask about combat", ResultDescription = "Dragon-fighting techniques revealed.", StrengthChange = 5, AgilityChange = 4, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Heart of the Dragon", Description = "You feel a dragon's heart beat within your chest.",
                Rarity = Rarity.Rare, OutcomeType = EventOutcome.Positive, TriggerActions = "Battle,Train", BaseProbability = 0.05, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Accept the power", ResultDescription = "Dragon's might courses through you!", CheckStat = StatType.Endurance, CheckDifficulty = 50,
                        StrengthChange = 5, EnduranceChange = 5, HealthChange = 20, ExperienceChange = 40, FailureDescription = "The power recedes.", FailureEnduranceChange = 3, DisplayOrder = 1 },
                    new() { Text = "Temper it with wisdom", ResultDescription = "Balanced power is lasting power.", StrengthChange = 4, EnduranceChange = 3, IntelligenceChange = 3, DisplayOrder = 2 }
                }},
            // Epic events (2)
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Spirit of the Last Dragon", Description = "An ancient dragon's spirit emerges from the book.",
                Rarity = Rarity.Epic, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest", BaseProbability = 0.03, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Challenge the spirit", ResultDescription = "You prove your worth as a true slayer!", CheckStat = StatType.Strength, CheckDifficulty = 65,
                        StrengthChange = 12, EnduranceChange = 8, AgilityChange = 5, ExperienceChange = 70, ReputationChange = 25,
                        FailureDescription = "The dragon teaches through defeat.", FailureStrengthChange = 4, FailureEnduranceChange = 4, DisplayOrder = 1 },
                    new() { Text = "Seek wisdom, not battle", ResultDescription = "The dragon shares ancient knowledge.", IntelligenceChange = 8, StrengthChange = 6, CharismaChange = 4, DisplayOrder = 2 }
                }},
            new RandomEvent { StorybookId = dragonBook.Id, Title = "Dragonlord's Throne", Description = "You stand before an empty throne meant for dragon masters.",
                Rarity = Rarity.Epic, OutcomeType = EventOutcome.Positive, TriggerActions = "Rest,Battle", BaseProbability = 0.025, IsActive = true,
                Choices = new List<EventChoice> {
                    new() { Text = "Claim the throne", ResultDescription = "Dragons bow to your authority!", CheckStat = StatType.Charisma, CheckDifficulty = 60,
                        StrengthChange = 8, CharismaChange = 8, EnduranceChange = 5, ExperienceChange = 60, ReputationChange = 25,
                        FailureDescription = "You're not ready for such power.", FailureStrengthChange = 3, FailureCharismaChange = 3, DisplayOrder = 1 },
                    new() { Text = "Study its power", ResultDescription = "Understanding without claiming.", IntelligenceChange = 6, StrengthChange = 5, CharismaChange = 4, DisplayOrder = 2 }
                }},
            // Legendary event (1)
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

    private static async Task SeedSkillsAsync(ApplicationDbContext context)
    {
        var skills = new List<Skill>
        {
            // === PASSIVE SKILLS ===
            new() { Name = "Quick Reflexes", Description = "Your heightened reflexes give you a chance to dodge incoming attacks.",
                SkillType = SkillType.Passive, Rarity = Rarity.Common,
                PassiveEffect = PassiveEffect.Evasion, PassiveValue = 8.0, IsActive = true },

            new() { Name = "Eagle Eye", Description = "Your keen observation allows you to find weak points in enemy defenses.",
                SkillType = SkillType.Passive, Rarity = Rarity.Common,
                PassiveEffect = PassiveEffect.CriticalChance, PassiveValue = 10.0, IsActive = true },

            new() { Name = "Iron Skin", Description = "Your hardened body reduces damage from all attacks.",
                SkillType = SkillType.Passive, Rarity = Rarity.Uncommon,
                PassiveEffect = PassiveEffect.DamageReduction, PassiveValue = 15.0, IsActive = true },

            new() { Name = "Vampiric Touch", Description = "Your attacks drain life force from enemies, healing you slightly.",
                SkillType = SkillType.Passive, Rarity = Rarity.Rare,
                PassiveEffect = PassiveEffect.LifeSteal, PassiveValue = 10.0, IsActive = true },

            new() { Name = "Counter Strike", Description = "When hit, you have a chance to automatically counter-attack.",
                SkillType = SkillType.Passive, Rarity = Rarity.Uncommon,
                PassiveEffect = PassiveEffect.CounterAttack, PassiveValue = 15.0, IsActive = true },

            new() { Name = "Thorned Armor", Description = "Enemies that strike you take damage from your magical thorns.",
                SkillType = SkillType.Passive, Rarity = Rarity.Rare,
                PassiveEffect = PassiveEffect.Thorns, PassiveValue = 20.0, IsActive = true },

            new() { Name = "Shadow Step", Description = "Your mastery of shadows grants exceptional evasion.",
                SkillType = SkillType.Passive, Rarity = Rarity.Epic,
                PassiveEffect = PassiveEffect.Evasion, PassiveValue = 18.0, IsActive = true },

            new() { Name = "Assassin's Mark", Description = "Your strikes find vital points with deadly precision.",
                SkillType = SkillType.Passive, Rarity = Rarity.Epic,
                PassiveEffect = PassiveEffect.CriticalChance, PassiveValue = 20.0, IsActive = true },

            // === ACTIVE SKILLS ===
            new() { Name = "Power Strike", Description = "A devastating blow that deals extra damage based on your strength.",
                SkillType = SkillType.Active, Rarity = Rarity.Common,
                TriggerChance = 0.20, BaseDamage = 15, ScalingStat = StatType.Strength, ScalingMultiplier = 0.3,
                ActiveNarrative = "You unleash a powerful strike!", IsActive = true },

            new() { Name = "Swift Slash", Description = "A quick attack that strikes before the enemy can react.",
                SkillType = SkillType.Active, Rarity = Rarity.Common,
                TriggerChance = 0.25, BaseDamage = 10, ScalingStat = StatType.Agility, ScalingMultiplier = 0.35,
                ActiveNarrative = "You strike with lightning speed!", IsActive = true },

            new() { Name = "Arcane Bolt", Description = "Channel magical energy into a damaging projectile.",
                SkillType = SkillType.Active, Rarity = Rarity.Common,
                TriggerChance = 0.20, BaseDamage = 12, ScalingStat = StatType.Intelligence, ScalingMultiplier = 0.4,
                ActiveNarrative = "You hurl a bolt of arcane energy!", IsActive = true },

            new() { Name = "Crushing Blow", Description = "A bone-shattering attack that ignores some enemy defense.",
                SkillType = SkillType.Active, Rarity = Rarity.Uncommon,
                TriggerChance = 0.15, BaseDamage = 25, ScalingStat = StatType.Strength, ScalingMultiplier = 0.4,
                ActiveNarrative = "You deliver a crushing blow!", IsActive = true },

            new() { Name = "Shadow Strike", Description = "Attack from the shadows with deadly precision.",
                SkillType = SkillType.Active, Rarity = Rarity.Rare,
                TriggerChance = 0.18, BaseDamage = 30, ScalingStat = StatType.Agility, ScalingMultiplier = 0.5,
                ActiveNarrative = "You strike from the shadows!", IsActive = true },

            new() { Name = "Inferno Blast", Description = "Unleash a devastating wave of magical fire.",
                SkillType = SkillType.Active, Rarity = Rarity.Rare,
                TriggerChance = 0.15, BaseDamage = 35, ScalingStat = StatType.Intelligence, ScalingMultiplier = 0.55,
                ActiveNarrative = "You unleash a blazing inferno!", IsActive = true },

            new() { Name = "Divine Smite", Description = "Call upon divine power to smite your foes.",
                SkillType = SkillType.Active, Rarity = Rarity.Epic,
                TriggerChance = 0.12, BaseDamage = 50, ScalingStat = StatType.Intelligence, ScalingMultiplier = 0.6,
                ActiveNarrative = "Divine light strikes down your enemy!", IsActive = true },

            new() { Name = "Berserker Rage", Description = "Enter a battle frenzy, dealing massive damage.",
                SkillType = SkillType.Active, Rarity = Rarity.Epic,
                TriggerChance = 0.10, BaseDamage = 60, ScalingStat = StatType.Strength, ScalingMultiplier = 0.7,
                ActiveNarrative = "You enter a berserker rage!", IsActive = true },

            // === BONUS SKILLS ===
            new() { Name = "Fortune's Favor", Description = "Lady Luck smiles upon you, increasing gold found.",
                SkillType = SkillType.Bonus, Rarity = Rarity.Common,
                BonusEffect = BonusEffect.GoldGain, BonusPercentage = 15.0, BonusFlatValue = 5, IsActive = true },

            new() { Name = "Wisdom Seeker", Description = "Your thirst for knowledge grants bonus experience.",
                SkillType = SkillType.Bonus, Rarity = Rarity.Common,
                BonusEffect = BonusEffect.ExperienceGain, BonusPercentage = 10.0, BonusFlatValue = 3, IsActive = true },

            new() { Name = "Boundless Energy", Description = "Your vitality knows no bounds, recovering more energy when resting.",
                SkillType = SkillType.Bonus, Rarity = Rarity.Uncommon,
                BonusEffect = BonusEffect.EnergyGain, BonusPercentage = 20.0, BonusFlatValue = 10, IsActive = true },

            new() { Name = "Regeneration", Description = "Your body heals naturally over time.",
                SkillType = SkillType.Bonus, Rarity = Rarity.Uncommon,
                BonusEffect = BonusEffect.HealthRegen, BonusPercentage = 0, BonusFlatValue = 5, IsActive = true },

            new() { Name = "Lucky Star", Description = "Fortune favors you in all endeavors.",
                SkillType = SkillType.Bonus, Rarity = Rarity.Rare,
                BonusEffect = BonusEffect.LuckBoost, BonusPercentage = 15.0, BonusFlatValue = 0, IsActive = true },

            new() { Name = "Master's Guidance", Description = "An invisible mentor guides your training.",
                SkillType = SkillType.Bonus, Rarity = Rarity.Rare,
                BonusEffect = BonusEffect.TrainingBoost, BonusPercentage = 20.0, BonusFlatValue = 1, IsActive = true },

            new() { Name = "Midas Touch", Description = "Everything you touch turns to gold... figuratively.",
                SkillType = SkillType.Bonus, Rarity = Rarity.Epic,
                BonusEffect = BonusEffect.GoldGain, BonusPercentage = 30.0, BonusFlatValue = 15, IsActive = true },

            new() { Name = "Sage's Blessing", Description = "Ancient wisdom accelerates your learning.",
                SkillType = SkillType.Bonus, Rarity = Rarity.Epic,
                BonusEffect = BonusEffect.ExperienceGain, BonusPercentage = 25.0, BonusFlatValue = 10, IsActive = true },

            new() { Name = "Eternal Vitality", Description = "Your life force is boundless, recovering health each turn.",
                SkillType = SkillType.Bonus, Rarity = Rarity.Legendary,
                BonusEffect = BonusEffect.HealthRegen, BonusPercentage = 0, BonusFlatValue = 15, IsActive = true },

            new() { Name = "Destiny's Champion", Description = "Fate itself guides your every action.",
                SkillType = SkillType.Bonus, Rarity = Rarity.Legendary,
                BonusEffect = BonusEffect.LuckBoost, BonusPercentage = 30.0, BonusFlatValue = 10, IsActive = true }
        };

        await context.Skills.AddRangeAsync(skills);
    }
}
