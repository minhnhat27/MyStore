using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class renameaddress2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WardId",
                table: "DeliveryAddresses",
                newName: "WardID");

            migrationBuilder.RenameColumn(
                name: "ProvinceId",
                table: "DeliveryAddresses",
                newName: "ProvinceID");

            migrationBuilder.RenameColumn(
                name: "DistrictId",
                table: "DeliveryAddresses",
                newName: "DistrictID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WardID",
                table: "DeliveryAddresses",
                newName: "WardId");

            migrationBuilder.RenameColumn(
                name: "ProvinceID",
                table: "DeliveryAddresses",
                newName: "ProvinceId");

            migrationBuilder.RenameColumn(
                name: "DistrictID",
                table: "DeliveryAddresses",
                newName: "DistrictId");
        }
    }
}
