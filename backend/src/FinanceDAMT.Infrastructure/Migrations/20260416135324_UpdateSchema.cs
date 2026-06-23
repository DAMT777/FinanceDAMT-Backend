using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceDAMT.Infrastructure.Migrations
{
        public partial class UpdateSchema : Migration
    {
                protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MilestonesReached",
                table: "SavingGoals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "AlertSent100",
                table: "Budgets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AlertSent80",
                table: "Budgets",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

                protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MilestonesReached",
                table: "SavingGoals");

            migrationBuilder.DropColumn(
                name: "AlertSent100",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "AlertSent80",
                table: "Budgets");
        }
    }
}
