using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatriculaCMP.Migrations
{
    /// <inheritdoc />
    public partial class AddCorrelativoAndHistorialToSolicitudes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroSolicitud",
                table: "Solicitudes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Correlativos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UltimoNumero = table.Column<int>(type: "int", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Correlativos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudHistorialEstados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    EstadoAnteriorId = table.Column<int>(type: "int", nullable: true),
                    EstadoNuevoId = table.Column<int>(type: "int", nullable: false),
                    FechaCambio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsuarioCambio = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudHistorialEstados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudHistorialEstados_EstadoSolicitudes_EstadoAnteriorId",
                        column: x => x.EstadoAnteriorId,
                        principalTable: "EstadoSolicitudes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudHistorialEstados_EstadoSolicitudes_EstadoNuevoId",
                        column: x => x.EstadoNuevoId,
                        principalTable: "EstadoSolicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudHistorialEstados_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudHistorialEstados_EstadoAnteriorId",
                table: "SolicitudHistorialEstados",
                column: "EstadoAnteriorId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudHistorialEstados_EstadoNuevoId",
                table: "SolicitudHistorialEstados",
                column: "EstadoNuevoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudHistorialEstados_SolicitudId",
                table: "SolicitudHistorialEstados",
                column: "SolicitudId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Correlativos");

            migrationBuilder.DropTable(
                name: "SolicitudHistorialEstados");

            migrationBuilder.DropColumn(
                name: "NumeroSolicitud",
                table: "Solicitudes");
        }
    }
}
