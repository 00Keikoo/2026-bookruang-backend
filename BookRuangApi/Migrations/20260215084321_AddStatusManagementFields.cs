using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookRuangApi.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "RoomLoans",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "RoomLoans",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RoomLoans",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "RoomLoans",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "RoomLoans",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "RoomLoans",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedBy",
                table: "RoomLoans",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "RoomLoans",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RoomLoans",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "RoomLoans");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "RoomLoans");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RoomLoans");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "RoomLoans");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "RoomLoans");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "RoomLoans");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "RoomLoans");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "RoomLoans");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RoomLoans");
        }
    }
}
