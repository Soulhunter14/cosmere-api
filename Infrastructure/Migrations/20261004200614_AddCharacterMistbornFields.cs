using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterMistbornFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "Bendiciones",
                table: "Characters",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'");

            migrationBuilder.AddColumn<string>(
                name: "CaminoInicial",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CaminoMetal",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Poderes",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "Recursos",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bendiciones",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "CaminoInicial",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "CaminoMetal",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Poderes",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Recursos",
                table: "Characters");
        }
    }
}
