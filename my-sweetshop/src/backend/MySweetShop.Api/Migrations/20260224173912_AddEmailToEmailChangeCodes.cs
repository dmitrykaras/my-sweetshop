using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MySweetShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailToEmailChangeCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "EmailChangeCodes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "EmailChangeCodes");
        }
    }
}
