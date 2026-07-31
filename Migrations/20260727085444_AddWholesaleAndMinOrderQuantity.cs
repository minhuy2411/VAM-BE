using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAM.Migrations
{
    /// <inheritdoc />
    public partial class AddWholesaleAndMinOrderQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWholesale",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MinOrderQuantity",
                table: "products",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWholesale",
                table: "products");

            migrationBuilder.DropColumn(
                name: "MinOrderQuantity",
                table: "products");
        }
    }
}
