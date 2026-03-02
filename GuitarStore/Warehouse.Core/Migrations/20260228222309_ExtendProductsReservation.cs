using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Core.Migrations
{
    /// <inheritdoc />
    public partial class ExtendProductsReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "Warehouse",
                table: "ProductReservations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAtUtc",
                schema: "Warehouse",
                table: "ProductReservations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                schema: "Warehouse",
                table: "ProductReservations",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "Warehouse",
                table: "ProductReservations");

            migrationBuilder.DropColumn(
                name: "ExpiresAtUtc",
                schema: "Warehouse",
                table: "ProductReservations");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Warehouse",
                table: "ProductReservations");
        }
    }
}
