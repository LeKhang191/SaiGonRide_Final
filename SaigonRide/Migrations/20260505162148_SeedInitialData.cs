using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaigonRide.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Vehicles_VehicleId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_VehicleId",
                table: "Transactions");

            migrationBuilder.InsertData(
                table: "Stations",
                columns: new[] { "StationId", "CurrentInventory", "Location", "MaxCapacity", "Name" },
                values: new object[] { 1, 10, "Bai Sau", 0, "Vung Tau Food Tour Station" });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "VehicleId", "StationId", "Status", "Type", "VehicleNumber" },
                values: new object[] { 1, 1, "Available", "Electric Bike", "72-VungTau-001" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Stations",
                keyColumn: "StationId",
                keyValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_VehicleId",
                table: "Transactions",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Vehicles_VehicleId",
                table: "Transactions",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
