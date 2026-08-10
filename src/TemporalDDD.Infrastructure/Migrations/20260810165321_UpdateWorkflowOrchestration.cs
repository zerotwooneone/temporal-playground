using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemporalDDD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkflowOrchestration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Workflow_Definitions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    PublicId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    FlowJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflow_Definitions", x => x.Id);
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
                    RetryPolicyMaxAttempts = table.Column<int>(type: "INTEGER", nullable: true),
                    RetryPolicyBackoffCoefficient = table.Column<int>(type: "INTEGER", nullable: true),
                    ContractMappingConvertXmlToJson = table.Column<bool>(type: "INTEGER", nullable: true),
                    ContractMappingQueryParameters = table.Column<string>(type: "TEXT", nullable: true),
                    ContractMappingRequestMapping = table.Column<string>(type: "TEXT", nullable: true),
                    ContractMappingResponseMapping = table.Column<string>(type: "TEXT", nullable: true),
                    EndpointUrl = table.Column<string>(type: "TEXT", nullable: true),
                    AuthToken = table.Column<string>(type: "TEXT", nullable: true),
                    MessageTemplate = table.Column<string>(type: "TEXT", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_Identity_Users_PublicId",
                table: "Identity_Users",
                column: "PublicId",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Identity_Roles");

            migrationBuilder.DropTable(
                name: "Identity_Users");

            migrationBuilder.DropTable(
                name: "Workflow_Nodes");

            migrationBuilder.DropTable(
                name: "Workflow_Definitions");
        }
    }
}
