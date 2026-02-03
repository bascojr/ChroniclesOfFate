using ChroniclesOfFate.Core.DTOs;
using ChroniclesOfFate.Core.Entities;
using ChroniclesOfFate.Core.Enums;
using ChroniclesOfFate.Core.Interfaces;

namespace ChroniclesOfFate.Core.Services;

/// <summary>
/// Handles auto-battle simulation with stat-based combat
/// </summary>
public class BattleService : IBattleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRandomService _random;
    private readonly IProgressionService _progression;

    public BattleService(IUnitOfWork unitOfWork, IRandomService random, IProgressionService progression)
    {
        _unitOfWork = unitOfWork;
        _random = random;
        _progression = progression;
    }

    public async Task<BattleResultDto> SimulateBattleAsync(int characterId, int enemyId)
    {
        var character = await _unitOfWork.Characters.GetWithFullDetailsAsync(characterId)
            ?? throw new InvalidOperationException("Character not found");

        var enemy = await _unitOfWork.Enemies.GetByIdAsync(enemyId)
            ?? throw new InvalidOperationException("Enemy not found");

        // Energy check
        const int battleEnergyCost = 15;
        if (character.CurrentEnergy < battleEnergyCost)
        {
            return new BattleResultDto(
                BattleResult.Fled,
                "You don't have enough energy to fight!",
                new List<BattleRoundDto>(),
                0, 0, 0, 0, 0,
                MapToEnemyDto(enemy),
                null,
                0,
                0,
                new List<SkillProcDto>()
            );
        }

        // Initialize battle state
        int playerHealth = character.CurrentHealth;
        int enemyHealth = enemy.Health;
        var rounds = new List<BattleRoundDto>();

        // Calculate combat stats
        int playerPower = CalculateCombatPower(character);
        int enemyPower = CalculateEnemyPower(enemy);

        // Get character's skills for combat calculations
        var skills = character.Skills?.Where(cs => cs.Skill != null).Select(cs => cs.Skill!).ToList() ?? new List<Skill>();

        // Track which active skills have already been used (they only proc once per battle)
        var usedActiveSkills = new HashSet<int>();
        var skillsUsed = new List<SkillProcDto>();

        // Calculate attack intervals based on agility
        int playerInterval = CalculateAttackInterval(character.Agility);
        int enemyInterval = CalculateAttackInterval(enemy.Agility);

        // Initialize timers - first attacks happen after their respective intervals
        int playerNextAttack = playerInterval;
        int enemyNextAttack = enemyInterval;
        int currentTime = 0;
        const int maxBattleDurationMs = 60000;

        // Time-based battle loop
        while (playerHealth > 0 && enemyHealth > 0 && currentTime < maxBattleDurationMs)
        {
            if (playerNextAttack <= enemyNextAttack)
            {
                // Player attacks
                currentTime = playerNextAttack;

                var (damage, action, skillNarrative) = CalculatePlayerAttackWithSkills(character, enemy, skills, usedActiveSkills, skillsUsed, currentTime);
                enemyHealth -= damage;

                // Apply life steal
                int healingDone = ApplyLifeSteal(skills, damage, ref playerHealth, character.MaxHealth);

                var narrativeParts = new List<string>();
                narrativeParts.Add($"{character.Name} {action} for {damage} damage!");
                if (!string.IsNullOrEmpty(skillNarrative)) narrativeParts.Add(skillNarrative);
                if (healingDone > 0) narrativeParts.Add($"Life stolen: +{healingDone} HP!");

                rounds.Add(new BattleRoundDto(
                    currentTime,
                    "Player",
                    action,
                    damage,
                    playerHealth,
                    Math.Max(enemyHealth, 0),
                    string.Join(" ", narrativeParts)
                ));

                playerNextAttack += playerInterval;
            }
            else
            {
                // Enemy attacks
                currentTime = enemyNextAttack;

                var (damage, action) = CalculateEnemyAttackWithSkills(enemy, character, skills, ref enemyHealth);

                var narrativeParts = new List<string>();

                // Check player evasion
                if (CheckEvasion(skills, character.Agility))
                {
                    narrativeParts.Add($"{character.Name} dodges the attack!");

                    // Check for counter attack on dodge
                    if (CheckCounterAttack(skills))
                    {
                        int counterDamage = (int)(character.Strength * 0.2) + _random.Next(5, 10);
                        enemyHealth -= counterDamage;
                        narrativeParts.Add($"{character.Name} counter-attacks for {counterDamage} damage!");
                    }

                    rounds.Add(new BattleRoundDto(
                        currentTime,
                        "Enemy",
                        action + " (dodged)",
                        0,
                        playerHealth,
                        Math.Max(enemyHealth, 0),
                        string.Join(" ", narrativeParts)
                    ));
                }
                else
                {
                    playerHealth -= damage;
                    narrativeParts.Add($"{enemy.Name} {action} for {damage} damage!");

                    rounds.Add(new BattleRoundDto(
                        currentTime,
                        "Enemy",
                        action,
                        damage,
                        Math.Max(playerHealth, 0),
                        enemyHealth,
                        string.Join(" ", narrativeParts)
                    ));
                }

                enemyNextAttack += enemyInterval;
            }
        }

        // Determine result
        BattleResult result;
        string narrative;
        int expGained = 0, goldGained = 0, repGained = 0;

        if (enemyHealth <= 0)
        {
            result = BattleResult.Victory;
            double battleDurationSec = currentTime / 1000.0;
            narrative = GenerateVictoryNarrative(character.Name, enemy.Name, battleDurationSec);
            expGained = CalculateExperienceReward(enemy, character.Level);
            goldGained = enemy.GoldReward + _random.Next(0, enemy.GoldReward / 2);
            repGained = enemy.ReputationReward;
        }
        else if (playerHealth <= 0)
        {
            result = BattleResult.Defeat;
            narrative = GenerateDefeatNarrative(character.Name, enemy.Name);
        }
        else
        {
            result = BattleResult.Draw;
            narrative = $"After 60 seconds of intense combat, neither {character.Name} nor {enemy.Name} could claim victory. Both fighters retreat to recover.";
        }

        // Calculate health lost
        int healthLost = character.CurrentHealth - Math.Max(playerHealth, 0);

        // Update character state
        character.CurrentEnergy -= battleEnergyCost;
        character.CurrentHealth = Math.Max(playerHealth, 1); // Don't let health drop to 0
        character.Experience += expGained;
        character.Gold += goldGained;
        character.Reputation += repGained;
        character.UpdatedAt = DateTime.UtcNow;

        // Log the battle
        var battleLog = new BattleLog
        {
            CharacterId = characterId,
            EnemyId = enemyId,
            Result = result,
            BattleNarrative = narrative,
            RoundsCount = rounds.Count,
            CharacterPowerAtBattle = playerPower,
            EnemyPowerAtBattle = enemyPower,
            ExperienceGained = expGained,
            GoldGained = goldGained,
            ReputationGained = repGained,
            HealthLost = healthLost,
            EnergySpent = battleEnergyCost,
            Year = character.CurrentYear,
            Month = character.CurrentMonth,
            Turn = character.TotalTurns
        };

        await _unitOfWork.BattleLogs.AddAsync(battleLog);
        await _unitOfWork.Characters.UpdateAsync(character);
        await _unitOfWork.SaveChangesAsync();

        // Check for level up after gaining XP (only on victory)
        LevelUpResultDto? levelUp = null;
        if (result == BattleResult.Victory)
        {
            levelUp = await _progression.CheckLevelUpAsync(character);
        }

        return new BattleResultDto(
            result,
            narrative,
            rounds,
            expGained,
            goldGained,
            repGained,
            healthLost,
            battleEnergyCost,
            MapToEnemyDto(enemy),
            levelUp,
            playerInterval,
            enemyInterval,
            skillsUsed
        );
    }

    private int CalculateAttackInterval(int agility)
    {
        const int baseIntervalMs = 5000;
        const int minIntervalMs = 500;
        const int agilityDivisor = 100;
        int interval = (int)(baseIntervalMs / (1.0 + agility / (double)agilityDivisor));
        return Math.Max(minIntervalMs, interval);
    }

    public async Task<BattleResultDto> SimulateRandomBattleAsync(int characterId)
    {
        var character = await _unitOfWork.Characters.GetByIdAsync(characterId)
            ?? throw new InvalidOperationException("Character not found");

        // Determine difficulty tier based on character level and power
        int minTier = Math.Max(1, character.Level / 2);
        int maxTier = Math.Min(10, character.Level + 2);

        var enemy = await _unitOfWork.Enemies.GetRandomEnemyAsync(minTier, maxTier);
        if (enemy == null)
        {
            // Fallback to any available enemy
            var enemies = await _unitOfWork.Enemies.GetByDifficultyTierAsync(1);
            enemy = enemies.FirstOrDefault() 
                ?? throw new InvalidOperationException("No enemies available");
        }

        return await SimulateBattleAsync(characterId, enemy.Id);
    }

    public async Task<IEnumerable<EnemyDto>> GetAvailableEnemiesAsync(int characterId)
    {
        var character = await _unitOfWork.Characters.GetByIdAsync(characterId)
            ?? throw new InvalidOperationException("Character not found");

        var enemies = await _unitOfWork.Enemies.GetEligibleEnemiesAsync(
            character.Level, 
            character.CurrentSeason);

        return enemies.Select(MapToEnemyDto);
    }

    public int CalculateCombatPower(Character character)
    {
        // Combat power formula weighted by class
        double strWeight = 1.0, agiWeight = 1.0, intWeight = 1.0, endWeight = 1.0;

        switch (character.Class)
        {
            case CharacterClass.Warrior:
                strWeight = 1.5; endWeight = 1.3;
                break;
            case CharacterClass.Mage:
                intWeight = 1.5; agiWeight = 1.2;
                break;
            case CharacterClass.Rogue:
                agiWeight = 1.5; strWeight = 1.2;
                break;
            case CharacterClass.Cleric:
                intWeight = 1.3; endWeight = 1.3;
                break;
            case CharacterClass.Ranger:
                agiWeight = 1.4; strWeight = 1.3;
                break;
        }

        return (int)(
            character.Strength * strWeight +
            character.Agility * agiWeight +
            character.Intelligence * intWeight +
            character.Endurance * endWeight +
            character.Luck * 0.5
        );
    }

    public int CalculateEnemyPower(Enemy enemy)
    {
        return enemy.Strength + enemy.Agility + enemy.Intelligence + enemy.Endurance;
    }

    private bool CheckEvasion(List<Skill> skills, int agility)
    {
        double evasionChance = agility / 1000.0; // Base evasion from agility

        foreach (var skill in skills.Where(s => s.SkillType == SkillType.Passive && s.PassiveEffect == PassiveEffect.Evasion))
        {
            evasionChance += skill.PassiveValue / 100.0;
        }

        return _random.RollChance(evasionChance);
    }

    private bool CheckCounterAttack(List<Skill> skills)
    {
        double counterChance = 0;
        foreach (var skill in skills.Where(s => s.SkillType == SkillType.Passive && s.PassiveEffect == PassiveEffect.CounterAttack))
        {
            counterChance += skill.PassiveValue / 100.0;
        }
        return _random.RollChance(counterChance);
    }

    private int ApplyLifeSteal(List<Skill> skills, int damageDealt, ref int currentHealth, int maxHealth)
    {
        double lifeStealPercent = 0;
        foreach (var skill in skills.Where(s => s.SkillType == SkillType.Passive && s.PassiveEffect == PassiveEffect.LifeSteal))
        {
            lifeStealPercent += skill.PassiveValue / 100.0;
        }

        if (lifeStealPercent <= 0) return 0;

        int healing = (int)(damageDealt * lifeStealPercent);
        int oldHealth = currentHealth;
        currentHealth = Math.Min(maxHealth, currentHealth + healing);
        return currentHealth - oldHealth;
    }

    private (int damage, string action, string? skillNarrative) CalculatePlayerAttackWithSkills(Character character, Enemy enemy, List<Skill> skills, HashSet<int> usedActiveSkills, List<SkillProcDto> skillsUsed, int currentTimeMs)
    {
        // Check for active skill trigger first (each active skill can only proc once per battle)
        var availableActiveSkills = skills.Where(s => s.SkillType == SkillType.Active && !usedActiveSkills.Contains(s.Id)).ToList();

        foreach (var skill in availableActiveSkills)
        {
            if (_random.RollChance(skill.TriggerChance))
            {
                // Mark this skill as used for the rest of the battle
                usedActiveSkills.Add(skill.Id);

                int statBonus = skill.ScalingStat.HasValue ? (int)(character.GetStat(skill.ScalingStat.Value) * skill.ScalingMultiplier) : 0;
                int skillDamage = skill.BaseDamage + statBonus;

                // Defense reduction
                int enemyDefense = enemy.Endurance / 5;
                skillDamage = Math.Max(1, skillDamage - enemyDefense);

                // Track this skill proc for the battle result
                skillsUsed.Add(new SkillProcDto(skill.Name, skillDamage, currentTimeMs));

                return (skillDamage, skill.ActiveNarrative ?? "uses a special skill", skill.Name + " activated!");
            }
        }

        // Regular attack with passive modifiers
        int baseDamage = character.Class switch
        {
            CharacterClass.Warrior => character.Strength,
            CharacterClass.Mage => character.Intelligence,
            CharacterClass.Rogue => character.Agility,
            CharacterClass.Cleric => (character.Intelligence + character.Endurance) / 2,
            CharacterClass.Ranger => (character.Agility + character.Strength) / 2,
            _ => character.Strength
        };

        int damage = (int)(baseDamage * 0.3) + _random.Next(5, 15);

        // Critical hit check (base + skill bonuses)
        double critChance = character.Luck / 500.0;
        foreach (var skill in skills.Where(s => s.SkillType == SkillType.Passive && s.PassiveEffect == PassiveEffect.CriticalChance))
        {
            critChance += skill.PassiveValue / 100.0;
        }

        bool isCritical = _random.RollChance(critChance);
        if (isCritical)
        {
            damage = (int)(damage * 1.5);
        }

        // Defense reduction
        int defense = enemy.Endurance / 5;
        damage = Math.Max(1, damage - defense);

        string action = GenerateAttackAction(character.Class, isCritical);
        return (damage, action, isCritical ? "Critical hit!" : null);
    }

    private (int damage, string action) CalculateEnemyAttackWithSkills(Enemy enemy, Character character, List<Skill> skills, ref int enemyHealth)
    {
        int baseDamage = (enemy.Strength + enemy.Intelligence) / 2;
        int damage = (int)(baseDamage * 0.25) + _random.Next(3, 12);

        // Defense from character's endurance
        int defense = character.Endurance / 5;

        // Add damage reduction from skills
        double damageReduction = 0;
        foreach (var skill in skills.Where(s => s.SkillType == SkillType.Passive && s.PassiveEffect == PassiveEffect.DamageReduction))
        {
            damageReduction += skill.PassiveValue / 100.0;
        }

        damage = (int)(damage * (1 - damageReduction));
        damage = Math.Max(1, damage - defense);

        // Apply thorns damage to enemy
        double thornsDamage = 0;
        foreach (var skill in skills.Where(s => s.SkillType == SkillType.Passive && s.PassiveEffect == PassiveEffect.Thorns))
        {
            thornsDamage += damage * (skill.PassiveValue / 100.0);
        }
        if (thornsDamage > 0)
        {
            enemyHealth -= (int)thornsDamage;
        }

        string[] actions = { "strikes", "attacks", "lunges at you", "swings wildly" };
        string action = actions[_random.Next(actions.Length)];

        return (damage, action);
    }

    private string GenerateAttackAction(CharacterClass characterClass, bool isCritical)
    {
        var actions = characterClass switch
        {
            CharacterClass.Warrior => isCritical 
                ? new[] { "delivers a devastating blow", "executes a powerful cleave", "unleashes a mighty strike" }
                : new[] { "swings their sword", "strikes with their weapon", "attacks fiercely" },
            CharacterClass.Mage => isCritical
                ? new[] { "casts a powerful spell", "unleashes arcane fury", "channels devastating magic" }
                : new[] { "casts a spell", "hurls a magic bolt", "channels arcane energy" },
            CharacterClass.Rogue => isCritical
                ? new[] { "finds a vital spot", "strikes from the shadows", "delivers a precise critical blow" }
                : new[] { "strikes swiftly", "attacks with precision", "slashes quickly" },
            CharacterClass.Cleric => isCritical
                ? new[] { "calls down divine judgment", "smites with holy power", "channels radiant energy" }
                : new[] { "invokes divine power", "strikes with blessed weapon", "channels holy energy" },
            CharacterClass.Ranger => isCritical
                ? new[] { "finds the perfect shot", "strikes a vital point", "unleashes a deadly volley" }
                : new[] { "fires an arrow", "strikes with precision", "attacks from range" },
            _ => new[] { "attacks" }
        };

        return actions[_random.Next(actions.Length)];
    }

    private string GenerateVictoryNarrative(string characterName, string enemyName, double durationSeconds)
    {
        string durationText = durationSeconds < 10 ? $"{durationSeconds:F1} seconds" : $"{durationSeconds:F0} seconds";
        string[] templates =
        {
            $"After {durationText} of intense combat, {characterName} emerges victorious over {enemyName}!",
            $"{characterName} defeats {enemyName} in a fierce battle lasting {durationText}!",
            $"With determination and skill, {characterName} overcomes {enemyName} after {durationText} of combat!",
            $"The battle is won! {characterName} stands triumphant over the fallen {enemyName}!"
        };
        return templates[_random.Next(templates.Length)];
    }

    private string GenerateDefeatNarrative(string characterName, string enemyName)
    {
        string[] templates =
        {
            $"{characterName} falls in battle against {enemyName}. A tactical retreat is in order...",
            $"{enemyName} proves too powerful. {characterName} must recover and grow stronger.",
            $"Defeated by {enemyName}, {characterName} retreats to fight another day.",
            $"The battle is lost, but {characterName}'s journey continues. Learn from this defeat."
        };
        return templates[_random.Next(templates.Length)];
    }

    private int CalculateExperienceReward(Enemy enemy, int characterLevel)
    {
        int baseExp = enemy.ExperienceReward;
        
        // Bonus for fighting higher tier enemies
        int tierDiff = enemy.DifficultyTier - characterLevel;
        if (tierDiff > 0)
        {
            baseExp = (int)(baseExp * (1 + tierDiff * 0.1));
        }
        else if (tierDiff < -2)
        {
            // Reduced exp for much weaker enemies
            baseExp = (int)(baseExp * 0.5);
        }

        return baseExp;
    }

    private static EnemyDto MapToEnemyDto(Enemy enemy)
    {
        return new EnemyDto(
            enemy.Id,
            enemy.Name,
            enemy.Description,
            enemy.ImageUrl,
            enemy.Strength,
            enemy.Agility,
            enemy.Intelligence,
            enemy.Endurance,
            enemy.Health,
            enemy.DifficultyTier,
            enemy.EnemyType
        );
    }
}
