using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuTicketAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregaUnicidadSlaRegla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_SlaRegla_Politica_Prioridad_Categoria_Activo",
                table: "SlaRegla",
                columns: new[] { "IdSlaPolitica", "IdPrioridadTicket", "IdCategoriaTicket" },
                unique: true,
                filter: "[Activo] = 1 AND [IdCategoriaTicket] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_SlaRegla_Politica_Prioridad_Global_Activo",
                table: "SlaRegla",
                columns: new[] { "IdSlaPolitica", "IdPrioridadTicket" },
                unique: true,
                filter: "[Activo] = 1 AND [IdCategoriaTicket] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_SlaRegla_Politica_Prioridad_Categoria_Activo",
                table: "SlaRegla");

            migrationBuilder.DropIndex(
                name: "UX_SlaRegla_Politica_Prioridad_Global_Activo",
                table: "SlaRegla");
        }
    }
}
