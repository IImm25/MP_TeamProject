using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class MakeColumnNamesConsistent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "date",
                table: "Plans",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "createdAt",
                table: "Plans",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "departure",
                table: "BoatSchedules",
                newName: "Departure");

            migrationBuilder.RenameColumn(
                name: "arrival",
                table: "BoatSchedules",
                newName: "Arrival");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Plans",
                newName: "date");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Plans",
                newName: "createdAt");

            migrationBuilder.RenameColumn(
                name: "Departure",
                table: "BoatSchedules",
                newName: "departure");

            migrationBuilder.RenameColumn(
                name: "Arrival",
                table: "BoatSchedules",
                newName: "arrival");
        }
    }
}
