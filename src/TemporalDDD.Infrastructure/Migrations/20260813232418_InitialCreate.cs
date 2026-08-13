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
            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    PublicId = table.Column<string>(type: "TEXT", nullable: true),
                    ProviderId = table.Column<string>(type: "TEXT", nullable: false),
                    FacilityId = table.Column<string>(type: "TEXT", nullable: false),
                    PositionId = table.Column<string>(type: "TEXT", nullable: false),
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
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    PublicId = table.Column<string>(type: "TEXT", nullable: true),
                    ProviderId = table.Column<string>(type: "TEXT", nullable: false),
                    LicenseNumber = table.Column<string>(type: "TEXT", nullable: false),
                    MedicalBoard = table.Column<string>(type: "TEXT", nullable: false),
                    LicenseExpiryDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    IsCompliant = table.Column<bool>(type: "INTEGER", nullable: false),
                    ComplianceNotes = table.Column<string>(type: "TEXT", nullable: true),
                    EvaluatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkflowId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredentialEvaluations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Facilities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
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
                    Id = table.Column<string>(type: "TEXT", nullable: false),
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
                name: "Identity_Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    PermissionsJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identity_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Identity_Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    PublicId = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    AssignedRolesJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identity_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LodgingBookings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
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
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    PublicId = table.Column<string>(type: "TEXT", nullable: true),
                    ProviderId = table.Column<string>(type: "TEXT", nullable: false),
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
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    PublicId = table.Column<string>(type: "TEXT", nullable: true),
                    ProviderId = table.Column<string>(type: "TEXT", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Workflow_Definitions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    PublicId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ClassName = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    FlowJson = table.Column<string>(type: "TEXT", nullable: false),
                    ExpectedInputsJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflow_Definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workflow_Instances",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    PublicId = table.Column<string>(type: "TEXT", nullable: false),
                    WorkflowDefinitionId = table.Column<string>(type: "TEXT", nullable: false),
                    BusinessReferenceId = table.Column<string>(type: "TEXT", nullable: false),
                    TemporalRunId = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ContextData = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflow_Instances", x => x.Id);
                    table.UniqueConstraint("AK_Workflow_Instances_PublicId", x => x.PublicId);
                });

            migrationBuilder.CreateTable(
                name: "Workflow_Nodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    WorkflowDefinitionId = table.Column<string>(type: "TEXT", nullable: false),
                    NodeType = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    BusinessNotes = table.Column<string>(type: "TEXT", nullable: true),
                    IsConfigured = table.Column<bool>(type: "INTEGER", nullable: false),
                    InputDefinitionsJson = table.Column<string>(type: "TEXT", nullable: false),
                    OutputDefinitionsJson = table.Column<string>(type: "TEXT", nullable: false),
                    InputBindingsJson = table.Column<string>(type: "TEXT", nullable: false),
                    RetryPolicyMaxAttempts = table.Column<int>(type: "INTEGER", nullable: true),
                    RetryPolicyBackoffCoefficient = table.Column<int>(type: "INTEGER", nullable: true),
                    ContractMappingConvertXmlToJson = table.Column<bool>(type: "INTEGER", nullable: true),
                    ContractMappingQueryParameters = table.Column<string>(type: "TEXT", nullable: true),
                    ContractMappingRequestMapping = table.Column<string>(type: "TEXT", nullable: true),
                    ContractMappingResponseMapping = table.Column<string>(type: "TEXT", nullable: true),
                    ApiWorkflowNodeDbo_TechnicalInputsJson = table.Column<string>(type: "TEXT", nullable: true),
                    RequiredRole = table.Column<string>(type: "TEXT", nullable: true),
                    SignalName = table.Column<string>(type: "TEXT", nullable: true),
                    TimeoutInMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    UIFormSchema = table.Column<string>(type: "TEXT", nullable: true),
                    TechnicalInputsJson = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflow_Nodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workflow_Nodes_Workflow_Definitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "Workflow_Definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTransitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkflowDefinitionId = table.Column<string>(type: "TEXT", nullable: false),
                    SourceNodeId = table.Column<string>(type: "TEXT", nullable: false),
                    TargetNodeId = table.Column<string>(type: "TEXT", nullable: false),
                    SourcePort = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_Workflow_Definitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "Workflow_Definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_Identity_Users_PublicId",
                table: "Identity_Users",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LodgingBookings_PublicId",
                table: "LodgingBookings",
                column: "PublicId",
                unique: true,
                filter: "PublicId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderProfiles_ProviderId",
                table: "ProviderProfiles",
                column: "ProviderId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Workflow_Definitions_ClassName",
                table: "Workflow_Definitions",
                column: "ClassName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workflow_Definitions_PublicId",
                table: "Workflow_Definitions",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workflow_Nodes_WorkflowDefinitionId",
                table: "Workflow_Nodes",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_WorkflowDefinitionId",
                table: "WorkflowTransitions",
                column: "WorkflowDefinitionId");
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
                name: "Identity_Roles");

            migrationBuilder.DropTable(
                name: "Identity_Users");

            migrationBuilder.DropTable(
                name: "LodgingBookings");

            migrationBuilder.DropTable(
                name: "ProviderProfiles");

            migrationBuilder.DropTable(
                name: "Timesheets");

            migrationBuilder.DropTable(
                name: "Workflow_Instances");

            migrationBuilder.DropTable(
                name: "Workflow_Nodes");

            migrationBuilder.DropTable(
                name: "WorkflowTransitions");

            migrationBuilder.DropTable(
                name: "Workflow_Definitions");
        }
    }
}
