using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignWorld : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Era",
                table: "Campaigns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "World",
                table: "Campaigns",
                type: "text",
                nullable: false,
                defaultValue: "stormlight");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Era",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "World",
                table: "Campaigns");
        }
    }
}
