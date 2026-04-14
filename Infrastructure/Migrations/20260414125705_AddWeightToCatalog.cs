using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWeightToCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "WeaponCatalog",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "ArmorCatalog",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            // Seed weapon weights from the Manual de Juego
            migrationBuilder.Sql(@"
UPDATE ""WeaponCatalog"" SET ""Weight"" = CASE ""Id""
  WHEN 1  THEN 1.0   -- Arco corto
  WHEN 2  THEN 2.0   -- Bastón
  WHEN 3  THEN 0.5   -- Cuchillo
  WHEN 4  THEN 1.0   -- Espada lateral
  WHEN 5  THEN 1.0   -- Espada ropera
  WHEN 6  THEN 0.5   -- Honda
  WHEN 7  THEN 1.0   -- Jabalina
  WHEN 8  THEN 1.5   -- Lanza corta
  WHEN 9  THEN 1.5   -- Maza
  WHEN 10 THEN 2.5   -- Alabarda
  WHEN 11 THEN 1.5   -- Arco largo
  WHEN 12 THEN 3.5   -- Ballesta
  WHEN 13 THEN 1.0   -- Escudo
  WHEN 14 THEN 1.5   -- Espada larga
  WHEN 15 THEN 1.0   -- Hacha
  WHEN 16 THEN 4.5   -- Lanza larga
  WHEN 17 THEN 3.5   -- Mandoble
  WHEN 18 THEN 4.0   -- Martillo
  WHEN 19 THEN 0.0   -- Arma improvisada
  WHEN 20 THEN 0.0   -- Ataque sin armas
  WHEN 21 THEN 10.0  -- Gran arco
  WHEN 22 THEN 2.0   -- Hoja esquirlada
  WHEN 23 THEN 0.0   -- Hoja esquirlada (Radiante)
  WHEN 24 THEN 75.0  -- Martillo de guerra
  WHEN 25 THEN 5.0   -- Semiesquirla
  ELSE 0.0
END
WHERE ""IsCustom"" = false;
");

            // Seed armor weights from the Manual de Juego
            migrationBuilder.Sql(@"
UPDATE ""ArmorCatalog"" SET ""Weight"" = CASE ""Id""
  WHEN 1 THEN 2.5    -- Uniforme
  WHEN 2 THEN 5.0    -- Cuero
  WHEN 3 THEN 12.5   -- Malla
  WHEN 4 THEN 15.0   -- Coraza
  WHEN 5 THEN 20.0   -- Armadura de placas y malla
  WHEN 6 THEN 27.5   -- Armadura completa
  WHEN 7 THEN 700.0  -- Armadura esquirlada
  WHEN 8 THEN 0.0    -- Armadura esquirlada (Radiante)
  ELSE 0.0
END
WHERE ""IsCustom"" = false;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Weight",
                table: "WeaponCatalog");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "ArmorCatalog");
        }
    }
}
