using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MySweetShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQrAndLoyalty2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCashier",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCashier",
                table: "Users");
        }
    }
}
