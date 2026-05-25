using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSync.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonTypeAndSwapRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LearnerId",
                table: "SwapRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LessonType",
                table: "SwapRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TeacherId",
                table: "SwapRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LearnerId",
                table: "SwapRequests");

            migrationBuilder.DropColumn(
                name: "LessonType",
                table: "SwapRequests");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "SwapRequests");
        }
    }
}
