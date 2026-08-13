using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemporalDDD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicalInputsJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthToken",
                table: "Workflow_Nodes");

            migrationBuilder.RenameColumn(
                name: "MessageTemplate",
                table: "Workflow_Nodes",
                newName: "TechnicalInputsJson");

            migrationBuilder.RenameColumn(
                name: "EndpointUrl",
                table: "Workflow_Nodes",
                newName: "ApiWorkflowNodeDbo_TechnicalInputsJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TechnicalInputsJson",
                table: "Workflow_Nodes",
                newName: "MessageTemplate");

            migrationBuilder.RenameColumn(
                name: "ApiWorkflowNodeDbo_TechnicalInputsJson",
                table: "Workflow_Nodes",
                newName: "EndpointUrl");

            migrationBuilder.AddColumn<string>(
                name: "AuthToken",
                table: "Workflow_Nodes",
                type: "TEXT",
                nullable: true);
        }
    }
}
