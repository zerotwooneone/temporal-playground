using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemporalDDD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowDataMappingAndBranching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BranchLabel",
                table: "WorkflowTransitions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InputBindingsJson",
                table: "Workflow_Nodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InputDefinitionsJson",
                table: "Workflow_Nodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OutputDefinitionsJson",
                table: "Workflow_Nodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExpectedInputsJson",
                table: "Workflow_Definitions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchLabel",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "InputBindingsJson",
                table: "Workflow_Nodes");

            migrationBuilder.DropColumn(
                name: "InputDefinitionsJson",
                table: "Workflow_Nodes");

            migrationBuilder.DropColumn(
                name: "OutputDefinitionsJson",
                table: "Workflow_Nodes");

            migrationBuilder.DropColumn(
                name: "ExpectedInputsJson",
                table: "Workflow_Definitions");
        }
    }
}
