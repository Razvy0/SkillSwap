using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSwap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSwapSchedulingAndValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReceiverValidated",
                table: "SwapRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequesterValidated",
                table: "SwapRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeSlotEnd",
                table: "SwapRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeSlotStart",
                table: "SwapRequests",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverValidated",
                table: "SwapRequests");

            migrationBuilder.DropColumn(
                name: "RequesterValidated",
                table: "SwapRequests");

            migrationBuilder.DropColumn(
                name: "TimeSlotEnd",
                table: "SwapRequests");

            migrationBuilder.DropColumn(
                name: "TimeSlotStart",
                table: "SwapRequests");
        }
    }
}
