using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatriculaCMP.Migrations
{
    /// <inheritdoc />
    public partial class AddTablesMatricula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsejoRegionales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsejoRegionales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Educaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    EsExtranjera = table.Column<bool>(type: "bit", nullable: false),
                    PaisUniversidadId = table.Column<int>(type: "int", nullable: false),
                    UniversidadId = table.Column<int>(type: "int", nullable: false),
                    FechaEmisionTitulo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoValidacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumeroResolucion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResolucionPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UniversidadPeruanaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Educaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadoCiviles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoCiviles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrupoSanguineos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoSanguineos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsejoRegionalId = table.Column<int>(type: "int", nullable: false),
                    Nombres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApellidoPaterno = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApellidoMaterno = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sexo = table.Column<bool>(type: "bit", nullable: false),
                    EstadoCivilId = table.Column<int>(type: "int", nullable: false),
                    TipoDocumentoId = table.Column<int>(type: "int", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GrupoSanguineoId = table.Column<int>(type: "int", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaisNacimientoId = table.Column<int>(type: "int", nullable: false),
                    DepartamentoNacimientoId = table.Column<int>(type: "int", nullable: false),
                    ProvinciaNacimientoId = table.Column<int>(type: "int", nullable: false),
                    DistritoNacimientoId = table.Column<int>(type: "int", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Celular = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZonaDomicilioId = table.Column<int>(type: "int", nullable: false),
                    DescripcionZona = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ViaDomicilioId = table.Column<int>(type: "int", nullable: false),
                    DescripcionVia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartamentoDomicilioId = table.Column<int>(type: "int", nullable: false),
                    ProvinciaDomicilioId = table.Column<int>(type: "int", nullable: false),
                    DistritoDomicilioId = table.Column<int>(type: "int", nullable: false),
                    FotoPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoDocumentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoDocumentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ubigeos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodigoPadre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ubigeos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ViaDomicilios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViaDomicilios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZonaDomicilios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZonaDomicilios", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsejoRegionales");

            migrationBuilder.DropTable(
                name: "Educaciones");

            migrationBuilder.DropTable(
                name: "EstadoCiviles");

            migrationBuilder.DropTable(
                name: "GrupoSanguineos");

            migrationBuilder.DropTable(
                name: "Personas");

            migrationBuilder.DropTable(
                name: "TipoDocumentos");

            migrationBuilder.DropTable(
                name: "Ubigeos");

            migrationBuilder.DropTable(
                name: "ViaDomicilios");

            migrationBuilder.DropTable(
                name: "ZonaDomicilios");
        }
    }
}
