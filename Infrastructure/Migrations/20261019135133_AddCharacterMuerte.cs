using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterMuerte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CausaMuerte",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EquipoAlMorir",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<bool>(
                name: "Muerto",
                table: "Characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "MuertoEn",
                table: "Characters",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CausaMuerte",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "EquipoAlMorir",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Muerto",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "MuertoEn",
                table: "Characters");
        }
    }
}
