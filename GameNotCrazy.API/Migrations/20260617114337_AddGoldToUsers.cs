using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameNotCrazy.API.Migrations
{
    /// <inheritdoc />
    public partial class AddGoldToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "gold",
                table: "users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "gold",
                table: "users");
        }
    }
}
