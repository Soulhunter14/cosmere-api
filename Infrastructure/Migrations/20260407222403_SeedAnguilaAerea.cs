using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAnguilaAerea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "GlobalNpcs",
                columns: ["Name","Source","Tipo","Ascendencia","Level",
                          "Fuerza","Velocidad","Intelecto","Voluntad","Discernimiento","Presencia",
                          "MaxHealth","MaxConcentration","MaxInvestiture",
                          "Agilidad","ArmasLigeras","ArmasPesadas","Atletismo","Hurto","Sigilo",
                          "Deduccion","Disciplina","Intimidacion","Manufactura","Medicina","Conocimiento",
                          "Engano","Liderazgo","Percepcion","Perspicacia","Persuasion","Supervivencia",
                          "Talentos","Apariencia","Notas","ImageUrl","CreatedAt","UpdatedAt"],
                values: new object[] {
                    "Anguila Aérea", "Caminapiedras", "Secuaz Rango 1", "Animal", 1,
                    2, 3, 0, 1, 3, 0,
                    12, 3, 0,
                    4, 0, 0, 0, 0, 4,
                    0, 0, 0, 0, 0, 0,
                    0, 0, 5, 0, 0, 4,
                    "Sentidos mejorados: ventaja en pruebas de vista que no sean ataques.\nSecuaz: derrotada al sufrir una lesión; sus ataques no pueden provocar impactos críticos.\nResbaladiza: no provoca Acometidas reactivas mientras nada.",
                    "Criatura anfibia larga y delgada con aleta dorsal y aletas similares a alas. Mide 1,20–1,50 metros. Fuera del agua se mantiene en el aire gracias a bolsas de gas hinchables.",
                    "Movimiento: 3m, volar 9m, nadar 12m. Sentidos: 6m (vista).\n\n1 Mordisco: Ataque +4, cercanía 1,5m. Rasguño: 2 (1d4) por laceración. Impacto: 6 (1d4+4) por laceración; si el objetivo es Mediano o menor puede gastar 1 concentración para envolverse en él (queda Retenido).\n\n2 Bomba en picado: Vuela hasta 18m hacia un objetivo y usa Mordisco con ventaja. Después no puede volar hasta el final de la escena.",
                    "/api/gnpc-images/1.png",
                    new DateTime(2026, 4, 8, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2026, 4, 8, 0, 0, 0, DateTimeKind.Utc)
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "GlobalNpcs", keyColumn: "Name", keyValue: "Anguila Aérea");
        }
    }
}
