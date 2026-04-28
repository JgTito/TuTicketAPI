using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuTicketAPI.Migrations
{
    /// <inheritdoc />
    public partial class AjustaIndicesUnicosActivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_FlujoEstadoTicket_Origen_Destino",
                table: "FlujoEstadoTicket");

            migrationBuilder.DropIndex(
                name: "UX_EquipoSoporteUsuario_Equipo_Usuario",
                table: "EquipoSoporteUsuario");

            migrationBuilder.DropIndex(
                name: "UX_CategoriaEquipoSoporte_Categoria_Equipo",
                table: "CategoriaEquipoSoporte");

            migrationBuilder.CreateIndex(
                name: "UX_FlujoEstadoTicket_Origen_Destino_Activo",
                table: "FlujoEstadoTicket",
                columns: new[] { "IdEstadoOrigen", "IdEstadoDestino" },
                unique: true,
                filter: "[Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_EquipoSoporteUsuario_Equipo_Usuario_Activo",
                table: "EquipoSoporteUsuario",
                columns: new[] { "IdEquipoSoporte", "IdUsuario" },
                unique: true,
                filter: "[Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_CategoriaEquipoSoporte_Categoria_Equipo_Activo",
                table: "CategoriaEquipoSoporte",
                columns: new[] { "IdCategoriaTicket", "IdEquipoSoporte" },
                unique: true,
                filter: "[Activo] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_FlujoEstadoTicket_Origen_Destino_Activo",
                table: "FlujoEstadoTicket");

            migrationBuilder.DropIndex(
                name: "UX_EquipoSoporteUsuario_Equipo_Usuario_Activo",
                table: "EquipoSoporteUsuario");

            migrationBuilder.DropIndex(
                name: "UX_CategoriaEquipoSoporte_Categoria_Equipo_Activo",
                table: "CategoriaEquipoSoporte");

            migrationBuilder.CreateIndex(
                name: "UX_FlujoEstadoTicket_Origen_Destino",
                table: "FlujoEstadoTicket",
                columns: new[] { "IdEstadoOrigen", "IdEstadoDestino" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_EquipoSoporteUsuario_Equipo_Usuario",
                table: "EquipoSoporteUsuario",
                columns: new[] { "IdEquipoSoporte", "IdUsuario" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CategoriaEquipoSoporte_Categoria_Equipo",
                table: "CategoriaEquipoSoporte",
                columns: new[] { "IdCategoriaTicket", "IdEquipoSoporte" },
                unique: true);
        }
    }
}
