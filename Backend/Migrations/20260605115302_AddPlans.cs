using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ExecutionIntervalEnd",
                table: "Tasks",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExecutionIntervalStart",
                table: "Tasks",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Turbines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<float>(type: "real", nullable: false),
                    Longitude = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turbines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Boats",
                columns: table => new
                {
                    BoatNumber = table.Column<int>(type: "integer", nullable: false),
                    PlanId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boats", x => new { x.PlanId, x.BoatNumber });
                    table.ForeignKey(
                        name: "FK_Boats_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoatPersons",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    BoatNumber = table.Column<int>(type: "integer", nullable: false),
                    PersonId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoatPersons", x => new { x.PlanId, x.BoatNumber, x.PersonId });
                    table.ForeignKey(
                        name: "FK_BoatPersons_Boats_PlanId_BoatNumber",
                        columns: x => new { x.PlanId, x.BoatNumber },
                        principalTable: "Boats",
                        principalColumns: new[] { "PlanId", "BoatNumber" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoatPersons_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoatSchedules",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    BoatNumber = table.Column<int>(type: "integer", nullable: false),
                    TripNumber = table.Column<int>(type: "integer", nullable: false),
                    departure = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    arrival = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoatSchedules", x => new { x.PlanId, x.BoatNumber, x.TripNumber });
                    table.ForeignKey(
                        name: "FK_BoatSchedules_Boats_PlanId_BoatNumber",
                        columns: x => new { x.PlanId, x.BoatNumber },
                        principalTable: "Boats",
                        principalColumns: new[] { "PlanId", "BoatNumber" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoolTools",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    BoatNumber = table.Column<int>(type: "integer", nullable: false),
                    ToolId = table.Column<int>(type: "integer", nullable: false),
                    RequiredAmount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoolTools", x => new { x.PlanId, x.BoatNumber, x.ToolId });
                    table.ForeignKey(
                        name: "FK_BoolTools_Boats_PlanId_BoatNumber",
                        columns: x => new { x.PlanId, x.BoatNumber },
                        principalTable: "Boats",
                        principalColumns: new[] { "PlanId", "BoatNumber" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoolTools_Tools_ToolId",
                        column: x => x.ToolId,
                        principalTable: "Tools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskSchedules",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "integer", nullable: false),
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    BoatNumber = table.Column<int>(type: "integer", nullable: false),
                    TaskItemId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskSchedules", x => new { x.PlanId, x.BoatNumber, x.TaskId });
                    table.ForeignKey(
                        name: "FK_TaskSchedules_Boats_PlanId_BoatNumber",
                        columns: x => new { x.PlanId, x.BoatNumber },
                        principalTable: "Boats",
                        principalColumns: new[] { "PlanId", "BoatNumber" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskSchedules_Tasks_TaskItemId",
                        column: x => x.TaskItemId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoatPersons_PersonId",
                table: "BoatPersons",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_BoolTools_ToolId",
                table: "BoolTools",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSchedules_TaskItemId",
                table: "TaskSchedules",
                column: "TaskItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoatPersons");

            migrationBuilder.DropTable(
                name: "BoatSchedules");

            migrationBuilder.DropTable(
                name: "BoolTools");

            migrationBuilder.DropTable(
                name: "TaskSchedules");

            migrationBuilder.DropTable(
                name: "Turbines");

            migrationBuilder.DropTable(
                name: "Boats");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropColumn(
                name: "ExecutionIntervalEnd",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ExecutionIntervalStart",
                table: "Tasks");
        }
    }
}
