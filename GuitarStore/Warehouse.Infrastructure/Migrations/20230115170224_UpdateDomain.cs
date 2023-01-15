using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    public partial class UpdateDomain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModelName",
                schema: "Warehouse",
                table: "Products",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "Warehouse",
                table: "Products",
                type: "Char(3)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "Warehouse",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "Warehouse",
                table: "Products",
                newName: "ModelName");
        }
    }
}
