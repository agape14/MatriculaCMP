using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatriculaCMP.Migrations
{
    /// <inheritdoc />
    public partial class CambiarGrupoSanguine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "GrupoSanguineoId",
                table: "Personas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personas_GrupoSanguineoId",
                table: "Personas",
                column: "GrupoSanguineoId");

            migrationBuilder.CreateIndex(
                name: "IX_Educaciones_UniversidadId",
                table: "Educaciones",
                column: "UniversidadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Educaciones_Universidades_UniversidadId",
                table: "Educaciones",
                column: "UniversidadId",
                principalTable: "Universidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Personas_MaestroRegistro_GrupoSanguineoId",
                table: "Personas",
                column: "GrupoSanguineoId",
                principalTable: "MaestroRegistro",
                principalColumn: "MaestroRegistro_Key",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Educaciones_Universidades_UniversidadId",
                table: "Educaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Personas_MaestroRegistro_GrupoSanguineoId",
                table: "Personas");

            migrationBuilder.DropIndex(
                name: "IX_Personas_GrupoSanguineoId",
                table: "Personas");

            migrationBuilder.DropIndex(
                name: "IX_Educaciones_UniversidadId",
                table: "Educaciones");

            migrationBuilder.AlterColumn<string>(
                name: "GrupoSanguineoId",
                table: "Personas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
