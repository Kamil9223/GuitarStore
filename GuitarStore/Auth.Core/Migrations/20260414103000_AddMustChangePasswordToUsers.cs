using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddMustChangePasswordToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                schema: "Auth",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                schema: "Auth",
                table: "AspNetUsers");
        }
    }
}
