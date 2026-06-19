using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameNotCrazy.API.Migrations
{
    public partial class AddPaymentRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payment_requests",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id    = table.Column<long>(type: "bigint", nullable: false),
                    code       = table.Column<string>(type: "varchar(20)", nullable: false)
                                     .Annotation("MySql:CharSet", "utf8mb4"),
                    gold_amount = table.Column<int>(type: "int", nullable: false),
                    price      = table.Column<int>(type: "int", nullable: false),
                    status     = table.Column<string>(type: "varchar(20)", nullable: false, defaultValue: "pending")
                                     .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at  = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    approved_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    note       = table.Column<string>(type: "varchar(255)", nullable: true)
                                     .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_requests", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_payment_requests_code",
                table: "payment_requests",
                column: "code",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "payment_requests");
        }
    }
}
