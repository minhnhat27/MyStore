using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changeToFlashSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCampaigns_Campaigns_CampaignId",
                table: "ProductCampaigns");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "TotalRevenue",
                table: "Campaigns");

            migrationBuilder.RenameColumn(
                name: "CampaignId",
                table: "ProductCampaigns",
                newName: "FlashSaleId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductCampaigns_CampaignId",
                table: "ProductCampaigns",
                newName: "IX_ProductCampaigns_FlashSaleId");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Campaigns",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Sold",
                table: "Campaigns",
                newName: "DiscountTimeFrame");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCampaigns_Campaigns_FlashSaleId",
                table: "ProductCampaigns",
                column: "FlashSaleId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCampaigns_Campaigns_FlashSaleId",
                table: "ProductCampaigns");

            migrationBuilder.RenameColumn(
                name: "FlashSaleId",
                table: "ProductCampaigns",
                newName: "CampaignId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductCampaigns_FlashSaleId",
                table: "ProductCampaigns",
                newName: "IX_ProductCampaigns_CampaignId");

            migrationBuilder.RenameColumn(
                name: "DiscountTimeFrame",
                table: "Campaigns",
                newName: "Sold");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Campaigns",
                newName: "StartDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Campaigns",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Campaigns",
                type: "character varying(55)",
                maxLength: 55,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "TotalRevenue",
                table: "Campaigns",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCampaigns_Campaigns_CampaignId",
                table: "ProductCampaigns",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
