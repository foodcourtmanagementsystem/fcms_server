using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    public partial class Update11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAddress_AspNetUsers_UserId",
                table: "UserAddress");

            migrationBuilder.DropIndex(
                name: "IX_UserAddress_UserId",
                table: "UserAddress");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserAddress",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddress_UserId",
                table: "UserAddress",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAddress_AspNetUsers_UserId",
                table: "UserAddress",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAddress_AspNetUsers_UserId",
                table: "UserAddress");

            migrationBuilder.DropIndex(
                name: "IX_UserAddress_UserId",
                table: "UserAddress");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserAddress",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAddress_UserId",
                table: "UserAddress",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAddress_AspNetUsers_UserId",
                table: "UserAddress",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
