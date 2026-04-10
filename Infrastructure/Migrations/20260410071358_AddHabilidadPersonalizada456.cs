using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHabilidadPersonalizada456 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HabilidadPersonalizada4",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HabilidadPersonalizada4Atributo",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "HabilidadPersonalizada4Valor",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HabilidadPersonalizada5",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HabilidadPersonalizada5Atributo",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "HabilidadPersonalizada5Valor",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HabilidadPersonalizada6",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HabilidadPersonalizada6Atributo",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "HabilidadPersonalizada6Valor",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HabilidadPersonalizada4",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "HabilidadPersonalizada4Atributo",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "HabilidadPersonalizada4Valor",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "HabilidadPersonalizada5",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "HabilidadPersonalizada5Atributo",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "HabilidadPersonalizada5Valor",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "HabilidadPersonalizada6",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "HabilidadPersonalizada6Atributo",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "HabilidadPersonalizada6Valor",
                table: "Characters");
        }
    }
}
