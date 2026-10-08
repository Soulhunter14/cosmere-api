using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <summary>
    /// Era of the adversaries (1 or 2, null = both eras, as the catalog): a nullable column, and the era of the Mistborn
    /// adversaries seeded by SeedMistbornAdversarios and SeedLegadoAdversarios. The tags come from the PDF bookmarks of each
    /// section («ERA 1» / «ERA 2»; Guía del mundo PDF 222-269, El legado PDF 192-242): a stat block takes the tag of its
    /// section (Vigilante de la ley is in «Agentes de la ley», the Buvidas twins in «Mellizos Buvidas», the society members in
    /// «Miembro de una sociedad»), and an untagged section is for both eras. Only rows of those two sources are updated.
    /// </summary>
    public partial class AddGlobalNpcEra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "Era",
                table: "GlobalNpcs",
                type: "smallint",
                nullable: true);

            // Guía del mundo, era 1 (5)
            migrationBuilder.Sql(@"UPDATE ""GlobalNpcs"" SET ""Era"" = 1 WHERE ""World"" = 'mistborn' AND ""Source"" = 'Guía del mundo' AND ""Name"" IN ("
                + "'Feruquimista', 'Inquisidor de Acero', 'Asesino nacido de la bruma', 'Cortesano nacido de la bruma', 'Obligador');");
            // Guía del mundo, era 2 (9)
            migrationBuilder.Sql(@"UPDATE ""GlobalNpcs"" SET ""Era"" = 2 WHERE ""World"" = 'mistborn' AND ""Source"" = 'Guía del mundo' AND ""Name"" IN ("
                + "'Agente del grupo', 'Alguacil', 'Vigilante de la ley', 'Avatar de Trell', 'Oficial de sangre koloss', 'Piloto malwish', 'Quimera hemalúrgica', 'Quimera hemalúrgica, líder de manada', 'Sangre espectral');");
            // El legado, era 1 (9)
            migrationBuilder.Sql(@"UPDATE ""GlobalNpcs"" SET ""Era"" = 1 WHERE ""World"" = 'mistborn' AND ""Source"" = 'El legado' AND ""Name"" IN ("
                + "'Anastas Elariel', 'Ashweather Cett', 'Bezryl', 'Bestia hemalúrgica', 'Delina Tekiel', 'Duvall Haught', 'Guiverre Seeris', 'Caridus Buvidas', 'Silia Buvidas');");
            // El legado, era 2 (15)
            migrationBuilder.Sql(@"UPDATE ""GlobalNpcs"" SET ""Era"" = 2 WHERE ""World"" = 'mistborn' AND ""Source"" = 'El legado' AND ""Name"" IN ("
                + "'Azmine Wilko', 'Bayron Conrad', 'Capitana Eliane Vorn', 'Monstruosidad hemalúrgica', 'Operativo de Conrad', 'Operativo de élite de Conrad', 'Ingeniero de Conrad', 'Informador ojo de estaño de Conrad', 'Kwylliam Elariel', 'Luchador por la libertad', 'Lysarra Tekiel', 'Iniciado de una sociedad', 'Aspirante de una sociedad', 'Protector del clan koloss', 'Yunque');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Era",
                table: "GlobalNpcs");
        }
    }
}
