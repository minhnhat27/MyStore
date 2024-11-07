using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class renameFlashSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCampaigns_Campaigns_FlashSaleId",
                table: "ProductCampaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCampaigns_Products_ProductId",
                table: "ProductCampaigns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductCampaigns",
                table: "ProductCampaigns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Campaigns",
                table: "Campaigns");

            migrationBuilder.RenameTable(
                name: "ProductCampaigns",
                newName: "ProductFlashSales");

            migrationBuilder.RenameTable(
                name: "Campaigns",
                newName: "FlashSales");

            migrationBuilder.RenameIndex(
                name: "IX_ProductCampaigns_FlashSaleId",
                table: "ProductFlashSales",
                newName: "IX_ProductFlashSales_FlashSaleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductFlashSales",
                table: "ProductFlashSales",
                columns: new[] { "ProductId", "FlashSaleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_FlashSales",
                table: "FlashSales",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductFlashSales_FlashSales_FlashSaleId",
                table: "ProductFlashSales",
                column: "FlashSaleId",
                principalTable: "FlashSales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductFlashSales_Products_ProductId",
                table: "ProductFlashSales",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductFlashSales_FlashSales_FlashSaleId",
                table: "ProductFlashSales");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductFlashSales_Products_ProductId",
                table: "ProductFlashSales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductFlashSales",
                table: "ProductFlashSales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FlashSales",
                table: "FlashSales");

            migrationBuilder.RenameTable(
                name: "ProductFlashSales",
                newName: "ProductCampaigns");

            migrationBuilder.RenameTable(
                name: "FlashSales",
                newName: "Campaigns");

            migrationBuilder.RenameIndex(
                name: "IX_ProductFlashSales_FlashSaleId",
                table: "ProductCampaigns",
                newName: "IX_ProductCampaigns_FlashSaleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductCampaigns",
                table: "ProductCampaigns",
                columns: new[] { "ProductId", "FlashSaleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Campaigns",
                table: "Campaigns",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCampaigns_Campaigns_FlashSaleId",
                table: "ProductCampaigns",
                column: "FlashSaleId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCampaigns_Products_ProductId",
                table: "ProductCampaigns",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
