using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalDateSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SessionId",
                table: "ProposalDates",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slot",
                table: "ProposalDates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ProposalDates",
                type: "text",
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalDates_SessionId",
                table: "ProposalDates",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProposalDates_Sessions_SessionId",
                table: "ProposalDates",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Backfill proposals already resolved: the promoted date points at its session, every other date is rejected.
            migrationBuilder.Sql("""
                UPDATE "ProposalDates" AS d
                SET "Status" = 'Accepted', "SessionId" = p."PromotedSessionId"
                FROM "SessionProposals" AS p
                JOIN "Sessions" AS s ON s."Id" = p."PromotedSessionId"
                WHERE d."ProposalId" = p."Id" AND p."Status" = 'Promoted' AND d."ProposedDate" = s."Date";
                """);
            migrationBuilder.Sql("""
                UPDATE "ProposalDates" AS d
                SET "Status" = 'Rejected'
                FROM "SessionProposals" AS p
                WHERE d."ProposalId" = p."Id" AND p."Status" <> 'Pending' AND d."Status" = 'Pending';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProposalDates_Sessions_SessionId",
                table: "ProposalDates");

            migrationBuilder.DropIndex(
                name: "IX_ProposalDates_SessionId",
                table: "ProposalDates");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "ProposalDates");

            migrationBuilder.DropColumn(
                name: "Slot",
                table: "ProposalDates");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ProposalDates");
        }
    }
}
