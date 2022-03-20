using Microsoft.EntityFrameworkCore.Migrations;

namespace Api.Migrations
{
    public partial class InputedBool : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Inputed",
                table: "Filenames",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Inputed",
                table: "Filenames");
        }
    }
}
