using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatriculaCMP.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFirmaSesionUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FirmaSesionUsuarios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FirmaSesionUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoFirma = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmaSesionUsuarios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FirmaSesionUsuarios_CodigoFirma",
                table: "FirmaSesionUsuarios",
                column: "CodigoFirma");
        }
    }
}
