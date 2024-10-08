using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class upproductreview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Review",
                table: "ProductReviews");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ProductReviews",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string[]>(
                name: "ImagesUrls",
                table: "ProductReviews",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "ImagesUrls",
                table: "ProductReviews");

            migrationBuilder.AddColumn<string>(
                name: "Review",
                table: "ProductReviews",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
