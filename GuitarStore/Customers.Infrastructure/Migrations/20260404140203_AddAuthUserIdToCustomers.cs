using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Customers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthUserIdToCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AuthUserId",
                schema: "Customers",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("UPDATE [Customers].[Customers] SET [AuthUserId] = NEWID() WHERE [AuthUserId] IS NULL");

            migrationBuilder.AlterColumn<Guid>(
                name: "AuthUserId",
                schema: "Customers",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AuthUserId",
                schema: "Customers",
                table: "Customers",
                column: "AuthUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_AuthUserId",
                schema: "Customers",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AuthUserId",
                schema: "Customers",
                table: "Customers");
        }
    }
}
