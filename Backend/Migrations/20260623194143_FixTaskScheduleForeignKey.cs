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
            migrationBuilder.DropForeignKey(
                name: "FK_TaskSchedules_Tasks_TaskItemId",
                table: "TaskSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TaskSchedules_TaskItemId",
                table: "TaskSchedules");

            migrationBuilder.DropColumn(
                name: "TaskItemId",
                table: "TaskSchedules");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSchedules_TaskId",
                table: "TaskSchedules",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskSchedules_Tasks_TaskId",
                table: "TaskSchedules",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskSchedules_Tasks_TaskId",
                table: "TaskSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TaskSchedules_TaskId",
                table: "TaskSchedules");

            migrationBuilder.AddColumn<int>(
                name: "TaskItemId",
                table: "TaskSchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TaskSchedules_TaskItemId",
                table: "TaskSchedules",
                column: "TaskItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskSchedules_Tasks_TaskItemId",
                table: "TaskSchedules",
                column: "TaskItemId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
