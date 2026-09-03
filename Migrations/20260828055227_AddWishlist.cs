using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dress_ordering_system.Migrations
{
    /// <inheritdoc />
    public partial class AddWishlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_wishlist",
                columns: table => new
                {
                    wishlist_id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation(
                            "SqlServer:Identity",
                            "1, 1"),

                    customer_id = table.Column<int>(
                        type: "int",
                        nullable: false),

                    dress_id = table.Column<int>(
                        type: "int",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_tbl_wishlist",
                        x => x.wishlist_id);

                    table.ForeignKey(
                        name: "FK_tbl_wishlist_tbl_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "tbl_customer",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_tbl_wishlist_tbl_dress_dress_id",
                        column: x => x.dress_id,
                        principalTable: "tbl_dress",
                        principalColumn: "dress_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_wishlist_customer_id",
                table: "tbl_wishlist",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_wishlist_dress_id",
                table: "tbl_wishlist",
                column: "dress_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_wishlist_customer_id_dress_id",
                table: "tbl_wishlist",
                columns: new[]
                {
                    "customer_id",
                    "dress_id"
                },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_wishlist");
        }
    }
}