using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addflashSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FlashSaleId",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FlashSale",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashSale", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_FlashSaleId",
                table: "Products",
                column: "FlashSaleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_FlashSale_FlashSaleId",
                table: "Products",
                column: "FlashSaleId",
                principalTable: "FlashSale",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_FlashSale_FlashSaleId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "FlashSale");

            migrationBuilder.DropIndex(
                name: "IX_Products_FlashSaleId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FlashSaleId",
                table: "Products");
        }
    }
}
