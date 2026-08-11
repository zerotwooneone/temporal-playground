using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemporalDDD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClassNameToWorkflowDefinitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequiredRole",
                table: "Workflow_Nodes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignalName",
                table: "Workflow_Nodes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeoutInMinutes",
                table: "Workflow_Nodes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UIFormSchema",
                table: "Workflow_Nodes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassName",
                table: "Workflow_Definitions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.CreateIndex(
                name: "IX_Workflow_Definitions_ClassName",
                table: "Workflow_Definitions",
                column: "ClassName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Workflow_Instances");

            migrationBuilder.DropIndex(
                name: "IX_Workflow_Definitions_ClassName",
                table: "Workflow_Definitions");

            migrationBuilder.DropColumn(
                name: "RequiredRole",
                table: "Workflow_Nodes");

            migrationBuilder.DropColumn(
                name: "SignalName",
                table: "Workflow_Nodes");

            migrationBuilder.DropColumn(
                name: "TimeoutInMinutes",
                table: "Workflow_Nodes");

            migrationBuilder.DropColumn(
                name: "UIFormSchema",
                table: "Workflow_Nodes");

            migrationBuilder.DropColumn(
                name: "ClassName",
                table: "Workflow_Definitions");
        }
    }
}
