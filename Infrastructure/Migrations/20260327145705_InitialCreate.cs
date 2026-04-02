using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArmorCatalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ArmorTypeId = table.Column<int>(type: "integer", nullable: false),
                    Desvio = table.Column<int>(type: "integer", nullable: false),
                    TraitIds = table.Column<List<int>>(type: "integer[]", nullable: false),
                    ExpertTraitIds = table.Column<List<int>>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArmorCatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GearItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Weight = table.Column<double>(type: "double precision", nullable: false),
                    Price = table.Column<double>(type: "double precision", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GearItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeaponCatalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    WeaponTypeId = table.Column<int>(type: "integer", nullable: false),
                    SkillId = table.Column<int>(type: "integer", nullable: false),
                    DamageDiceCount = table.Column<int>(type: "integer", nullable: false),
                    DamageDiceValue = table.Column<int>(type: "integer", nullable: false),
                    DamageTypeId = table.Column<int>(type: "integer", nullable: false),
                    RangeId = table.Column<int>(type: "integer", nullable: false),
                    TraitIds = table.Column<List<int>>(type: "integer[]", nullable: false),
                    ExpertTraitIds = table.Column<List<int>>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeaponCatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    GmUserId = table.Column<long>(type: "bigint", nullable: false),
                    InviteCode = table.Column<string>(type: "text", nullable: false),
                    InviteActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campaigns_Users_GmUserId",
                        column: x => x.GmUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignMembers",
                columns: table => new
                {
                    CampaignId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignMembers", x => new { x.CampaignId, x.UserId });
                    table.ForeignKey(
                        name: "FK_CampaignMembers_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<long>(type: "bigint", nullable: false),
                    IsNpc = table.Column<bool>(type: "boolean", nullable: false),
                    IsVisibleToPlayers = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PlayerName = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Experience = table.Column<int>(type: "integer", nullable: false),
                    CaminoHeroico = table.Column<string>(type: "text", nullable: false),
                    CaminoRadiante = table.Column<string>(type: "text", nullable: false),
                    Ascendencia = table.Column<string>(type: "text", nullable: false),
                    Fuerza = table.Column<int>(type: "integer", nullable: false),
                    Velocidad = table.Column<int>(type: "integer", nullable: false),
                    Intelecto = table.Column<int>(type: "integer", nullable: false),
                    Voluntad = table.Column<int>(type: "integer", nullable: false),
                    Discernimiento = table.Column<int>(type: "integer", nullable: false),
                    Presencia = table.Column<int>(type: "integer", nullable: false),
                    Health = table.Column<int>(type: "integer", nullable: false),
                    MaxHealth = table.Column<int>(type: "integer", nullable: false),
                    Mana = table.Column<int>(type: "integer", nullable: false),
                    MaxMana = table.Column<int>(type: "integer", nullable: false),
                    Concentration = table.Column<int>(type: "integer", nullable: false),
                    Investiture = table.Column<int>(type: "integer", nullable: false),
                    Desvio = table.Column<int>(type: "integer", nullable: false),
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
                    HabilidadPersonalizada1 = table.Column<string>(type: "text", nullable: false),
                    HabilidadPersonalizada1Valor = table.Column<int>(type: "integer", nullable: false),
                    HabilidadPersonalizada2 = table.Column<string>(type: "text", nullable: false),
                    HabilidadPersonalizada2Valor = table.Column<int>(type: "integer", nullable: false),
                    HabilidadPersonalizada3 = table.Column<string>(type: "text", nullable: false),
                    HabilidadPersonalizada3Valor = table.Column<int>(type: "integer", nullable: false),
                    Proposito = table.Column<string>(type: "text", nullable: false),
                    Obstaculo = table.Column<string>(type: "text", nullable: false),
                    Metas = table.Column<string>(type: "text", nullable: false),
                    Talentos = table.Column<string>(type: "text", nullable: false),
                    Apariencia = table.Column<string>(type: "text", nullable: false),
                    Notas = table.Column<string>(type: "text", nullable: false),
                    Conexiones = table.Column<string>(type: "text", nullable: false),
                    Weapons = table.Column<List<string>>(type: "text[]", nullable: false),
                    Armor = table.Column<List<string>>(type: "text[]", nullable: false),
                    Spells = table.Column<List<string>>(type: "text[]", nullable: false),
                    Equipment = table.Column<List<string>>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<long>(type: "bigint", nullable: false),
                    Resolution = table.Column<string>(type: "text", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "SideQuests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Acts = table.Column<List<string>>(type: "text[]", nullable: false),
                    Rewards = table.Column<List<string>>(type: "text[]", nullable: false),
                    Benefits = table.Column<List<string>>(type: "text[]", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Started = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SideQuests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SideQuests_Campaigns_CampaignId",
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
                name: "IX_CampaignMembers_UserId",
                table: "CampaignMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_GmUserId",
                table: "Campaigns",
                column: "GmUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_InviteCode",
                table: "Campaigns",
                column: "InviteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogOptions_Category",
                table: "CatalogOptions",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_CampaignId",
                table: "Characters",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_CampaignId",
                table: "Matches",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenes_MatchId",
                table: "Scenes",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SideQuests_CampaignId",
                table: "SideQuests",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArmorCatalog");

            migrationBuilder.DropTable(
                name: "CampaignMembers");

            migrationBuilder.DropTable(
                name: "CatalogOptions");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "GearItems");

            migrationBuilder.DropTable(
                name: "Scenes");

            migrationBuilder.DropTable(
                name: "SideQuests");

            migrationBuilder.DropTable(
                name: "WeaponCatalog");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
