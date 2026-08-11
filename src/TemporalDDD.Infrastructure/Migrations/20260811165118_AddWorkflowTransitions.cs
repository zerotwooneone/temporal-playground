using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemporalDDD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowTransitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkflowTransitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkflowDefinitionId = table.Column<string>(type: "TEXT", nullable: false),
                    SourceNodeId = table.Column<string>(type: "TEXT", nullable: false),
                    TargetNodeId = table.Column<string>(type: "TEXT", nullable: false)
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
                name: "IX_WorkflowTransitions_WorkflowDefinitionId",
                table: "WorkflowTransitions",
                column: "WorkflowDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowTransitions");
        }
    }
}
