using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameNotCrazy.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsNoAds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_no_ads",
                table: "users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_no_ads",
                table: "users");
        }
    }
}
