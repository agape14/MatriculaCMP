using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatriculaCMP.Migrations
{
    /// <inheritdoc />
    public partial class AddDDJJIdPeruFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DDJJFirmadaIdPeru",
                table: "Solicitudes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DocumentoFirmanteDDJJ",
                table: "Solicitudes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFirmaDDJJ",
                table: "Solicitudes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaDDJJFirmada",
                table: "Solicitudes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DDJJFirmadaIdPeru",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "DocumentoFirmanteDDJJ",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "FechaFirmaDDJJ",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "RutaDDJJFirmada",
                table: "Solicitudes");
        }
    }
}
