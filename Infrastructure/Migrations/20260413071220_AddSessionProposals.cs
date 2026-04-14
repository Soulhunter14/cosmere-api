using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionProposals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionProposals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PromotedSessionId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionProposals_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionProposals_Sessions_PromotedSessionId",
                        column: x => x.PromotedSessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProposalDates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProposalId = table.Column<long>(type: "bigint", nullable: false),
                    ProposedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposalDates_SessionProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "SessionProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProposalVotes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProposalDateId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CanAttend = table.Column<bool>(type: "boolean", nullable: false),
                    VotedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposalVotes_ProposalDates_ProposalDateId",
                        column: x => x.ProposalDateId,
                        principalTable: "ProposalDates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProposalVotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProposalDates_ProposalId",
                table: "ProposalDates",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalVotes_ProposalDateId_UserId",
                table: "ProposalVotes",
                columns: new[] { "ProposalDateId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProposalVotes_UserId",
                table: "ProposalVotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionProposals_CampaignId",
                table: "SessionProposals",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionProposals_PromotedSessionId",
                table: "SessionProposals",
                column: "PromotedSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProposalVotes");

            migrationBuilder.DropTable(
                name: "ProposalDates");

            migrationBuilder.DropTable(
                name: "SessionProposals");
        }
    }
}
