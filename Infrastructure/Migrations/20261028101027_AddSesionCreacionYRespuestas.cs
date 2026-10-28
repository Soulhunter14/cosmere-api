using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSesionCreacionYRespuestas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SesionCreacionEstado",
                table: "Campaigns",
                type: "text",
                nullable: false,
                defaultValue: "pendiente");

            migrationBuilder.AddColumn<string>(
                name: "SesionCreacionGuion",
                table: "Campaigns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SesionCreacionIniciadaEn",
                table: "Campaigns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SesionCreacionMesaId",
                table: "Campaigns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SesionCreacionPreparadaEn",
                table: "Campaigns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SesionCreacionTerminadaEn",
                table: "Campaigns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CharacterRespuestas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    Clave = table.Column<string>(type: "text", nullable: false),
                    TipoPregunta = table.Column<string>(type: "text", nullable: false),
                    Pregunta = table.Column<string>(type: "text", nullable: false),
                    TipoRespuesta = table.Column<string>(type: "text", nullable: false),
                    Respuesta = table.Column<string>(type: "text", nullable: false),
                    Detalle = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                    Origen = table.Column<string>(type: "text", nullable: false),
                    SesionId = table.Column<string>(type: "text", nullable: false),
                    SesionTitulo = table.Column<string>(type: "text", nullable: false),
                    RespondidaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AplicadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterRespuestas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterRespuestas_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterRespuestas_CharacterId_Clave",
                table: "CharacterRespuestas",
                columns: new[] { "CharacterId", "Clave" },
                unique: true);

            // Lo ya respondido en los pasos que no son campos de la ficha (CreacionDatos: alomante, alomancia) entra en el registro con la
            // pregunta que vio el jugador; las filas de los demás pasos nacen a partir de ahora, desde el PUT del personaje. Solo inserta
            // (aditiva) y no toca un JSON que no sea un objeto válido (pg_input_is_valid, PostgreSQL 16).
            migrationBuilder.Sql("""
                INSERT INTO "CharacterRespuestas" ("CharacterId", "Clave", "TipoPregunta", "Pregunta", "TipoRespuesta", "Respuesta", "Detalle", "Origen", "SesionId", "SesionTitulo", "RespondidaEn", "CreatedAt")
                SELECT c."Id", 'creacion:' || d.key, 'paso',
                       CASE d.key WHEN 'alomante' THEN '¿Quieres ser alomante?' WHEN 'alomancia' THEN '¿Qué tipo de brumoso?' ELSE 'creacion:' || d.key END,
                       CASE WHEN d.value IN ('si', 'no') THEN 'si-no' ELSE 'eleccion' END,
                       d.value, '{}', 'creacion', '', '', c."UpdatedAt", now()
                FROM "Characters" c
                CROSS JOIN LATERAL jsonb_each_text(
                    CASE WHEN c."CreacionDatos" IS NOT NULL AND pg_input_is_valid(c."CreacionDatos", 'jsonb')
                         THEN CASE WHEN jsonb_typeof(c."CreacionDatos"::jsonb) = 'object' THEN c."CreacionDatos"::jsonb ELSE '{}'::jsonb END
                         ELSE '{}'::jsonb END) AS d
                WHERE d.value <> ''
                ON CONFLICT ("CharacterId", "Clave") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterRespuestas");

            migrationBuilder.DropColumn(
                name: "SesionCreacionEstado",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "SesionCreacionGuion",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "SesionCreacionIniciadaEn",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "SesionCreacionMesaId",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "SesionCreacionPreparadaEn",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "SesionCreacionTerminadaEn",
                table: "Campaigns");
        }
    }
}
