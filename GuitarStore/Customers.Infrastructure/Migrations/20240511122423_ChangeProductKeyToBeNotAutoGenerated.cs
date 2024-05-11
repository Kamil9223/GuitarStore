﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Customers.Infrastructure.Migrations
{
    public partial class ChangeProductKeyToBeNotAutoGenerated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                schema: "Customers",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "Customers",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "Customers",
                table: "Products",
                type: "int",
                nullable: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                schema: "Customers",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
               name: "PK_Products",
               schema: "Customers",
               table: "Products");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "Customers",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "Customers",
                table: "Products",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                schema: "Customers",
                column: "Id");
        }
    }
}
