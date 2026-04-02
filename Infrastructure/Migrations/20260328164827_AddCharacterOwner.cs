using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OwnerId",
                table: "Characters",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Characters_OwnerId",
                table: "Characters",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Users_OwnerId",
                table: "Characters",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Users_OwnerId",
                table: "Characters");

            migrationBuilder.DropIndex(
                name: "IX_Characters_OwnerId",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Characters");
        }
    }
}
