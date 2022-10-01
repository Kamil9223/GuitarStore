using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    public partial class ChangeWarehouseModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcousticGuitars",
                schema: "Warehouse");

            migrationBuilder.DropTable(
                name: "ElectricGuitarPickup",
                schema: "Warehouse");

            migrationBuilder.DropTable(
                name: "ElectricGuitars",
                schema: "Warehouse");

            migrationBuilder.DropTable(
                name: "Pickups",
                schema: "Warehouse");

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalSchema: "Warehouse",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ProducerName = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuitarStoreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Warehouse",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Products_Stores_GuitarStoreId",
                        column: x => x.GuitarStoreId,
                        principalSchema: "Warehouse",
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                schema: "Warehouse",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                schema: "Warehouse",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_GuitarStoreId",
                schema: "Warehouse",
                table: "Products",
                column: "GuitarStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProducerName",
                schema: "Warehouse",
                table: "Products",
                column: "ProducerName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products",
                schema: "Warehouse");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "Warehouse");

            migrationBuilder.CreateTable(
                name: "AcousticGuitars",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuitarStoreId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcousticGuitars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcousticGuitars_Stores_GuitarStoreId",
                        column: x => x.GuitarStoreId,
                        principalSchema: "Warehouse",
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ElectricGuitars",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuitarStoreId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectricGuitars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectricGuitars_Stores_GuitarStoreId",
                        column: x => x.GuitarStoreId,
                        principalSchema: "Warehouse",
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pickups",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PickupType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pickups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElectricGuitarPickup",
                schema: "Warehouse",
                columns: table => new
                {
                    ElectricGuitarsId = table.Column<int>(type: "int", nullable: false),
                    PickupsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectricGuitarPickup", x => new { x.ElectricGuitarsId, x.PickupsId });
                    table.ForeignKey(
                        name: "FK_ElectricGuitarPickup_ElectricGuitars_ElectricGuitarsId",
                        column: x => x.ElectricGuitarsId,
                        principalSchema: "Warehouse",
                        principalTable: "ElectricGuitars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ElectricGuitarPickup_Pickups_PickupsId",
                        column: x => x.PickupsId,
                        principalSchema: "Warehouse",
                        principalTable: "Pickups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcousticGuitars_GuitarStoreId",
                schema: "Warehouse",
                table: "AcousticGuitars",
                column: "GuitarStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectricGuitarPickup_PickupsId",
                schema: "Warehouse",
                table: "ElectricGuitarPickup",
                column: "PickupsId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectricGuitars_GuitarStoreId",
                schema: "Warehouse",
                table: "ElectricGuitars",
                column: "GuitarStoreId");
        }
    }
}
