using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addorderVariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorName",
                table: "OrderDetails");

            migrationBuilder.RenameColumn(
                name: "SizeName",
                table: "OrderDetails",
                newName: "Variant");

            migrationBuilder.AddColumn<string>(
                name: "Variant",
                table: "ProductReviews",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Variant",
                table: "ProductReviews");

            migrationBuilder.RenameColumn(
                name: "Variant",
                table: "OrderDetails",
                newName: "SizeName");

            migrationBuilder.AddColumn<string>(
                name: "ColorName",
                table: "OrderDetails",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
