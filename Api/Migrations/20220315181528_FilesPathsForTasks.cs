using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Api.Migrations
{
    public partial class FilesPathsForTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AddColumn<string[]>(
                name: "FilesPaths",
                table: "Tasks",
                type: "text[]",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilesPaths",
                table: "Tasks");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Lastname", "Name", "Password", "RoleId", "TicketRequest" },
                values: new object[,]
                {
                    { 1, "base@base.base", "Admin", "Base", "qwerty", 1, false },
                    { 2, "init@init.init", "User", "Init", "qwerty", 2, false }
                });
        }
    }
}
