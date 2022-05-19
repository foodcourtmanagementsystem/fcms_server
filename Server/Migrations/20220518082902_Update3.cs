using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    public partial class Update3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItem");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Cart");

            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "Order",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<long>(
                name: "FoodItemId",
                table: "Cart",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Cart",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Cart_FoodItemId",
                table: "Cart",
                column: "FoodItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cart_FoodItem_FoodItemId",
                table: "Cart",
                column: "FoodItemId",
                principalTable: "FoodItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cart_FoodItem_FoodItemId",
                table: "Cart");

            migrationBuilder.DropIndex(
                name: "IX_Cart_FoodItemId",
                table: "Cart");

            migrationBuilder.DropColumn(
                name: "FoodItemId",
                table: "Cart");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Cart");

            migrationBuilder.AlterColumn<float>(
                name: "Amount",
                table: "Order",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "Cart",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CartItem",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartId = table.Column<long>(type: "bigint", nullable: false),
                    FoodItemId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItem_Cart_CartId",
                        column: x => x.CartId,
                        principalTable: "Cart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItem_FoodItem_FoodItemId",
                        column: x => x.FoodItemId,
                        principalTable: "FoodItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_CartId",
                table: "CartItem",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_FoodItemId",
                table: "CartItem",
                column: "FoodItemId");
        }
    }
}
