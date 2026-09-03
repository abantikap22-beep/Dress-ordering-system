using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dress_ordering_system.Migrations
{
    public partial class updateddressandcategorymigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_tbl_dress_cat_id",
                table: "tbl_dress",
                column: "cat_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_dress_tbl_category_cat_id",
                table: "tbl_dress",
                column: "cat_id",
                principalTable: "tbl_category",
                principalColumn: "category_id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_dress_tbl_category_cat_id",
                table: "tbl_dress");

            migrationBuilder.DropIndex(
                name: "IX_tbl_dress_cat_id",
                table: "tbl_dress");
        }
    }
}
