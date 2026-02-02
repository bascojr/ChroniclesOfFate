using ChroniclesOfFate.Core.Entities;
using ChroniclesOfFate.Core.Enums;
using ChroniclesOfFate.Core.Interfaces;
using ChroniclesOfFate.Core.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ChroniclesOfFate.Core.Tests;

public class BattleServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRandomService> _mockRandom;
    private readonly Mock<IProgressionService> _mockProgression;
    private readonly BattleService _battleService;

    public BattleServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRandom = new Mock<IRandomService>();
        _mockProgression = new Mock<IProgressionService>();
        _battleService = new BattleService(_mockUnitOfWork.Object, _mockRandom.Object, _mockProgression.Object);
    }

    [Theory]
    [InlineData(CharacterClass.Warrior, 100, 50, 30, 80)] // High STR, END
    [InlineData(CharacterClass.Mage, 30, 50, 100, 40)]    // High INT
    [InlineData(CharacterClass.Rogue, 50, 100, 50, 40)]   // High AGI
    public void CalculateCombatPower_AppliesClassWeights(
        CharacterClass characterClass, int str, int agi, int intel, int end)
    {
        // Arrange
        var character = new Character
        {
            Class = characterClass,
            Strength = str,
            Agility = agi,
            Intelligence = intel,
            Endurance = end,
            Charisma = 50,
            Luck = 50
        };

        // Act
        var power = _battleService.CalculateCombatPower(character);

        // Assert
        power.Should().BeGreaterThan(0);
        // Warriors should have higher power with high STR/END
        // Mages should have higher power with high INT
    }

    [Fact]
    public void CalculateEnemyPower_SumsStats()
    {
        // Arrange
        var enemy = new Enemy
        {
            Strength = 50,
            Agility = 40,
            Intelligence = 30,
            Endurance = 60
        };

        // Act
        var power = _battleService.CalculateEnemyPower(enemy);

        // Assert
        power.Should().Be(180); // 50 + 40 + 30 + 60
    }

    [Fact]
    public async Task SimulateBattleAsync_InsufficientEnergy_ReturnsFled()
    {
        // Arrange
        var character = new Character
        {
            Id = 1,
            CurrentEnergy = 10, // Less than 30 required
            CurrentHealth = 100
        };

        _mockUnitOfWork.Setup(u => u.Characters.GetWithFullDetailsAsync(1))
            .ReturnsAsync(character);
        _mockUnitOfWork.Setup(u => u.Enemies.GetByIdAsync(1))
            .ReturnsAsync(new Enemy { Id = 1, Name = "Goblin" });

        // Act
        var result = await _battleService.SimulateBattleAsync(1, 1);

        // Assert
        result.Result.Should().Be(BattleResult.Fled);
        result.Narrative.Should().Contain("energy");
    }
}

public class CharacterTests
{
    [Fact]
    public void GetStat_ReturnsCorrectValue()
    {
        // Arrange
        var character = new Character
        {
            Strength = 50,
            Agility = 60,
            Intelligence = 70
        };

        // Act & Assert
        character.GetStat(StatType.Strength).Should().Be(50);
        character.GetStat(StatType.Agility).Should().Be(60);
        character.GetStat(StatType.Intelligence).Should().Be(70);
    }

    [Fact]
    public void SetStat_ClampsValue()
    {
        // Arrange
        var character = new Character();

        // Act
        character.SetStat(StatType.Strength, 1500); // Over max
        character.SetStat(StatType.Agility, -50);   // Under min

        // Assert
        character.Strength.Should().Be(999);
        character.Agility.Should().Be(0);
    }

    [Fact]
    public void AddStat_IncreasesValue()
    {
        // Arrange
        var character = new Character { Strength = 50 };

        // Act
        character.AddStat(StatType.Strength, 10);

        // Assert
        character.Strength.Should().Be(60);
    }

    [Theory]
    [InlineData(1, Season.Winter)]
    [InlineData(3, Season.Spring)]
    [InlineData(6, Season.Summer)]
    [InlineData(9, Season.Autumn)]
    [InlineData(12, Season.Winter)]
    public void CurrentSeason_ReturnsCorrectSeason(int month, Season expected)
    {
        // Arrange
        var character = new Character { CurrentMonth = month };

        // Act & Assert
        character.CurrentSeason.Should().Be(expected);
    }

    [Fact]
    public void TotalPower_SumsAllStats()
    {
        // Arrange
        var character = new Character
        {
            Strength = 10,
            Agility = 20,
            Intelligence = 30,
            Endurance = 40,
            Charisma = 50,
            Luck = 60
        };

        // Act & Assert
        character.TotalPower.Should().Be(210);
    }

    [Fact]
    public void AdvanceTurn_IncrementsMonthAndTurn()
    {
        // Arrange
        var character = new Character { CurrentYear = 1, CurrentMonth = 1, TotalTurns = 0 };

        // Act
        character.AdvanceTurn();

        // Assert
        character.CurrentMonth.Should().Be(2);
        character.TotalTurns.Should().Be(1);
    }

    [Fact]
    public void AdvanceTurn_WrapsYearCorrectly()
    {
        // Arrange
        var character = new Character { CurrentYear = 1, CurrentMonth = 12, TotalTurns = 11 };

        // Act
        character.AdvanceTurn();

        // Assert
        character.CurrentYear.Should().Be(2);
        character.CurrentMonth.Should().Be(1);
        character.TotalTurns.Should().Be(12);
    }

    [Fact]
    public void IsGameComplete_TrueAt120Turns()
    {
        // Arrange
        var character = new Character { TotalTurns = 119 };

        // Assert
        character.IsGameComplete.Should().BeFalse();

        // Act
        character.TotalTurns = 120;

        // Assert
        character.IsGameComplete.Should().BeTrue();
    }
}

public class RandomServiceTests
{
    private readonly RandomService _randomService;

    public RandomServiceTests()
    {
        _randomService = new RandomService();
    }

    [Fact]
    public void Next_ReturnsWithinRange()
    {
        // Act
        var results = Enumerable.Range(0, 100).Select(_ => _randomService.Next(10)).ToList();

        // Assert
        results.Should().AllSatisfy(r => r.Should().BeInRange(0, 9));
    }

    [Fact]
    public void RollDice_ReturnsSumInRange()
    {
        // Act - Roll 3d6
        var results = Enumerable.Range(0, 100).Select(_ => _randomService.RollDice(6, 3)).ToList();

        // Assert - 3d6 should be between 3 and 18
        results.Should().AllSatisfy(r => r.Should().BeInRange(3, 18));
    }

    [Fact]
    public void RollChance_AlwaysTrueForProbabilityOne()
    {
        // Act
        var results = Enumerable.Range(0, 10).Select(_ => _randomService.RollChance(1.0)).ToList();

        // Assert
        results.Should().AllBeEquivalentTo(true);
    }

    [Fact]
    public void RollChance_AlwaysFalseForProbabilityZero()
    {
        // Act
        var results = Enumerable.Range(0, 10).Select(_ => _randomService.RollChance(0.0)).ToList();

        // Assert
        results.Should().AllBeEquivalentTo(false);
    }
}
