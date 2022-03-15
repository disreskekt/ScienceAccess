using Microsoft.EntityFrameworkCore.Migrations;

namespace Api.Migrations
{
    public partial class SplitFilePathForTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilesPaths",
                table: "Tasks",
                newName: "FileNames");

            migrationBuilder.AddColumn<string>(
                name: "DirectoryPath",
                table: "Tasks",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DirectoryPath",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "FileNames",
                table: "Tasks",
                newName: "FilesPaths");
        }
    }
}
