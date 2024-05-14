using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class up : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductMaterial_Materials_MaterialId",
                table: "ProductMaterial");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductMaterial_Products_ProductId",
                table: "ProductMaterial");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductSize_Products_ProductId",
                table: "ProductSize");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductSize_Sizes_SizeId",
                table: "ProductSize");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductSize",
                table: "ProductSize");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductMaterial",
                table: "ProductMaterial");

            migrationBuilder.RenameTable(
                name: "ProductSize",
                newName: "ProductSizes");

            migrationBuilder.RenameTable(
                name: "ProductMaterial",
                newName: "ProductMaterials");

            migrationBuilder.RenameIndex(
                name: "IX_ProductSize_SizeId",
                table: "ProductSizes",
                newName: "IX_ProductSizes_SizeId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductMaterial_MaterialId",
                table: "ProductMaterials",
                newName: "IX_ProductMaterials_MaterialId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductSizes",
                table: "ProductSizes",
                columns: new[] { "ProductId", "SizeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductMaterials",
                table: "ProductMaterials",
                columns: new[] { "ProductId", "MaterialId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMaterials_Materials_MaterialId",
                table: "ProductMaterials",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMaterials_Products_ProductId",
                table: "ProductMaterials",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSizes_Products_ProductId",
                table: "ProductSizes",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSizes_Sizes_SizeId",
                table: "ProductSizes",
                column: "SizeId",
                principalTable: "Sizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductMaterials_Materials_MaterialId",
                table: "ProductMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductMaterials_Products_ProductId",
                table: "ProductMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductSizes_Products_ProductId",
                table: "ProductSizes");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductSizes_Sizes_SizeId",
                table: "ProductSizes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductSizes",
                table: "ProductSizes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductMaterials",
                table: "ProductMaterials");

            migrationBuilder.RenameTable(
                name: "ProductSizes",
                newName: "ProductSize");

            migrationBuilder.RenameTable(
                name: "ProductMaterials",
                newName: "ProductMaterial");

            migrationBuilder.RenameIndex(
                name: "IX_ProductSizes_SizeId",
                table: "ProductSize",
                newName: "IX_ProductSize_SizeId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductMaterials_MaterialId",
                table: "ProductMaterial",
                newName: "IX_ProductMaterial_MaterialId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductSize",
                table: "ProductSize",
                columns: new[] { "ProductId", "SizeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductMaterial",
                table: "ProductMaterial",
                columns: new[] { "ProductId", "MaterialId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMaterial_Materials_MaterialId",
                table: "ProductMaterial",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMaterial_Products_ProductId",
                table: "ProductMaterial",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSize_Products_ProductId",
                table: "ProductSize",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSize_Sizes_SizeId",
                table: "ProductSize",
                column: "SizeId",
                principalTable: "Sizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
