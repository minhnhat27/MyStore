using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class renameaddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Ward_name",
                table: "DeliveryAddresses",
                newName: "WardName");

            migrationBuilder.RenameColumn(
                name: "Ward_id",
                table: "DeliveryAddresses",
                newName: "WardId");

            migrationBuilder.RenameColumn(
                name: "Province_name",
                table: "DeliveryAddresses",
                newName: "ProvinceName");

            migrationBuilder.RenameColumn(
                name: "Province_id",
                table: "DeliveryAddresses",
                newName: "ProvinceId");

            migrationBuilder.RenameColumn(
                name: "District_name",
                table: "DeliveryAddresses",
                newName: "DistrictName");

            migrationBuilder.RenameColumn(
                name: "District_id",
                table: "DeliveryAddresses",
                newName: "DistrictId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WardName",
                table: "DeliveryAddresses",
                newName: "Ward_name");

            migrationBuilder.RenameColumn(
                name: "WardId",
                table: "DeliveryAddresses",
                newName: "Ward_id");

            migrationBuilder.RenameColumn(
                name: "ProvinceName",
                table: "DeliveryAddresses",
                newName: "Province_name");

            migrationBuilder.RenameColumn(
                name: "ProvinceId",
                table: "DeliveryAddresses",
                newName: "Province_id");

            migrationBuilder.RenameColumn(
                name: "DistrictName",
                table: "DeliveryAddresses",
                newName: "District_name");

            migrationBuilder.RenameColumn(
                name: "DistrictId",
                table: "DeliveryAddresses",
                newName: "District_id");
        }
    }
}
