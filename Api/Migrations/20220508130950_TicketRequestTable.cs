using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    public partial class TicketRequestTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketRequest",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "TicketRequestId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.CreateTable(
                name: "TicketRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsRequested = table.Column<bool>(type: "boolean", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    Duration = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketRequest", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_TicketRequestId",
                table: "Users",
                column: "TicketRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_TicketRequest_TicketRequestId",
                table: "Users",
                column: "TicketRequestId",
                principalTable: "TicketRequest",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_TicketRequest_TicketRequestId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "TicketRequest");

            migrationBuilder.DropIndex(
                name: "IX_Users_TicketRequestId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TicketRequestId",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "TicketRequest",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "Tickets",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "Tickets",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
