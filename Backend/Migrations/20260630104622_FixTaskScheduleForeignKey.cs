using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixTaskScheduleForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskSchedules",
                table: "TaskSchedules");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "TaskSchedules");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskSchedules",
                table: "TaskSchedules",
                columns: new[] { "PlanId", "BoatNumber", "TaskItemId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskSchedules",
                table: "TaskSchedules");

            migrationBuilder.AddColumn<int>(
                name: "TaskId",
                table: "TaskSchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskSchedules",
                table: "TaskSchedules",
                columns: new[] { "PlanId", "BoatNumber", "TaskId" });
        }
    }
}
