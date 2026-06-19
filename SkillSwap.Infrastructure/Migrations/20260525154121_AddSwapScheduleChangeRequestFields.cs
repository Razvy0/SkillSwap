using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSwap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSwapScheduleChangeRequestFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChangeRequestNote",
                table: "SwapSchedules",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ChangeRequestTime",
                table: "SwapSchedules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ChangeRequestedAt",
                table: "SwapSchedules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChangeRequestedById",
                table: "SwapSchedules",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeRequestNote",
                table: "SwapSchedules");

            migrationBuilder.DropColumn(
                name: "ChangeRequestTime",
                table: "SwapSchedules");

            migrationBuilder.DropColumn(
                name: "ChangeRequestedAt",
                table: "SwapSchedules");

            migrationBuilder.DropColumn(
                name: "ChangeRequestedById",
                table: "SwapSchedules");
        }
    }
}
