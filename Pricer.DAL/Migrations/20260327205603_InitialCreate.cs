using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricer.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AmountKg = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    EstimatedLengthMeters = table.Column<decimal>(type: "decimal(38,18)", nullable: false),
                    AveragePricePerKgMoney = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Printers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AveragePowerWatts = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    HourlyCostMoney = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Printers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ElectricityPricePerKwhMoney = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FixedCostPerPrintMoney = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TotalCost = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalCost = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    PrintTransaction_MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PrintTransaction_MaterialNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FilamentKg = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    EstimatedMetersUsed = table.Column<decimal>(type: "decimal(38,18)", nullable: true),
                    PrinterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PrinterNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PrintHours = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    MaterialCost = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    ElectricityKwh = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    ElectricityCost = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    PrinterWearCost = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    FixedCost = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    ExtraFixedCost = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    PrintTransaction_Note = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RevertedByTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RevertedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: true),
                    MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaterialNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    KgDelta = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    MetersDelta = table.Column<decimal>(type: "decimal(38,18)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "Printers");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
