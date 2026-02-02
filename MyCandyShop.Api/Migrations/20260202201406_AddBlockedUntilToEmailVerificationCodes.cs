using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCandyShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockedUntilToEmailVerificationCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BlockedUntil",
                table: "EmailVerificationCodes",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockedUntil",
                table: "EmailVerificationCodes");
        }
    }
}
