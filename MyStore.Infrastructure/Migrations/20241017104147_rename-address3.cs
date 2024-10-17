using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class renameaddress3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DistrictId",
                table: "Orders",
                newName: "DistrictID");

            migrationBuilder.RenameColumn(
                name: "WardCode",
                table: "Orders",
                newName: "WardID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DistrictID",
                table: "Orders",
                newName: "DistrictId");

            migrationBuilder.RenameColumn(
                name: "WardID",
                table: "Orders",
                newName: "WardCode");
        }
    }
}
