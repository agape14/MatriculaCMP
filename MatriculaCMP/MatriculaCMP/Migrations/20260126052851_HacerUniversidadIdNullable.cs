using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatriculaCMP.Migrations
{
    /// <inheritdoc />
    public partial class HacerUniversidadIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Educaciones_Universidades_UniversidadId",
                table: "Educaciones");

            migrationBuilder.AlterColumn<int>(
                name: "UniversidadId",
                table: "Educaciones",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Educaciones_Universidades_UniversidadId",
                table: "Educaciones",
                column: "UniversidadId",
                principalTable: "Universidades",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Educaciones_Universidades_UniversidadId",
                table: "Educaciones");

            migrationBuilder.AlterColumn<int>(
                name: "UniversidadId",
                table: "Educaciones",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Educaciones_Universidades_UniversidadId",
                table: "Educaciones",
                column: "UniversidadId",
                principalTable: "Universidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
