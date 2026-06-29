using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class GeneraliseLocationAndAddToBoatSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Turbines_LocationId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "Turbines");

            migrationBuilder.AddColumn<int>(
                name: "DestinationId",
                table: "BoatSchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OriginId",
                table: "BoatSchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<float>(type: "real", nullable: false),
                    Longitude = table.Column<float>(type: "real", nullable: false),
                    IsHarbour = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "Id", "IsHarbour", "Latitude", "Longitude", "Name" },
                values: new object[,]
                {
                    { 1, true, 54.4333f, 13.03136f, "Habour" },
                    { 2, false, 54.61092f, 12.63f, "B1" },
                    { 3, false, 54.60553f, 12.63f, "B2" },
                    { 4, false, 54.60014f, 12.63f, "B3" },
                    { 5, false, 54.59475f, 12.63f, "B4" },
                    { 6, false, 54.58936f, 12.63f, "B5" },
                    { 7, false, 54.58397f, 12.63f, "B6" },
                    { 8, false, 54.61658f, 12.64239f, "B7" },
                    { 9, false, 54.61119f, 12.64239f, "B8" },
                    { 10, false, 54.60581f, 12.64239f, "B9" },
                    { 11, false, 54.60042f, 12.64239f, "B10" },
                    { 12, false, 54.59503f, 12.64239f, "B11" },
                    { 13, false, 54.62033f, 12.65475f, "B12" },
                    { 14, false, 54.61494f, 12.65475f, "B13" },
                    { 15, false, 54.60956f, 12.65475f, "B14" },
                    { 16, false, 54.60447f, 12.65508f, "B15" },
                    { 17, false, 54.62358f, 12.66714f, "B16" },
                    { 18, false, 54.61819f, 12.66714f, "B17" },
                    { 19, false, 54.61281f, 12.66714f, "B18" },
                    { 20, false, 54.62714f, 12.6795f, "B19" },
                    { 21, false, 54.62175f, 12.6795f, "B20" },
                    { 22, false, 54.63067f, 12.69189f, "B21" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoatSchedules_DestinationId",
                table: "BoatSchedules",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_BoatSchedules_OriginId",
                table: "BoatSchedules",
                column: "OriginId");

            migrationBuilder.AddForeignKey(
                name: "FK_BoatSchedules_Locations_DestinationId",
                table: "BoatSchedules",
                column: "DestinationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BoatSchedules_Locations_OriginId",
                table: "BoatSchedules",
                column: "OriginId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Locations_LocationId",
                table: "Tasks",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoatSchedules_Locations_DestinationId",
                table: "BoatSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_BoatSchedules_Locations_OriginId",
                table: "BoatSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Locations_LocationId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_BoatSchedules_DestinationId",
                table: "BoatSchedules");

            migrationBuilder.DropIndex(
                name: "IX_BoatSchedules_OriginId",
                table: "BoatSchedules");

            migrationBuilder.DropColumn(
                name: "DestinationId",
                table: "BoatSchedules");

            migrationBuilder.DropColumn(
                name: "OriginId",
                table: "BoatSchedules");

            migrationBuilder.CreateTable(
                name: "Turbines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Latitude = table.Column<float>(type: "real", nullable: false),
                    Longitude = table.Column<float>(type: "real", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turbines", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Turbines",
                columns: new[] { "Id", "Latitude", "Longitude", "Name" },
                values: new object[,]
                {
                    { 1, 54.61092f, 12.63f, "B1" },
                    { 2, 54.60553f, 12.63f, "B2" },
                    { 3, 54.60014f, 12.63f, "B3" },
                    { 4, 54.59475f, 12.63f, "B4" },
                    { 5, 54.58936f, 12.63f, "B5" },
                    { 6, 54.58397f, 12.63f, "B6" },
                    { 7, 54.61658f, 12.64239f, "B7" },
                    { 8, 54.61119f, 12.64239f, "B8" },
                    { 9, 54.60581f, 12.64239f, "B9" },
                    { 10, 54.60042f, 12.64239f, "B10" },
                    { 11, 54.59503f, 12.64239f, "B11" },
                    { 12, 54.62033f, 12.65475f, "B12" },
                    { 13, 54.61494f, 12.65475f, "B13" },
                    { 14, 54.60956f, 12.65475f, "B14" },
                    { 15, 54.60447f, 12.65508f, "B15" },
                    { 16, 54.62358f, 12.66714f, "B16" },
                    { 17, 54.61819f, 12.66714f, "B17" },
                    { 18, 54.61281f, 12.66714f, "B18" },
                    { 19, 54.62714f, 12.6795f, "B19" },
                    { 20, 54.62175f, 12.6795f, "B20" },
                    { 21, 54.63067f, 12.69189f, "B21" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Turbines_LocationId",
                table: "Tasks",
                column: "LocationId",
                principalTable: "Turbines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
