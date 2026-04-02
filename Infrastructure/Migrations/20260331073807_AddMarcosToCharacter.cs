using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMarcosToCharacter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaxMana",
                table: "Characters",
                newName: "MarcosOpacas");

            migrationBuilder.RenameColumn(
                name: "Mana",
                table: "Characters",
                newName: "MarcosInfusas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MarcosOpacas",
                table: "Characters",
                newName: "MaxMana");

            migrationBuilder.RenameColumn(
                name: "MarcosInfusas",
                table: "Characters",
                newName: "Mana");
        }
    }
}
