using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stock_Warehouse_Tracking_Project_API.Migrations
{
    /// <inheritdoc />
    public partial class AddProductMinStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MinStock",
                table: "Products",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinStock",
                table: "Products");
        }
    }
}
