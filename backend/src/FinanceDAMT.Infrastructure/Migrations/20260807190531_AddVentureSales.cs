using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceDAMT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVentureSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Income",
                table: "VentureBatches",
                newName: "UnitPrice");

            migrationBuilder.AddColumn<int>(
                name: "UnitsSold",
                table: "VentureBatches",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitsSold",
                table: "VentureBatches");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "VentureBatches",
                newName: "Income");
        }
    }
}
