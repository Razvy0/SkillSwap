using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SkillSync.Infrastructure.Migrations
{
    public partial class AddSwapSchedulesAndLessonMode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LessonMode",
                table: "Skills",
                type: "text",
                nullable: false,
                defaultValue: "Both");

            migrationBuilder.AddColumn<int>(
                name: "RequiredSessions",
                table: "Skills",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "RequestedCadence",
                table: "SwapRequests",
                type: "text",
                nullable: false,
                defaultValue: "Single");

            migrationBuilder.AddColumn<string>(
                name: "OfferedCadence",
                table: "SwapRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwoWayScheduleMode",
                table: "SwapRequests",
                type: "text",
                nullable: false,
                defaultValue: "Separate");

            migrationBuilder.AddColumn<int>(
                name: "SwapSessionId",
                table: "TimeTransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SwapSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SwapRequestId = table.Column<int>(type: "integer", nullable: false),
                    Track = table.Column<string>(type: "text", nullable: false),
                    Cadence = table.Column<string>(type: "text", nullable: false),
                    SessionCount = table.Column<int>(type: "integer", nullable: false),
                    WeekDays = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TimeOfDayMinutes = table.Column<int>(type: "integer", nullable: true),
                    SingleSessionStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ProposedById = table.Column<string>(type: "text", nullable: false),
                    ProposedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwapSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SwapSchedules_AspNetUsers_ProposedById",
                        column: x => x.ProposedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SwapSchedules_SwapRequests_SwapRequestId",
                        column: x => x.SwapRequestId,
                        principalTable: "SwapRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SwapSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SwapRequestId = table.Column<int>(type: "integer", nullable: false),
                    SwapScheduleId = table.Column<int>(type: "integer", nullable: true),
                    Track = table.Column<string>(type: "text", nullable: false),
                    SessionOrder = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RequesterValidated = table.Column<bool>(type: "boolean", nullable: false),
                    ReceiverValidated = table.Column<bool>(type: "boolean", nullable: false),
                    ValidatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwapSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SwapSessions_SwapRequests_SwapRequestId",
                        column: x => x.SwapRequestId,
                        principalTable: "SwapRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SwapSessions_SwapSchedules_SwapScheduleId",
                        column: x => x.SwapScheduleId,
                        principalTable: "SwapSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeTransactions_SwapSessionId",
                table: "TimeTransactions",
                column: "SwapSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SwapSchedules_ProposedById",
                table: "SwapSchedules",
                column: "ProposedById");

            migrationBuilder.CreateIndex(
                name: "IX_SwapSchedules_SwapRequestId",
                table: "SwapSchedules",
                column: "SwapRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SwapSessions_SwapRequestId",
                table: "SwapSessions",
                column: "SwapRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SwapSessions_SwapScheduleId",
                table: "SwapSessions",
                column: "SwapScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeTransactions_SwapSessions_SwapSessionId",
                table: "TimeTransactions",
                column: "SwapSessionId",
                principalTable: "SwapSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeTransactions_SwapSessions_SwapSessionId",
                table: "TimeTransactions");

            migrationBuilder.DropTable(
                name: "SwapSessions");

            migrationBuilder.DropTable(
                name: "SwapSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TimeTransactions_SwapSessionId",
                table: "TimeTransactions");

            migrationBuilder.DropColumn(
                name: "LessonMode",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "RequiredSessions",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "RequestedCadence",
                table: "SwapRequests");

            migrationBuilder.DropColumn(
                name: "OfferedCadence",
                table: "SwapRequests");

            migrationBuilder.DropColumn(
                name: "TwoWayScheduleMode",
                table: "SwapRequests");

            migrationBuilder.DropColumn(
                name: "SwapSessionId",
                table: "TimeTransactions");
        }
    }
}
