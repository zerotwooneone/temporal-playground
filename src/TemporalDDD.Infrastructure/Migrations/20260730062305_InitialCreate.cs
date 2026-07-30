using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemporalDDD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Configure SQLite for WAL mode at the database level
            // suppressTransaction: true is required because PRAGMA journal_mode cannot be run inside a transaction
            migrationBuilder.Sql("PRAGMA journal_mode=WAL;", suppressTransaction: true);
            
            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PublicId = table.Column<string>(type: "TEXT", nullable: true),
                    ProviderId = table.Column<uint>(type: "INTEGER", nullable: false),
                    FacilityId = table.Column<uint>(type: "INTEGER", nullable: false),
                    PositionId = table.Column<uint>(type: "INTEGER", nullable: false),
                    MatchScore = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ProposedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    AcceptedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CredentialEvaluations",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PublicId = table.Column<string>(type: "TEXT", nullable: true),
                    ProviderId = table.Column<uint>(type: "INTEGER", nullable: false),
                    LicenseNumber = table.Column<string>(type: "TEXT", nullable: false),
                    MedicalBoard = table.Column<string>(type: "TEXT", nullable: false),
                    LicenseExpiryDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    IsCompliant = table.Column<bool>(type: "INTEGER", nullable: false),
                    ComplianceNotes = table.Column<string>(type: "TEXT", nullable: true),
                    EvaluatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredentialEvaluations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Facilities",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    RequiredSpecialties = table.Column<string>(type: "TEXT", nullable: false),
                    AcceptedMedicalBoards = table.Column<string>(type: "TEXT", nullable: false),
                    StandardBillRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    OvertimeBillRate = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlightBookings",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PublicId = table.Column<string>(type: "TEXT", nullable: true),
                    FlightNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Origin = table.Column<string>(type: "TEXT", nullable: false),
                    Destination = table.Column<string>(type: "TEXT", nullable: false),
                    DepartureTime = table.Column<long>(type: "INTEGER", nullable: false),
                    CostAmount = table.Column<string>(type: "TEXT", nullable: false),
                    CostCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    BookedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightBookings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LodgingBookings",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PublicId = table.Column<string>(type: "TEXT", nullable: true),
                    HotelName = table.Column<string>(type: "TEXT", nullable: false),
                    AddressStreet = table.Column<string>(type: "TEXT", nullable: false),
                    AddressCity = table.Column<string>(type: "TEXT", nullable: false),
                    AddressState = table.Column<string>(type: "TEXT", nullable: false),
                    AddressZipCode = table.Column<string>(type: "TEXT", nullable: false),
                    StayPeriodStartUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    StayPeriodEndUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CostAmount = table.Column<string>(type: "TEXT", nullable: false),
                    CostCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    BookedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LodgingBookings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProviderProfiles",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PublicId = table.Column<string>(type: "TEXT", nullable: true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Specialty = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ActivatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Timesheets",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PublicId = table.Column<string>(type: "TEXT", nullable: true),
                    ProviderId = table.Column<uint>(type: "INTEGER", nullable: false),
                    PeriodStartUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    PeriodEndUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalHours = table.Column<decimal>(type: "TEXT", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    GrossPayAmount = table.Column<string>(type: "TEXT", nullable: false),
                    GrossPayCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    TaxAmount = table.Column<string>(type: "TEXT", nullable: false),
                    TaxCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    NetPayAmount = table.Column<string>(type: "TEXT", nullable: false),
                    NetPayCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SubmittedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    ProcessedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    PaymentReference = table.Column<string>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timesheets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_PublicId",
                table: "Assignments",
                column: "PublicId",
                unique: true,
                filter: "PublicId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CredentialEvaluations_PublicId",
                table: "CredentialEvaluations",
                column: "PublicId",
                unique: true,
                filter: "PublicId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FlightBookings_PublicId",
                table: "FlightBookings",
                column: "PublicId",
                unique: true,
                filter: "PublicId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LodgingBookings_PublicId",
                table: "LodgingBookings",
                column: "PublicId",
                unique: true,
                filter: "PublicId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderProfiles_PublicId",
                table: "ProviderProfiles",
                column: "PublicId",
                unique: true,
                filter: "PublicId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Timesheets_PublicId",
                table: "Timesheets",
                column: "PublicId",
                unique: true,
                filter: "PublicId IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "CredentialEvaluations");

            migrationBuilder.DropTable(
                name: "Facilities");

            migrationBuilder.DropTable(
                name: "FlightBookings");

            migrationBuilder.DropTable(
                name: "LodgingBookings");

            migrationBuilder.DropTable(
                name: "ProviderProfiles");

            migrationBuilder.DropTable(
                name: "Timesheets");
        }
    }
}
