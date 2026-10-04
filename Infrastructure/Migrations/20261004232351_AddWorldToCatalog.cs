using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorldToCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CatalogOptions_Category",
                table: "CatalogOptions");

            migrationBuilder.AddColumn<short>(
                name: "Era",
                table: "WeaponCatalog",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRewardOnly",
                table: "WeaponCatalog",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "WeaponCatalog",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "World",
                table: "WeaponCatalog",
                type: "text",
                nullable: false,
                defaultValue: "stormlight");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "GearItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "Era",
                table: "GearItems",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRewardOnly",
                table: "GearItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "World",
                table: "GearItems",
                type: "text",
                nullable: false,
                defaultValue: "stormlight");

            migrationBuilder.AddColumn<string>(
                name: "World",
                table: "CatalogOptions",
                type: "text",
                nullable: false,
                defaultValue: "stormlight");

            migrationBuilder.AddColumn<short>(
                name: "Era",
                table: "ArmorCatalog",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRewardOnly",
                table: "ArmorCatalog",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "ArmorCatalog",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "World",
                table: "ArmorCatalog",
                type: "text",
                nullable: false,
                defaultValue: "stormlight");

            migrationBuilder.CreateIndex(
                name: "IX_WeaponCatalog_World",
                table: "WeaponCatalog",
                column: "World");

            migrationBuilder.CreateIndex(
                name: "IX_GearItems_World",
                table: "GearItems",
                column: "World");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogOptions_World_Category",
                table: "CatalogOptions",
                columns: new[] { "World", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_ArmorCatalog_World",
                table: "ArmorCatalog",
                column: "World");

            // Data of M3 (spec 4.3): section "-- M3" of seed-mistborn.sql, copied as is. Runs after the columns and
            // indexes above: the options shared by every world move to 'cosmere' and the four IDENTITY sequences catch
            // up with the ids that SeedCatalogData inserted explicitly (they never advanced).
            migrationBuilder.Sql(@"-- =====================================================================================================================
-- M3
-- Migración AddWorldToCatalog (T39a). Se ejecuta DESPUÉS de crear las columnas World, Era, IsRewardOnly, Price y Category
-- y sus índices (§4.1). Solo toca datos existentes: no inserta filas.
-- =====================================================================================================================

-- (1) Opciones existentes compartidas por Archivo de las Tormentas y Nacidos de la Bruma: pasan de stormlight (DEFAULT) a cosmere.
--     Mapa del informe 07 §5.2 y UPDATE de la especificación §4.3 M3 (50 ids, resultado esperado: 50 filas actualizadas).
--     Siguen en stormlight: range 41 y 45, weapon_trait 55, 59 y 63, armor_type 72, 75, 76 y 77, armor_trait 84.
UPDATE ""CatalogOptions"" SET ""World"" = 'cosmere' WHERE ""Id"" IN (
    1, 2, 3,                                                                 -- weapon_type
    10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27,   -- skill
    30, 31, 32,                                                              -- damage_type
    40, 42, 43, 44, 46,                                                      -- range
    50, 51, 52, 53, 54, 56, 57, 58, 60, 61, 62, 64,                          -- weapon_trait
    70, 71, 73, 74,                                                          -- armor_type
    80, 81, 82, 83, 85                                                       -- armor_trait
);

-- (2) Secuencias IDENTITY al máximo id existente. 20260410143041_SeedCatalogData insertó ids explícitos sin avanzar las
--     secuencias: sin esto, el primer alta con id autogenerado (POST /catalog/weapons) chocaría con un id sembrado.
SELECT setval(pg_get_serial_sequence('""WeaponCatalog""','Id'), (SELECT MAX(""Id"") FROM ""WeaponCatalog""));
SELECT setval(pg_get_serial_sequence('""ArmorCatalog""','Id'), (SELECT MAX(""Id"") FROM ""ArmorCatalog""));
SELECT setval(pg_get_serial_sequence('""GearItems""','Id'), (SELECT MAX(""Id"") FROM ""GearItems""));
SELECT setval(pg_get_serial_sequence('""CatalogOptions""','Id'), (SELECT MAX(""Id"") FROM ""CatalogOptions""));
-- fin de M3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WeaponCatalog_World",
                table: "WeaponCatalog");

            migrationBuilder.DropIndex(
                name: "IX_GearItems_World",
                table: "GearItems");

            migrationBuilder.DropIndex(
                name: "IX_CatalogOptions_World_Category",
                table: "CatalogOptions");

            migrationBuilder.DropIndex(
                name: "IX_ArmorCatalog_World",
                table: "ArmorCatalog");

            migrationBuilder.DropColumn(
                name: "Era",
                table: "WeaponCatalog");

            migrationBuilder.DropColumn(
                name: "IsRewardOnly",
                table: "WeaponCatalog");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "WeaponCatalog");

            migrationBuilder.DropColumn(
                name: "World",
                table: "WeaponCatalog");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "GearItems");

            migrationBuilder.DropColumn(
                name: "Era",
                table: "GearItems");

            migrationBuilder.DropColumn(
                name: "IsRewardOnly",
                table: "GearItems");

            migrationBuilder.DropColumn(
                name: "World",
                table: "GearItems");

            migrationBuilder.DropColumn(
                name: "World",
                table: "CatalogOptions");

            migrationBuilder.DropColumn(
                name: "Era",
                table: "ArmorCatalog");

            migrationBuilder.DropColumn(
                name: "IsRewardOnly",
                table: "ArmorCatalog");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ArmorCatalog");

            migrationBuilder.DropColumn(
                name: "World",
                table: "ArmorCatalog");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogOptions_Category",
                table: "CatalogOptions",
                column: "Category");
        }
    }
}
