using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMatchesAddGlobalNpcs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Scenes");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.CreateTable(
                name: "GlobalNpcs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Ascendencia = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Fuerza = table.Column<int>(type: "integer", nullable: false),
                    Velocidad = table.Column<int>(type: "integer", nullable: false),
                    Intelecto = table.Column<int>(type: "integer", nullable: false),
                    Voluntad = table.Column<int>(type: "integer", nullable: false),
                    Discernimiento = table.Column<int>(type: "integer", nullable: false),
                    Presencia = table.Column<int>(type: "integer", nullable: false),
                    MaxHealth = table.Column<int>(type: "integer", nullable: false),
                    MaxConcentration = table.Column<int>(type: "integer", nullable: false),
                    MaxInvestiture = table.Column<int>(type: "integer", nullable: false),
                    Agilidad = table.Column<int>(type: "integer", nullable: false),
                    ArmasLigeras = table.Column<int>(type: "integer", nullable: false),
                    ArmasPesadas = table.Column<int>(type: "integer", nullable: false),
                    Atletismo = table.Column<int>(type: "integer", nullable: false),
                    Hurto = table.Column<int>(type: "integer", nullable: false),
                    Sigilo = table.Column<int>(type: "integer", nullable: false),
                    Deduccion = table.Column<int>(type: "integer", nullable: false),
                    Disciplina = table.Column<int>(type: "integer", nullable: false),
                    Intimidacion = table.Column<int>(type: "integer", nullable: false),
                    Manufactura = table.Column<int>(type: "integer", nullable: false),
                    Medicina = table.Column<int>(type: "integer", nullable: false),
                    Conocimiento = table.Column<int>(type: "integer", nullable: false),
                    Engano = table.Column<int>(type: "integer", nullable: false),
                    Liderazgo = table.Column<int>(type: "integer", nullable: false),
                    Percepcion = table.Column<int>(type: "integer", nullable: false),
                    Perspicacia = table.Column<int>(type: "integer", nullable: false),
                    Persuasion = table.Column<int>(type: "integer", nullable: false),
                    Supervivencia = table.Column<int>(type: "integer", nullable: false),
                    Talentos = table.Column<string>(type: "text", nullable: false),
                    Apariencia = table.Column<string>(type: "text", nullable: false),
                    Notas = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalNpcs", x => x.Id);
                });

            // Seed: Anguila Aérea (Caminapiedras, Apéndice A, p.136)
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
                    DateTime.UtcNow, DateTime.UtcNow
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobalNpcs");

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<long>(type: "bigint", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    Resolution = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scenes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MatchId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scenes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scenes_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_CampaignId",
                table: "Matches",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenes_MatchId",
                table: "Scenes",
                column: "MatchId");
        }
    }
}
