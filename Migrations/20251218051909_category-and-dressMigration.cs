using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dress_ordering_system.Migrations
{
    /// <inheritdoc />
    public partial class categoryanddressMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_category",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_category", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_dress",
                columns: table => new
                {
                    dress_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dress_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dress_price = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dress_description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dress_image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cat_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_dress", x => x.dress_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_category");

            migrationBuilder.DropTable(
                name: "tbl_dress");
        }
    }
}
