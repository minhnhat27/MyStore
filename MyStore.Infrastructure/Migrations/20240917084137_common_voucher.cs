using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class common_voucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserVoucher_AspNetUsers_UserId",
                table: "UserVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVoucher_Voucher_VoucherCode",
                table: "UserVoucher");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Voucher",
                table: "Voucher");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserVoucher",
                table: "UserVoucher");

            migrationBuilder.RenameTable(
                name: "Voucher",
                newName: "Vouchers");

            migrationBuilder.RenameTable(
                name: "UserVoucher",
                newName: "UserVouchers");

            migrationBuilder.RenameIndex(
                name: "IX_UserVoucher_VoucherCode",
                table: "UserVouchers",
                newName: "IX_UserVouchers_VoucherCode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vouchers",
                table: "Vouchers",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserVouchers",
                table: "UserVouchers",
                columns: new[] { "UserId", "VoucherCode" });

            migrationBuilder.CreateTable(
                name: "CommonVouchers",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DiscountPercent = table.Column<int>(type: "integer", nullable: true),
                    DiscountAmount = table.Column<double>(type: "double precision", nullable: true),
                    MinOrder = table.Column<double>(type: "double precision", nullable: false),
                    MaxDiscount = table.Column<double>(type: "double precision", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommonVouchers", x => x.Code);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_UserVouchers_AspNetUsers_UserId",
                table: "UserVouchers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVouchers_Vouchers_VoucherCode",
                table: "UserVouchers",
                column: "VoucherCode",
                principalTable: "Vouchers",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserVouchers_AspNetUsers_UserId",
                table: "UserVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVouchers_Vouchers_VoucherCode",
                table: "UserVouchers");

            migrationBuilder.DropTable(
                name: "CommonVouchers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vouchers",
                table: "Vouchers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserVouchers",
                table: "UserVouchers");

            migrationBuilder.RenameTable(
                name: "Vouchers",
                newName: "Voucher");

            migrationBuilder.RenameTable(
                name: "UserVouchers",
                newName: "UserVoucher");

            migrationBuilder.RenameIndex(
                name: "IX_UserVouchers_VoucherCode",
                table: "UserVoucher",
                newName: "IX_UserVoucher_VoucherCode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Voucher",
                table: "Voucher",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserVoucher",
                table: "UserVoucher",
                columns: new[] { "UserId", "VoucherCode" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserVoucher_AspNetUsers_UserId",
                table: "UserVoucher",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVoucher_Voucher_VoucherCode",
                table: "UserVoucher",
                column: "VoucherCode",
                principalTable: "Voucher",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
