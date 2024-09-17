using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_voucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Voucher",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    DiscountPercent = table.Column<int>(type: "integer", nullable: true),
                    DiscountAmount = table.Column<double>(type: "double precision", nullable: true),
                    MinOrder = table.Column<double>(type: "double precision", nullable: false),
                    MaxDiscount = table.Column<double>(type: "double precision", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voucher", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "UserVoucher",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    VoucherCode = table.Column<string>(type: "text", nullable: false),
                    Used = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVoucher", x => new { x.UserId, x.VoucherCode });
                    table.ForeignKey(
                        name: "FK_UserVoucher_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVoucher_Voucher_VoucherCode",
                        column: x => x.VoucherCode,
                        principalTable: "Voucher",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserVoucher_VoucherCode",
                table: "UserVoucher",
                column: "VoucherCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserVoucher");

            migrationBuilder.DropTable(
                name: "Voucher");
        }
    }
}
