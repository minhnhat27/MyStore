using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class favorite_up : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductFavorite_AspNetUsers_UserId",
                table: "ProductFavorite");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductFavorite_Products_ProductId",
                table: "ProductFavorite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductFavorite",
                table: "ProductFavorite");

            migrationBuilder.RenameTable(
                name: "ProductFavorite",
                newName: "ProductFavorites");

            migrationBuilder.RenameIndex(
                name: "IX_ProductFavorite_ProductId",
                table: "ProductFavorites",
                newName: "IX_ProductFavorites_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductFavorites",
                table: "ProductFavorites",
                columns: new[] { "UserId", "ProductId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductFavorites_AspNetUsers_UserId",
                table: "ProductFavorites",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductFavorites_Products_ProductId",
                table: "ProductFavorites",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductFavorites_AspNetUsers_UserId",
                table: "ProductFavorites");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductFavorites_Products_ProductId",
                table: "ProductFavorites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductFavorites",
                table: "ProductFavorites");

            migrationBuilder.RenameTable(
                name: "ProductFavorites",
                newName: "ProductFavorite");

            migrationBuilder.RenameIndex(
                name: "IX_ProductFavorites_ProductId",
                table: "ProductFavorite",
                newName: "IX_ProductFavorite_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductFavorite",
                table: "ProductFavorite",
                columns: new[] { "UserId", "ProductId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductFavorite_AspNetUsers_UserId",
                table: "ProductFavorite",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductFavorite_Products_ProductId",
                table: "ProductFavorite",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
