using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAM.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "seller_profiles",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "seller_profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "reviews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SellerRepliedAt",
                table: "reviews",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerReply",
                table: "reviews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "products",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "seller_profiles");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "seller_profiles");

            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "SellerRepliedAt",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "SellerReply",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "products");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "products");
        }
    }
}
