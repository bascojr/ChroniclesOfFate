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

    public BattleService(IUnitOfWork unitOfWork, IRandomService random)
    {
        _unitOfWork = unitOfWork;
        _random = random;
    }

    public async Task<BattleResultDto> SimulateBattleAsync(int characterId, int enemyId)
    {
        var character = await _unitOfWork.Characters.GetWithFullDetailsAsync(characterId)
            ?? throw new InvalidOperationException("Character not found");
        
        var enemy = await _unitOfWork.Enemies.GetByIdAsync(enemyId)
            ?? throw new InvalidOperationException("Enemy not found");

        // Energy check
        const int battleEnergyCost = 30;
        if (character.CurrentEnergy < battleEnergyCost)
        {
            return new BattleResultDto(
                BattleResult.Fled,
                "You don't have enough energy to fight!",
                new List<BattleRoundDto>(),
                0, 0, 0, 0, 0,
                MapToEnemyDto(enemy)
            );
        }

        // Initialize battle state
        int playerHealth = character.CurrentHealth;
        int enemyHealth = enemy.Health;
        var rounds = new List<BattleRoundDto>();
        int roundNumber = 0;
        const int maxRounds = 20;

        // Calculate combat stats
        int playerPower = CalculateCombatPower(character);
        int enemyPower = CalculateEnemyPower(enemy);

        // Battle loop
        while (playerHealth > 0 && enemyHealth > 0 && roundNumber < maxRounds)
        {
            roundNumber++;
            var round = SimulateBattleRound(character, enemy, ref playerHealth, ref enemyHealth, roundNumber);
            rounds.Add(round);
        }

        // Determine result
        BattleResult result;
        string narrative;
        int expGained = 0, goldGained = 0, repGained = 0;

        if (enemyHealth <= 0)
        {
            result = BattleResult.Victory;
            narrative = GenerateVictoryNarrative(character.Name, enemy.Name, rounds.Count);
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
            narrative = $"After {maxRounds} rounds of intense combat, neither {character.Name} nor {enemy.Name} could claim victory. Both fighters retreat to recover.";
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

        return new BattleResultDto(
            result,
            narrative,
            rounds,
            expGained,
            goldGained,
            repGained,
            healthLost,
            battleEnergyCost,
            MapToEnemyDto(enemy)
        );
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

    private BattleRoundDto SimulateBattleRound(
        Character character, 
        Enemy enemy, 
        ref int playerHealth, 
        ref int enemyHealth,
        int roundNumber)
    {
        // Determine turn order based on agility
        bool playerFirst = character.Agility + _random.Next(20) >= enemy.Agility + _random.Next(20);

        int playerDamage = 0, enemyDamage = 0;
        string playerAction = "", enemyAction = "";
        var narrativeParts = new List<string>();

        if (playerFirst)
        {
            // Player attacks first
            (playerDamage, playerAction) = CalculatePlayerAttack(character, enemy);
            enemyHealth -= playerDamage;
            narrativeParts.Add($"{character.Name} {playerAction} for {playerDamage} damage!");

            if (enemyHealth > 0)
            {
                // Enemy counterattacks
                (enemyDamage, enemyAction) = CalculateEnemyAttack(enemy, character);
                playerHealth -= enemyDamage;
                narrativeParts.Add($"{enemy.Name} {enemyAction} for {enemyDamage} damage!");
            }
        }
        else
        {
            // Enemy attacks first
            (enemyDamage, enemyAction) = CalculateEnemyAttack(enemy, character);
            playerHealth -= enemyDamage;
            narrativeParts.Add($"{enemy.Name} {enemyAction} for {enemyDamage} damage!");

            if (playerHealth > 0)
            {
                // Player counterattacks
                (playerDamage, playerAction) = CalculatePlayerAttack(character, enemy);
                enemyHealth -= playerDamage;
                narrativeParts.Add($"{character.Name} {playerAction} for {playerDamage} damage!");
            }
        }

        return new BattleRoundDto(
            roundNumber,
            playerAction,
            playerDamage,
            enemyAction,
            enemyDamage,
            Math.Max(playerHealth, 0),
            Math.Max(enemyHealth, 0),
            string.Join(" ", narrativeParts)
        );
    }

    private (int damage, string action) CalculatePlayerAttack(Character character, Enemy enemy)
    {
        // Base damage from primary stat based on class
        int baseDamage = character.Class switch
        {
            CharacterClass.Warrior => character.Strength,
            CharacterClass.Mage => character.Intelligence,
            CharacterClass.Rogue => character.Agility,
            CharacterClass.Cleric => (character.Intelligence + character.Endurance) / 2,
            CharacterClass.Ranger => (character.Agility + character.Strength) / 2,
            _ => character.Strength
        };

        // Add variance and scaling
        int damage = (int)(baseDamage * 0.3) + _random.Next(5, 15);
        
        // Critical hit check (based on luck)
        bool isCritical = _random.RollChance(character.Luck / 500.0);
        if (isCritical)
        {
            damage = (int)(damage * 1.5);
        }

        // Defense reduction
        int defense = enemy.Endurance / 5;
        damage = Math.Max(1, damage - defense);

        string action = GenerateAttackAction(character.Class, isCritical);
        return (damage, action);
    }

    private (int damage, string action) CalculateEnemyAttack(Enemy enemy, Character character)
    {
        int baseDamage = (enemy.Strength + enemy.Intelligence) / 2;
        int damage = (int)(baseDamage * 0.25) + _random.Next(3, 12);

        // Defense from character's endurance
        int defense = character.Endurance / 5;
        damage = Math.Max(1, damage - defense);

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

    private string GenerateVictoryNarrative(string characterName, string enemyName, int rounds)
    {
        string[] templates =
        {
            $"After {rounds} rounds of intense combat, {characterName} emerges victorious over {enemyName}!",
            $"{characterName} defeats {enemyName} in a fierce battle lasting {rounds} rounds!",
            $"With determination and skill, {characterName} overcomes {enemyName} after {rounds} rounds of combat!",
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
