using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorldToGlobalNpcs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "World",
                table: "GlobalNpcs",
                type: "text",
                nullable: false,
                defaultValue: "stormlight");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalNpcs_World",
                table: "GlobalNpcs",
                column: "World");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GlobalNpcs_World",
                table: "GlobalNpcs");

            migrationBuilder.DropColumn(
                name: "World",
                table: "GlobalNpcs");
        }
    }
}
