using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    public partial class UpdateSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Catalog");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "Warehouse",
                newName: "Products",
                newSchema: "Catalog");

            migrationBuilder.RenameTable(
                name: "Categories",
                schema: "Warehouse",
                newName: "Categories",
                newSchema: "Catalog");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Warehouse");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "Catalog",
                newName: "Products",
                newSchema: "Warehouse");

            migrationBuilder.RenameTable(
                name: "Categories",
                schema: "Catalog",
                newName: "Categories",
                newSchema: "Warehouse");
        }
    }
}
