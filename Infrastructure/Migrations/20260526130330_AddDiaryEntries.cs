using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDiaryEntries : Migration
    {
        private static readonly DateTime _seed = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiaryEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Preview = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Participants = table.Column<List<string>>(type: "text[]", nullable: false),
                    MentionsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiaryEntries_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiaryEntries_CampaignId_Number",
                table: "DiaryEntries",
                columns: new[] { "CampaignId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiaryEntries_CampaignId_Slug",
                table: "DiaryEntries",
                columns: new[] { "CampaignId", "Slug" },
                unique: true);

            // ── Seed: campaña 1 ───────────────────────────────────────────────

            migrationBuilder.InsertData(
                table: "DiaryEntries",
                columns: new[] { "CampaignId", "Number", "Title", "Slug", "Preview", "Body", "Participants", "MentionsJson", "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    1L, 1, "El Encuentro", "sesion-01",
                    "En las Montañas Irreclamadas, el destino unió a [[PJ - Guizmo]], [[PJ - Roca Pequeña]], [[PJ - Kaligula]], [[PJ - Hanol]] y [[PJ - Raito]].",
                    "En las Montañas Irreclamadas, el destino unió a [[PJ - Guizmo]], [[PJ - Roca Pequeña]], [[PJ - Kaligula]], [[PJ - Hanol]] y [[PJ - Raito]].\n\nAllí encontraron a [[PJ - Taszo-hijo-Clutio]], gravemente herido tras el ataque de sabuesos-hacha.\n\nEl monje, portador de una misteriosa piedra ([[Spren - Po'ahu]]), pidió escolta hacia el este.\n\nEl grupo aceptó, iniciando un viaje que los llevaría hasta la Encrucijada de la Piedra del Concilio.\n\nLa sesión termina con la creciente tensión en el campamento y la inminente amenaza que se cierne sobre Taszo.",
                    new[] { "Guizmo", "Roca Pequeña", "Kaligula", "Hanol", "Raito", "Taszo-hijo-Clutio" },
                    "[{\"raw\":\"PJ - Guizmo\",\"display\":\"Guizmo\",\"type\":\"pj\"},{\"raw\":\"PJ - Roca Pequeña\",\"display\":\"Roca Pequeña\",\"type\":\"pj\"},{\"raw\":\"PJ - Kaligula\",\"display\":\"Kaligula\",\"type\":\"pj\"},{\"raw\":\"PJ - Hanol\",\"display\":\"Hanol\",\"type\":\"pj\"},{\"raw\":\"PJ - Raito\",\"display\":\"Raito\",\"type\":\"pj\"},{\"raw\":\"PJ - Taszo-hijo-Clutio\",\"display\":\"Taszo-hijo-Clutio\",\"type\":\"pj\"},{\"raw\":\"Spren - Po'ahu\",\"display\":\"Po'ahu\",\"type\":\"spren\"}]",
                    _seed, _seed
                });

            migrationBuilder.InsertData(
                table: "DiaryEntries",
                columns: new[] { "CampaignId", "Number", "Title", "Slug", "Preview", "Body", "Participants", "MentionsJson", "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    1L, 2, "La Trampa", "sesion-02",
                    "En la Encrucijada, el grupo presencia la dureza del ejército alezi y conoce rumores sobre un prisionero que afirma ser un Heraldo.",
                    "En la Encrucijada, el grupo presencia la dureza del ejército alezi y conoce rumores sobre un prisionero que afirma ser un Heraldo.\n\nMientras tanto, [[PJ - Taszo-hijo-Clutio]] revela su misión: recuperar la Hoja de Honor.\n\nLa calma se rompe cuando [[NPC - Hana]] pide ayuda para rescatar el carro de [[NPC - Tet Rebin]].\n\n[[PJ - Roca Pequeña]] y [[PJ - Raito]] acuden en su ayuda, sin saber que se trata de una emboscada.\n\nLa sesión termina justo cuando el grupo se dispone a intervenir en la defensa de la caravana.",
                    new[] { "Taszo-hijo-Clutio", "Roca Pequeña", "Raito" },
                    "[{\"raw\":\"PJ - Taszo-hijo-Clutio\",\"display\":\"Taszo-hijo-Clutio\",\"type\":\"pj\"},{\"raw\":\"NPC - Hana\",\"display\":\"Hana\",\"type\":\"npc\"},{\"raw\":\"NPC - Tet Rebin\",\"display\":\"Tet Rebin\",\"type\":\"npc\"},{\"raw\":\"PJ - Roca Pequeña\",\"display\":\"Roca Pequeña\",\"type\":\"pj\"},{\"raw\":\"PJ - Raito\",\"display\":\"Raito\",\"type\":\"pj\"}]",
                    _seed, _seed
                });

            migrationBuilder.InsertData(
                table: "DiaryEntries",
                columns: new[] { "CampaignId", "Number", "Title", "Slug", "Preview", "Body", "Participants", "MentionsJson", "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    1L, 3, "El Sacrificio", "sesion-03",
                    "La emboscada se desata.",
                    "La emboscada se desata.\n\nDurante el combate, [[PJ - Roca Pequeña]] manifiesta por primera vez un poder sobrenatural vinculado a [[Spren - Magma]].\n\nEn paralelo, [[PJ - Kaligula]] descubre el robo de la Hoja de Honor por parte de [[NPC - Kaiana]].\n\n[[PJ - Taszo-hijo-Clutio]] huye hacia la tormenta en persecución de los ladrones.\n\nEl grupo lo sigue, encontrándolo mortalmente herido por agentes de [[Facción - Ojos de Pala]].\n\nEn su último aliento, Taszo les hace jurar continuar su misión y les deja el nombre de [[PJ - Liss]].\n\nSu muerte marca un punto de no retorno para el grupo.",
                    new[] { "Roca Pequeña", "Kaligula", "Taszo-hijo-Clutio", "Liss" },
                    "[{\"raw\":\"PJ - Roca Pequeña\",\"display\":\"Roca Pequeña\",\"type\":\"pj\"},{\"raw\":\"Spren - Magma\",\"display\":\"Magma\",\"type\":\"spren\"},{\"raw\":\"PJ - Kaligula\",\"display\":\"Kaligula\",\"type\":\"pj\"},{\"raw\":\"NPC - Kaiana\",\"display\":\"Kaiana\",\"type\":\"npc\"},{\"raw\":\"PJ - Taszo-hijo-Clutio\",\"display\":\"Taszo-hijo-Clutio\",\"type\":\"pj\"},{\"raw\":\"Facción - Ojos de Pala\",\"display\":\"Ojos de Pala\",\"type\":\"faction\"},{\"raw\":\"PJ - Liss\",\"display\":\"Liss\",\"type\":\"pj\"}]",
                    _seed, _seed
                });

            migrationBuilder.InsertData(
                table: "DiaryEntries",
                columns: new[] { "CampaignId", "Number", "Title", "Slug", "Preview", "Body", "Participants", "MentionsJson", "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    1L, 4, "Ecos en el Campamento", "sesion-04",
                    "El grupo llega a los Campamentos de Guerra.",
                    "El grupo llega a los Campamentos de Guerra.\n\nBuscan información sobre [[PJ - Liss]], descubriendo pistas que los llevan al [[Lugar - Rocabrote Rojo]].\n\n[[PJ - Kaligula]] tiene una visión de un encapuchado: [[NPC - Mraize]].\n\nEl grupo interactúa con mercaderes y soldados, y recibe recompensas de [[NPC - Tet Rebin]].\n\n[[PJ - Raito]] ayuda a los olvidados en la zona médica, donde jura proteger a los portadores del [[Lugar - Puente Nueve]].\n\nLa sesión termina con el grupo planificando sus movimientos para el día siguiente.",
                    new[] { "Liss", "Kaligula", "Raito" },
                    "[{\"raw\":\"PJ - Liss\",\"display\":\"Liss\",\"type\":\"pj\"},{\"raw\":\"Lugar - Rocabrote Rojo\",\"display\":\"Lugar - Rocabrote Rojo\",\"type\":\"unknown\"},{\"raw\":\"PJ - Kaligula\",\"display\":\"Kaligula\",\"type\":\"pj\"},{\"raw\":\"NPC - Mraize\",\"display\":\"Mraize\",\"type\":\"npc\"},{\"raw\":\"NPC - Tet Rebin\",\"display\":\"Tet Rebin\",\"type\":\"npc\"},{\"raw\":\"PJ - Raito\",\"display\":\"Raito\",\"type\":\"pj\"},{\"raw\":\"Lugar - Puente Nueve\",\"display\":\"Lugar - Puente Nueve\",\"type\":\"unknown\"}]",
                    _seed, _seed
                });

            migrationBuilder.InsertData(
                table: "DiaryEntries",
                columns: new[] { "CampaignId", "Number", "Title", "Slug", "Preview", "Body", "Participants", "MentionsJson", "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    1L, 5, "El Puente Nueve", "sesion-05",
                    "El grupo se divide.",
                    "El grupo se divide.\n\n[[PJ - Roca Pequeña]] y [[PJ - Raito]] se infiltran en el [[Lugar - Puente Nueve]].\n\n[[PJ - Kaligula]] acepta una misión de [[NPC - Mraize]] ([[Facción - Sangre Espectral]]).\n\n[[PJ - Hanol]] y [[PJ - Guizmo]] exploran los abismos en busca de parshendi.\n\nDurante el enfrentamiento, la situación se vuelve crítica.\n\nEn un momento desesperado, [[PJ - Hanol]] invoca un muro de piedra gracias a [[Spren - Po'ahu]], permitiendo la huida.\n\nLa sesión termina con el grupo escapando, pero siendo perseguidos.",
                    new[] { "Roca Pequeña", "Raito", "Kaligula", "Hanol", "Guizmo" },
                    "[{\"raw\":\"PJ - Roca Pequeña\",\"display\":\"Roca Pequeña\",\"type\":\"pj\"},{\"raw\":\"PJ - Raito\",\"display\":\"Raito\",\"type\":\"pj\"},{\"raw\":\"Lugar - Puente Nueve\",\"display\":\"Lugar - Puente Nueve\",\"type\":\"unknown\"},{\"raw\":\"PJ - Kaligula\",\"display\":\"Kaligula\",\"type\":\"pj\"},{\"raw\":\"NPC - Mraize\",\"display\":\"Mraize\",\"type\":\"npc\"},{\"raw\":\"Facción - Sangre Espectral\",\"display\":\"Sangre Espectral\",\"type\":\"faction\"},{\"raw\":\"PJ - Hanol\",\"display\":\"Hanol\",\"type\":\"pj\"},{\"raw\":\"PJ - Guizmo\",\"display\":\"Guizmo\",\"type\":\"pj\"},{\"raw\":\"Spren - Po'ahu\",\"display\":\"Po'ahu\",\"type\":\"spren\"}]",
                    _seed, _seed
                });

            migrationBuilder.InsertData(
                table: "DiaryEntries",
                columns: new[] { "CampaignId", "Number", "Title", "Slug", "Preview", "Body", "Participants", "MentionsJson", "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    1L, 6, "Las Ruinas", "sesion-06",
                    "La persecución culmina en las ruinas.",
                    "La persecución culmina en las ruinas.\n\nEl grupo enfrenta un abismoide, enjambres de cremlinos y múltiples peligros.\n\n[[PJ - Kaligula]] explora el interior y resuelve un misterio oculto.\n\n[[PJ - Roca Pequeña]] protege al grupo hasta caer inconsciente.\n\n[[PJ - Raito]] cumple su juramento ayudando a los mercaderes.\n\n[[PJ - Hanol]] y [[PJ - Guizmo]] resisten la persecución y logran reunirse con el grupo.\n\nFinalmente, regresan a los campamentos, donde:\n- Se reportan pérdidas\n- Se entregan prisioneros\n- [[PJ - Kaligula]] recibe una nota misteriosa\n\nLa sesión termina en un punto de aparente calma… antes de lo que vendrá.",
                    new[] { "Kaligula", "Roca Pequeña", "Raito", "Hanol", "Guizmo" },
                    "[{\"raw\":\"PJ - Kaligula\",\"display\":\"Kaligula\",\"type\":\"pj\"},{\"raw\":\"PJ - Roca Pequeña\",\"display\":\"Roca Pequeña\",\"type\":\"pj\"},{\"raw\":\"PJ - Raito\",\"display\":\"Raito\",\"type\":\"pj\"},{\"raw\":\"PJ - Hanol\",\"display\":\"Hanol\",\"type\":\"pj\"},{\"raw\":\"PJ - Guizmo\",\"display\":\"Guizmo\",\"type\":\"pj\"}]",
                    _seed, _seed
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiaryEntries");
        }
    }
}
