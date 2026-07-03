using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAM.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "seller_profiles",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FarmId",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "business_profiles",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_FarmId",
                table: "products",
                column: "FarmId");

            migrationBuilder.AddForeignKey(
                name: "FK_products_farms_FarmId",
                table: "products",
                column: "FarmId",
                principalTable: "farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_farms_FarmId",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_FarmId",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "seller_profiles");

            migrationBuilder.DropColumn(
                name: "FarmId",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "business_profiles");
        }
    }
}
