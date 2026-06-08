using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class defaultturbinedata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Turbines",
                keyColumn: "Id",
                keyValue: 21);
        }
    }
}
