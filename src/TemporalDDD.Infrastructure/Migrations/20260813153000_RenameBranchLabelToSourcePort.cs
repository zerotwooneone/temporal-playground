using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemporalDDD.Infrastructure.Migrations
{
    public partial class RenameBranchLabelToSourcePort : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BranchLabel",
                table: "Workflow_Transitions",
                newName: "SourcePort");

            migrationBuilder.AlterColumn<string>(
                name: "SourcePort",
                table: "Workflow_Transitions",
                type: "TEXT",
                nullable: false,
                defaultValue: "Default",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SourcePort",
                table: "Workflow_Transitions",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: false,
                oldDefaultValue: "Default");

            migrationBuilder.RenameColumn(
                name: "SourcePort",
                table: "Workflow_Transitions",
                newName: "BranchLabel");
        }
    }
}
