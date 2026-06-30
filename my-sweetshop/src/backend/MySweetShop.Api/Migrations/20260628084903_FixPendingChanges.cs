using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MySweetShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111111"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111112"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111113"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111114"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111115"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111116"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111117"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111118"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111119"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111120"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111121"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111122"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111123"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111124"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111125"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111126"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-1111-1111-8888-111111111127"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1111-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1112-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1113-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1114-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1115-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1116-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1117-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1118-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1119-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1120-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1121-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1122-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1123-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1124-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1125-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1126-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1127-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaa1128-aaaa-1111-aaaa-aaaaaaaaaaaa"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbb1111-bbbb-1111-bbbb-bbbbbbbbbbbb"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbb1112-bbbb-1111-bbbb-bbbbbbbbbbbb"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("cccc1111-cccc-1111-cccc-cccccccccccc"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("cccc1112-cccc-1111-cccc-cccccccccccc"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("dddd1111-dddd-1111-dddd-dddddddddddd"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("dddd1112-dddd-1111-dddd-dddddddddddd"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("eeee1111-eeee-1111-eeee-eeeeeeeeeeee"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("eeee1112-eeee-1111-eeee-eeeeeeeeeeee"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("eeee1113-eeee-1111-eeee-eeeeeeeeeeee"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ffff1111-ffff-1111-ffff-ffffffffffff"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ffff1112-ffff-1111-ffff-ffffffffffff"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ffff1113-ffff-1111-ffff-ffffffffffff"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ffff1114-ffff-1111-ffff-ffffffffffff"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ffff1115-ffff-1111-ffff-ffffffffffff"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ffff1116-ffff-1111-ffff-ffffffffffff"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ffff1117-ffff-1111-ffff-ffffffffffff"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ffff1118-ffff-1111-ffff-ffffffffffff"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ffff1119-ffff-1111-ffff-ffffffffffff"),
                column: "IsFavorite",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ffff1120-ffff-1111-ffff-ffffffffffff"),
                column: "IsFavorite",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "Products");
        }
    }
}
