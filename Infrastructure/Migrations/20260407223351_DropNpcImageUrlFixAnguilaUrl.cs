using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropNpcImageUrlFixAnguilaUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Characters");

            migrationBuilder.Sql(
                "UPDATE \"GlobalNpcs\" SET \"ImageUrl\" = '/npc-images/anguila-aerea.png' WHERE \"Name\" = 'Anguila Aérea'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Characters",
                type: "text",
                nullable: true);
        }
    }
}
