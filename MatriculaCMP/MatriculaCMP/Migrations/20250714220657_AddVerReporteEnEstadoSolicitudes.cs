using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatriculaCMP.Migrations
{
    /// <inheritdoc />
    public partial class AddVerReporteEnEstadoSolicitudes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "VerReporte",
                table: "EstadoSolicitudes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerReporte",
                table: "EstadoSolicitudes");
        }
    }
}
