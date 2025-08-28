using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatriculaCMP.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdenToMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Orden",
                table: "Menu",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Orden",
                table: "Menu");
        }
    }
}
