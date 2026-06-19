using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSwap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimeTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    SwapRequestId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimeTransactions_SwapRequests_SwapRequestId",
                        column: x => x.SwapRequestId,
                        principalTable: "SwapRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeTransactions_SwapRequestId",
                table: "TimeTransactions",
                column: "SwapRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTransactions_UserId",
                table: "TimeTransactions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeTransactions");
        }
    }
}
