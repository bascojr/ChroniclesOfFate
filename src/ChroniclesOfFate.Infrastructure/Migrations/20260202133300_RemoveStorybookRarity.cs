using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChroniclesOfFate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStorybookRarity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rarity",
                table: "Storybooks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rarity",
                table: "Storybooks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
