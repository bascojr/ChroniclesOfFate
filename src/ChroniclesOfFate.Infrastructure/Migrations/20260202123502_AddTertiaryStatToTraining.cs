using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChroniclesOfFate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTertiaryStatToTraining : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Enemies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Strength = table.Column<int>(type: "int", nullable: false),
                    Agility = table.Column<int>(type: "int", nullable: false),
                    Intelligence = table.Column<int>(type: "int", nullable: false),
                    Endurance = table.Column<int>(type: "int", nullable: false),
                    Health = table.Column<int>(type: "int", nullable: false),
                    DifficultyTier = table.Column<int>(type: "int", nullable: false),
                    EnemyType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExperienceReward = table.Column<int>(type: "int", nullable: false),
                    GoldReward = table.Column<int>(type: "int", nullable: false),
                    ReputationReward = table.Column<int>(type: "int", nullable: false),
                    LootTable = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ActiveSeasons = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Abilities = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enemies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainingScenarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PrimaryStat = table.Column<int>(type: "int", nullable: false),
                    SecondaryStat = table.Column<int>(type: "int", nullable: true),
                    TertiaryStat = table.Column<int>(type: "int", nullable: true),
                    BaseStatGain = table.Column<int>(type: "int", nullable: false),
                    SecondaryStatGain = table.Column<int>(type: "int", nullable: false),
                    TertiaryStatGain = table.Column<int>(type: "int", nullable: false),
                    EnergyCost = table.Column<int>(type: "int", nullable: false),
                    BonusChance = table.Column<double>(type: "float", nullable: false),
                    BonusStatGain = table.Column<int>(type: "int", nullable: false),
                    FailureChance = table.Column<double>(type: "float", nullable: false),
                    FailureHealthPenalty = table.Column<int>(type: "int", nullable: false),
                    BonusSeasons = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SeasonalBonusMultiplier = table.Column<double>(type: "float", nullable: false),
                    ExperienceGain = table.Column<int>(type: "int", nullable: false),
                    RequiredLevel = table.Column<int>(type: "int", nullable: false),
                    TrainingNarrative = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingScenarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Player"),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SessionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    FinalScore = table.Column<int>(type: "int", nullable: true),
                    EndingType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessions_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Class = table.Column<int>(type: "int", nullable: false),
                    Strength = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    Agility = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    Intelligence = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    Endurance = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    Charisma = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    Luck = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    CurrentEnergy = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    MaxEnergy = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    CurrentHealth = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    MaxHealth = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    Level = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    Gold = table.Column<int>(type: "int", nullable: false),
                    Reputation = table.Column<int>(type: "int", nullable: false),
                    CurrentYear = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CurrentMonth = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    TotalTurns = table.Column<int>(type: "int", nullable: false),
                    GameSessionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Storybooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Rarity = table.Column<int>(type: "int", nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StrengthBonus = table.Column<int>(type: "int", nullable: false),
                    AgilityBonus = table.Column<int>(type: "int", nullable: false),
                    IntelligenceBonus = table.Column<int>(type: "int", nullable: false),
                    EnduranceBonus = table.Column<int>(type: "int", nullable: false),
                    CharismaBonus = table.Column<int>(type: "int", nullable: false),
                    LuckBonus = table.Column<int>(type: "int", nullable: false),
                    EventTriggerChance = table.Column<double>(type: "float", nullable: false),
                    IsUnlockable = table.Column<bool>(type: "bit", nullable: false),
                    UnlockCondition = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GameSessionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Storybooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Storybooks_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BattleLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    EnemyId = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<int>(type: "int", nullable: false),
                    BattleNarrative = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    RoundsCount = table.Column<int>(type: "int", nullable: false),
                    CharacterPowerAtBattle = table.Column<int>(type: "int", nullable: false),
                    EnemyPowerAtBattle = table.Column<int>(type: "int", nullable: false),
                    ExperienceGained = table.Column<int>(type: "int", nullable: false),
                    GoldGained = table.Column<int>(type: "int", nullable: false),
                    ReputationGained = table.Column<int>(type: "int", nullable: false),
                    HealthLost = table.Column<int>(type: "int", nullable: false),
                    EnergySpent = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Turn = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattleLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BattleLogs_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BattleLogs_Enemies_EnemyId",
                        column: x => x.EnemyId,
                        principalTable: "Enemies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CharacterStorybooks",
                columns: table => new
                {
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    StorybookId = table.Column<int>(type: "int", nullable: false),
                    SlotPosition = table.Column<int>(type: "int", nullable: false),
                    EquippedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterStorybooks", x => new { x.CharacterId, x.StorybookId });
                    table.ForeignKey(
                        name: "FK_CharacterStorybooks_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterStorybooks_Storybooks_StorybookId",
                        column: x => x.StorybookId,
                        principalTable: "Storybooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RandomEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StorybookId = table.Column<int>(type: "int", nullable: true),
                    Rarity = table.Column<int>(type: "int", nullable: false),
                    OutcomeType = table.Column<int>(type: "int", nullable: false),
                    TriggerActions = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TriggerSeasons = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BaseProbability = table.Column<double>(type: "float", nullable: false),
                    StatRequirements = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RandomEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RandomEvents_Storybooks_StorybookId",
                        column: x => x.StorybookId,
                        principalTable: "Storybooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EventChoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RandomEventId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ResultDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StatRequirements = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CheckStat = table.Column<int>(type: "int", nullable: true),
                    CheckDifficulty = table.Column<int>(type: "int", nullable: false),
                    StrengthChange = table.Column<int>(type: "int", nullable: false),
                    AgilityChange = table.Column<int>(type: "int", nullable: false),
                    IntelligenceChange = table.Column<int>(type: "int", nullable: false),
                    EnduranceChange = table.Column<int>(type: "int", nullable: false),
                    CharismaChange = table.Column<int>(type: "int", nullable: false),
                    LuckChange = table.Column<int>(type: "int", nullable: false),
                    EnergyChange = table.Column<int>(type: "int", nullable: false),
                    HealthChange = table.Column<int>(type: "int", nullable: false),
                    GoldChange = table.Column<int>(type: "int", nullable: false),
                    ReputationChange = table.Column<int>(type: "int", nullable: false),
                    ExperienceChange = table.Column<int>(type: "int", nullable: false),
                    FailureDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FailureStrengthChange = table.Column<int>(type: "int", nullable: false),
                    FailureAgilityChange = table.Column<int>(type: "int", nullable: false),
                    FailureIntelligenceChange = table.Column<int>(type: "int", nullable: false),
                    FailureEnduranceChange = table.Column<int>(type: "int", nullable: false),
                    FailureCharismaChange = table.Column<int>(type: "int", nullable: false),
                    FailureLuckChange = table.Column<int>(type: "int", nullable: false),
                    FailureEnergyChange = table.Column<int>(type: "int", nullable: false),
                    FailureHealthChange = table.Column<int>(type: "int", nullable: false),
                    FailureGoldChange = table.Column<int>(type: "int", nullable: false),
                    FailureReputationChange = table.Column<int>(type: "int", nullable: false),
                    FollowUpEventId = table.Column<int>(type: "int", nullable: true),
                    TriggerBattleId = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventChoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventChoices_RandomEvents_RandomEventId",
                        column: x => x.RandomEventId,
                        principalTable: "RandomEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    RandomEventId = table.Column<int>(type: "int", nullable: false),
                    ChosenOptionId = table.Column<int>(type: "int", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Turn = table.Column<int>(type: "int", nullable: false),
                    CheckSucceeded = table.Column<bool>(type: "bit", nullable: true),
                    RollResult = table.Column<int>(type: "int", nullable: true),
                    ResultSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameEvents_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameEvents_EventChoices_ChosenOptionId",
                        column: x => x.ChosenOptionId,
                        principalTable: "EventChoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameEvents_RandomEvents_RandomEventId",
                        column: x => x.RandomEventId,
                        principalTable: "RandomEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BattleLogs_CharacterId",
                table: "BattleLogs",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleLogs_EnemyId",
                table: "BattleLogs",
                column: "EnemyId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_GameSessionId",
                table: "Characters",
                column: "GameSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterStorybooks_StorybookId",
                table: "CharacterStorybooks",
                column: "StorybookId");

            migrationBuilder.CreateIndex(
                name: "IX_EventChoices_RandomEventId",
                table: "EventChoices",
                column: "RandomEventId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEvents_CharacterId",
                table: "GameEvents",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEvents_ChosenOptionId",
                table: "GameEvents",
                column: "ChosenOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEvents_RandomEventId",
                table: "GameEvents",
                column: "RandomEventId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_UserId1",
                table: "GameSessions",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_RandomEvents_StorybookId",
                table: "RandomEvents",
                column: "StorybookId");

            migrationBuilder.CreateIndex(
                name: "IX_Storybooks_GameSessionId",
                table: "Storybooks",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BattleLogs");

            migrationBuilder.DropTable(
                name: "CharacterStorybooks");

            migrationBuilder.DropTable(
                name: "GameEvents");

            migrationBuilder.DropTable(
                name: "TrainingScenarios");

            migrationBuilder.DropTable(
                name: "Enemies");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "EventChoices");

            migrationBuilder.DropTable(
                name: "RandomEvents");

            migrationBuilder.DropTable(
                name: "Storybooks");

            migrationBuilder.DropTable(
                name: "GameSessions");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
