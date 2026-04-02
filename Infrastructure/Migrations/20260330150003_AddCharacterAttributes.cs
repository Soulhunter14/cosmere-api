using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HabilidadPersonalizada1Atributo",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HabilidadPersonalizada2Atributo",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HabilidadPersonalizada3Atributo",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxConcentration",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxInvestiture",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HabilidadPersonalizada1Atributo",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "HabilidadPersonalizada2Atributo",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "HabilidadPersonalizada3Atributo",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "MaxConcentration",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "MaxInvestiture",
                table: "Characters");
        }
    }
}
