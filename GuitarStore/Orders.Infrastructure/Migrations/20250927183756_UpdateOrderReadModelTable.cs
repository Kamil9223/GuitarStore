using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderReadModelTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Delivery_Country",
                schema: "Orders",
                table: "Orders_Read",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Delivery_HouseNumber",
                schema: "Orders",
                table: "Orders_Read",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Delivery_LocalNumber",
                schema: "Orders",
                table: "Orders_Read",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Delivery_LocalityName",
                schema: "Orders",
                table: "Orders_Read",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Delivery_PostalCode",
                schema: "Orders",
                table: "Orders_Read",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Delivery_Street",
                schema: "Orders",
                table: "Orders_Read",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Delivery_Country",
                schema: "Orders",
                table: "Orders_Read");

            migrationBuilder.DropColumn(
                name: "Delivery_HouseNumber",
                schema: "Orders",
                table: "Orders_Read");

            migrationBuilder.DropColumn(
                name: "Delivery_LocalNumber",
                schema: "Orders",
                table: "Orders_Read");

            migrationBuilder.DropColumn(
                name: "Delivery_LocalityName",
                schema: "Orders",
                table: "Orders_Read");

            migrationBuilder.DropColumn(
                name: "Delivery_PostalCode",
                schema: "Orders",
                table: "Orders_Read");

            migrationBuilder.DropColumn(
                name: "Delivery_Street",
                schema: "Orders",
                table: "Orders_Read");
        }
    }
}
