using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatriculaCMP.Migrations
{
    /// <inheritdoc />
    public partial class AddEducacionDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "EstadoSolicitudes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EducacionDocumentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EducacionId = table.Column<int>(type: "int", nullable: false),
                    TituloMedicoCirujanoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConstanciaInscripcionSuneduPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificadoAntecedentesPenalesPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CarnetExtranjeriaPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConstanciaInscripcionReconocimientoSuneduPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConstanciaInscripcionRevalidacionUniversidadNacionalPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReconocimientoSuneduPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevalidacionUniversidadNacionalPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducacionDocumentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducacionDocumentos_Educaciones_EducacionId",
                        column: x => x.EducacionId,
                        principalTable: "Educaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EducacionDocumentos_EducacionId",
                table: "EducacionDocumentos",
                column: "EducacionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EducacionDocumentos");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "EstadoSolicitudes");
        }
    }
}
