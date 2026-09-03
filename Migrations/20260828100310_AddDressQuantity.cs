using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dress_ordering_system.Migrations
{
    /// <inheritdoc />
    public partial class AddDressQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "dress_quantity",
                table: "tbl_dress",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dress_quantity",
                table: "tbl_dress");
        }
    }
}
