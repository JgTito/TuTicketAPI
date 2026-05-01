using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuTicketAPI.Migrations
{
    /// <inheritdoc />
    public partial class activoticket2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Ticket");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Ticket",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
