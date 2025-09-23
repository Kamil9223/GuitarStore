using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderItems_Read",
                schema: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems_Read", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders_Read",
                schema: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemsCount = table.Column<int>(type: "int", nullable: false),
                    Deliverer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders_Read", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_Read_OrderId",
                schema: "Orders",
                table: "OrderItems_Read",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Read_CustomerId",
                schema: "Orders",
                table: "Orders_Read",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Read_Status_CreatedAt",
                schema: "Orders",
                table: "Orders_Read",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems_Read",
                schema: "Orders");

            migrationBuilder.DropTable(
                name: "Orders_Read",
                schema: "Orders");
        }
    }
}
