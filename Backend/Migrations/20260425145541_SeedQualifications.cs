using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedQualifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Qualifications",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Fundamental knowledge of electrical systems in wind turbines.", "Electrical Systems Basics" },
                    { 2, "Safe handling and operation in high-voltage environments.", "High Voltage Safety" },
                    { 3, "Inspection and repair of hydraulic turbine systems.", "Hydraulic Systems Maintenance" },
                    { 4, "Repair of mechanical components like gearboxes and shafts.", "Mechanical Assembly & Repair" },
                    { 5, "Detecting and repairing structural blade damage.", "Blade Inspection & Repair" },
                    { 6, "Monitoring turbines via SCADA systems.", "SCADA Systems Operation" },
                    { 7, "Certified safety procedures for high-altitude work.", "Working at Heights Safety" },
                    { 8, "Fault detection and maintenance of gearboxes.", "Gearbox Diagnostics" },
                    { 9, "Identifying and resolving electrical failures.", "Electrical Fault Diagnosis" },
                    { 10, "Fundamentals of turbine assembly and installation.", "Wind Turbine Installation Basics" },
                    { 11, "Monitoring turbine health using sensor data.", "Condition Monitoring Systems" },
                    { 12, "Understanding vibration and rotor behavior.", "Rotor Dynamics Understanding" },
                    { 13, "Handling turbine emergencies and shutdowns.", "Emergency Response Procedures" },
                    { 14, "Maintaining lubrication systems for moving parts.", "Lubrication Systems Maintenance" },
                    { 15, "Servicing internal nacelle components.", "Nacelle Systems Maintenance" },
                    { 16, "Installation and repair of electrical wiring systems.", "Electrical Cabling & Wiring" },
                    { 17, "Maintenance of converters and inverters.", "Power Conversion Systems" },
                    { 18, "Connecting turbines to power grid infrastructure.", "Grid Connection Systems" },
                    { 19, "Understanding control logic and turbine automation.", "Turbine Control Systems" },
                    { 20, "Planning and scheduling turbine maintenance tasks.", "Preventive Maintenance Planning" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Qualifications",
                keyColumn: "Id",
                keyValue: 20);
        }
    }
}
