using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreacionGuiada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CreacionCompletada",
                table: "Characters",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "CreacionDatos",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<int>(
                name: "CreacionPaso",
                table: "Campaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreacionCompletada",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "CreacionDatos",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "CreacionPaso",
                table: "Campaigns");
        }
    }
}
