using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SkillSwap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Programming, software development, and IT skills", "Technology" },
                    { 2, "Physics, chemistry, biology, and other natural sciences", "Sciences" },
                    { 3, "Algebra, calculus, statistics, and applied mathematics", "Mathematics" },
                    { 4, "Foreign languages, translation, and linguistics", "Languages" },
                    { 5, "Instruments, music theory, singing, and production", "Music" },
                    { 6, "Exercise, personal training, yoga, and sports", "Fitness" },
                    { 7, "Drawing, painting, graphic design, and photography", "Art & Design" },
                    { 8, "World history, civilizations, and historical research", "History" },
                    { 9, "Creative writing, poetry, and literary analysis", "Literature" },
                    { 10, "Marketing, finance, entrepreneurship, and management", "Business" },
                    { 11, "Culinary arts, baking, and nutrition", "Cooking" },
                    { 12, "Woodworking, sewing, knitting, and home improvement", "Crafts & DIY" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 12);
        }
    }
}
