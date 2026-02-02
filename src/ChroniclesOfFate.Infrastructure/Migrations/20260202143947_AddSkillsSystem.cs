using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChroniclesOfFate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailureGrantSkillId",
                table: "EventChoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GrantSkillId",
                table: "EventChoices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SkillType = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<int>(type: "int", nullable: false),
                    PassiveEffect = table.Column<int>(type: "int", nullable: true),
                    PassiveValue = table.Column<double>(type: "float", nullable: false),
                    TriggerChance = table.Column<double>(type: "float", nullable: false),
                    BaseDamage = table.Column<int>(type: "int", nullable: false),
                    ScalingStat = table.Column<int>(type: "int", nullable: true),
                    ScalingMultiplier = table.Column<double>(type: "float", nullable: false),
                    ActiveNarrative = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BonusEffect = table.Column<int>(type: "int", nullable: true),
                    BonusPercentage = table.Column<double>(type: "float", nullable: false),
                    BonusFlatValue = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CharacterSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    AcquiredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcquiredOnTurn = table.Column<int>(type: "int", nullable: false),
                    AcquisitionSource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterSkills_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventChoices_FailureGrantSkillId",
                table: "EventChoices",
                column: "FailureGrantSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_EventChoices_GrantSkillId",
                table: "EventChoices",
                column: "GrantSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSkills_CharacterId_SkillId",
                table: "CharacterSkills",
                columns: new[] { "CharacterId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSkills_SkillId",
                table: "CharacterSkills",
                column: "SkillId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventChoices_Skills_FailureGrantSkillId",
                table: "EventChoices",
                column: "FailureGrantSkillId",
                principalTable: "Skills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventChoices_Skills_GrantSkillId",
                table: "EventChoices",
                column: "GrantSkillId",
                principalTable: "Skills",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventChoices_Skills_FailureGrantSkillId",
                table: "EventChoices");

            migrationBuilder.DropForeignKey(
                name: "FK_EventChoices_Skills_GrantSkillId",
                table: "EventChoices");

            migrationBuilder.DropTable(
                name: "CharacterSkills");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_EventChoices_FailureGrantSkillId",
                table: "EventChoices");

            migrationBuilder.DropIndex(
                name: "IX_EventChoices_GrantSkillId",
                table: "EventChoices");

            migrationBuilder.DropColumn(
                name: "FailureGrantSkillId",
                table: "EventChoices");

            migrationBuilder.DropColumn(
                name: "GrantSkillId",
                table: "EventChoices");
        }
    }
}
