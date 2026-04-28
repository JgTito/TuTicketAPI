using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuTicketAPI.Migrations
{
    /// <inheritdoc />
    public partial class secreantablastuticket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriaTicket",
                columns: table => new
                {
                    IdCategoriaTicket = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaTicket", x => x.IdCategoriaTicket);
                });

            migrationBuilder.CreateTable(
                name: "EquipoSoporte",
                columns: table => new
                {
                    IdEquipoSoporte = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipoSoporte", x => x.IdEquipoSoporte);
                });

            migrationBuilder.CreateTable(
                name: "EstadoTicket",
                columns: table => new
                {
                    IdEstadoTicket = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    EsEstadoFinal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoTicket", x => x.IdEstadoTicket);
                });

            migrationBuilder.CreateTable(
                name: "PrioridadTicket",
                columns: table => new
                {
                    IdPrioridadTicket = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Nivel = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrioridadTicket", x => x.IdPrioridadTicket);
                });

            migrationBuilder.CreateTable(
                name: "SlaPolitica",
                columns: table => new
                {
                    IdSlaPolitica = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaPolitica", x => x.IdSlaPolitica);
                });

            migrationBuilder.CreateTable(
                name: "TipoRelacionTicket",
                columns: table => new
                {
                    IdTipoRelacionTicket = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoRelacionTicket", x => x.IdTipoRelacionTicket);
                });

            migrationBuilder.CreateTable(
                name: "CategoriaResponsable",
                columns: table => new
                {
                    IdCategoriaResponsable = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCategoriaTicket = table.Column<int>(type: "int", nullable: false),
                    IdUsuarioResponsable = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaResponsable", x => x.IdCategoriaResponsable);
                    table.ForeignKey(
                        name: "FK_CategoriaResponsable_AspNetUsers",
                        column: x => x.IdUsuarioResponsable,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CategoriaResponsable_CategoriaTicket",
                        column: x => x.IdCategoriaTicket,
                        principalTable: "CategoriaTicket",
                        principalColumn: "IdCategoriaTicket");
                });

            migrationBuilder.CreateTable(
                name: "SubcategoriaTicket",
                columns: table => new
                {
                    IdSubcategoriaTicket = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCategoriaTicket = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubcategoriaTicket", x => x.IdSubcategoriaTicket);
                    table.ForeignKey(
                        name: "FK_SubcategoriaTicket_CategoriaTicket",
                        column: x => x.IdCategoriaTicket,
                        principalTable: "CategoriaTicket",
                        principalColumn: "IdCategoriaTicket");
                });

            migrationBuilder.CreateTable(
                name: "CategoriaEquipoSoporte",
                columns: table => new
                {
                    IdCategoriaEquipoSoporte = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCategoriaTicket = table.Column<int>(type: "int", nullable: false),
                    IdEquipoSoporte = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaEquipoSoporte", x => x.IdCategoriaEquipoSoporte);
                    table.ForeignKey(
                        name: "FK_CategoriaEquipoSoporte_CategoriaTicket",
                        column: x => x.IdCategoriaTicket,
                        principalTable: "CategoriaTicket",
                        principalColumn: "IdCategoriaTicket");
                    table.ForeignKey(
                        name: "FK_CategoriaEquipoSoporte_EquipoSoporte",
                        column: x => x.IdEquipoSoporte,
                        principalTable: "EquipoSoporte",
                        principalColumn: "IdEquipoSoporte");
                });

            migrationBuilder.CreateTable(
                name: "EquipoSoporteUsuario",
                columns: table => new
                {
                    IdEquipoSoporteUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEquipoSoporte = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EsLider = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipoSoporteUsuario", x => x.IdEquipoSoporteUsuario);
                    table.ForeignKey(
                        name: "FK_EquipoSoporteUsuario_AspNetUsers",
                        column: x => x.IdUsuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EquipoSoporteUsuario_EquipoSoporte",
                        column: x => x.IdEquipoSoporte,
                        principalTable: "EquipoSoporte",
                        principalColumn: "IdEquipoSoporte");
                });

            migrationBuilder.CreateTable(
                name: "FlujoEstadoTicket",
                columns: table => new
                {
                    IdFlujoEstadoTicket = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEstadoOrigen = table.Column<int>(type: "int", nullable: false),
                    IdEstadoDestino = table.Column<int>(type: "int", nullable: false),
                    RequiereComentario = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlujoEstadoTicket", x => x.IdFlujoEstadoTicket);
                    table.CheckConstraint("CK_FlujoEstadoTicket_EstadosDiferentes", "[IdEstadoOrigen] <> [IdEstadoDestino]");
                    table.ForeignKey(
                        name: "FK_FlujoEstadoTicket_EstadoDestino",
                        column: x => x.IdEstadoDestino,
                        principalTable: "EstadoTicket",
                        principalColumn: "IdEstadoTicket");
                    table.ForeignKey(
                        name: "FK_FlujoEstadoTicket_EstadoOrigen",
                        column: x => x.IdEstadoOrigen,
                        principalTable: "EstadoTicket",
                        principalColumn: "IdEstadoTicket");
                });

            migrationBuilder.CreateTable(
                name: "SlaRegla",
                columns: table => new
                {
                    IdSlaRegla = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSlaPolitica = table.Column<int>(type: "int", nullable: false),
                    IdPrioridadTicket = table.Column<int>(type: "int", nullable: false),
                    IdCategoriaTicket = table.Column<int>(type: "int", nullable: true),
                    MinutosPrimeraRespuesta = table.Column<int>(type: "int", nullable: false),
                    MinutosResolucion = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaRegla", x => x.IdSlaRegla);
                    table.CheckConstraint("CK_SlaRegla_MinutosPrimeraRespuesta", "[MinutosPrimeraRespuesta] > 0");
                    table.CheckConstraint("CK_SlaRegla_MinutosResolucion", "[MinutosResolucion] > 0");
                    table.ForeignKey(
                        name: "FK_SlaRegla_CategoriaTicket",
                        column: x => x.IdCategoriaTicket,
                        principalTable: "CategoriaTicket",
                        principalColumn: "IdCategoriaTicket");
                    table.ForeignKey(
                        name: "FK_SlaRegla_PrioridadTicket",
                        column: x => x.IdPrioridadTicket,
                        principalTable: "PrioridadTicket",
                        principalColumn: "IdPrioridadTicket");
                    table.ForeignKey(
                        name: "FK_SlaRegla_SlaPolitica",
                        column: x => x.IdSlaPolitica,
                        principalTable: "SlaPolitica",
                        principalColumn: "IdSlaPolitica");
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    IdTicket = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdEstadoTicket = table.Column<int>(type: "int", nullable: false),
                    IdPrioridadTicket = table.Column<int>(type: "int", nullable: false),
                    IdSubcategoriaTicket = table.Column<int>(type: "int", nullable: false),
                    IdUsuarioSolicitante = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IdUsuarioAsignado = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaPrimeraRespuesta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaResolucion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CantidadReaperturas = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket", x => x.IdTicket);
                    table.CheckConstraint("CK_Ticket_CantidadReaperturas", "[CantidadReaperturas] >= 0");
                    table.ForeignKey(
                        name: "FK_Ticket_EstadoTicket",
                        column: x => x.IdEstadoTicket,
                        principalTable: "EstadoTicket",
                        principalColumn: "IdEstadoTicket");
                    table.ForeignKey(
                        name: "FK_Ticket_PrioridadTicket",
                        column: x => x.IdPrioridadTicket,
                        principalTable: "PrioridadTicket",
                        principalColumn: "IdPrioridadTicket");
                    table.ForeignKey(
                        name: "FK_Ticket_SubcategoriaTicket",
                        column: x => x.IdSubcategoriaTicket,
                        principalTable: "SubcategoriaTicket",
                        principalColumn: "IdSubcategoriaTicket");
                    table.ForeignKey(
                        name: "FK_Ticket_UsuarioAsignado_AspNetUsers",
                        column: x => x.IdUsuarioAsignado,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Ticket_UsuarioSolicitante_AspNetUsers",
                        column: x => x.IdUsuarioSolicitante,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notificacion",
                columns: table => new
                {
                    IdNotificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuarioDestino = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IdTicket = table.Column<int>(type: "int", nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Leida = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    FechaLectura = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacion", x => x.IdNotificacion);
                    table.ForeignKey(
                        name: "FK_Notificacion_AspNetUsers",
                        column: x => x.IdUsuarioDestino,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notificacion_Ticket",
                        column: x => x.IdTicket,
                        principalTable: "Ticket",
                        principalColumn: "IdTicket");
                });

            migrationBuilder.CreateTable(
                name: "TicketAdjunto",
                columns: table => new
                {
                    IdTicketAdjunto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTicket = table.Column<int>(type: "int", nullable: false),
                    NombreArchivoOriginal = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NombreArchivoGuardado = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PesoBytes = table.Column<long>(type: "bigint", nullable: true),
                    IdUsuarioSubida = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAdjunto", x => x.IdTicketAdjunto);
                    table.CheckConstraint("CK_TicketAdjunto_PesoBytes", "[PesoBytes] IS NULL OR [PesoBytes] >= 0");
                    table.ForeignKey(
                        name: "FK_TicketAdjunto_AspNetUsers",
                        column: x => x.IdUsuarioSubida,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketAdjunto_Ticket",
                        column: x => x.IdTicket,
                        principalTable: "Ticket",
                        principalColumn: "IdTicket");
                });

            migrationBuilder.CreateTable(
                name: "TicketBitacora",
                columns: table => new
                {
                    IdTicketBitacora = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTicket = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsInterno = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IdUsuarioCreacion = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketBitacora", x => x.IdTicketBitacora);
                    table.ForeignKey(
                        name: "FK_TicketBitacora_AspNetUsers",
                        column: x => x.IdUsuarioCreacion,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketBitacora_Ticket",
                        column: x => x.IdTicket,
                        principalTable: "Ticket",
                        principalColumn: "IdTicket");
                });

            migrationBuilder.CreateTable(
                name: "TicketHistorial",
                columns: table => new
                {
                    IdTicketHistorial = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTicket = table.Column<int>(type: "int", nullable: false),
                    CampoModificado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValorAnterior = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValorNuevo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comentario = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdUsuarioModificacion = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketHistorial", x => x.IdTicketHistorial);
                    table.ForeignKey(
                        name: "FK_TicketHistorial_AspNetUsers",
                        column: x => x.IdUsuarioModificacion,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketHistorial_Ticket",
                        column: x => x.IdTicket,
                        principalTable: "Ticket",
                        principalColumn: "IdTicket");
                });

            migrationBuilder.CreateTable(
                name: "TicketRelacion",
                columns: table => new
                {
                    IdTicketRelacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTicketOrigen = table.Column<int>(type: "int", nullable: false),
                    IdTicketRelacionado = table.Column<int>(type: "int", nullable: false),
                    IdTipoRelacionTicket = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdUsuarioCreacion = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketRelacion", x => x.IdTicketRelacion);
                    table.CheckConstraint("CK_TicketRelacion_NoMismoTicket", "[IdTicketOrigen] <> [IdTicketRelacionado]");
                    table.ForeignKey(
                        name: "FK_TicketRelacion_AspNetUsers",
                        column: x => x.IdUsuarioCreacion,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketRelacion_TicketOrigen",
                        column: x => x.IdTicketOrigen,
                        principalTable: "Ticket",
                        principalColumn: "IdTicket");
                    table.ForeignKey(
                        name: "FK_TicketRelacion_TicketRelacionado",
                        column: x => x.IdTicketRelacionado,
                        principalTable: "Ticket",
                        principalColumn: "IdTicket");
                    table.ForeignKey(
                        name: "FK_TicketRelacion_TipoRelacionTicket",
                        column: x => x.IdTipoRelacionTicket,
                        principalTable: "TipoRelacionTicket",
                        principalColumn: "IdTipoRelacionTicket");
                });

            migrationBuilder.CreateTable(
                name: "TicketSla",
                columns: table => new
                {
                    IdTicketSla = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTicket = table.Column<int>(type: "int", nullable: false),
                    IdSlaRegla = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    FechaLimitePrimeraRespuesta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaLimiteResolucion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaPrimeraRespuestaReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaResolucionReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PrimeraRespuestaVencida = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ResolucionVencida = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketSla", x => x.IdTicketSla);
                    table.ForeignKey(
                        name: "FK_TicketSla_SlaRegla",
                        column: x => x.IdSlaRegla,
                        principalTable: "SlaRegla",
                        principalColumn: "IdSlaRegla");
                    table.ForeignKey(
                        name: "FK_TicketSla_Ticket",
                        column: x => x.IdTicket,
                        principalTable: "Ticket",
                        principalColumn: "IdTicket");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaEquipoSoporte_Categoria",
                table: "CategoriaEquipoSoporte",
                column: "IdCategoriaTicket");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaEquipoSoporte_Equipo",
                table: "CategoriaEquipoSoporte",
                column: "IdEquipoSoporte");

            migrationBuilder.CreateIndex(
                name: "UX_CategoriaEquipoSoporte_Categoria_Equipo",
                table: "CategoriaEquipoSoporte",
                columns: new[] { "IdCategoriaTicket", "IdEquipoSoporte" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaResponsable_UsuarioResponsable",
                table: "CategoriaResponsable",
                column: "IdUsuarioResponsable");

            migrationBuilder.CreateIndex(
                name: "UX_CategoriaResponsable_Activo",
                table: "CategoriaResponsable",
                column: "IdCategoriaTicket",
                unique: true,
                filter: "[Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaTicket_Nombre",
                table: "CategoriaTicket",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipoSoporte_Nombre",
                table: "EquipoSoporte",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipoSoporteUsuario_Usuario",
                table: "EquipoSoporteUsuario",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "UX_EquipoSoporteUsuario_Equipo_Usuario",
                table: "EquipoSoporteUsuario",
                columns: new[] { "IdEquipoSoporte", "IdUsuario" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadoTicket_Nombre",
                table: "EstadoTicket",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlujoEstadoTicket_IdEstadoDestino",
                table: "FlujoEstadoTicket",
                column: "IdEstadoDestino");

            migrationBuilder.CreateIndex(
                name: "UX_FlujoEstadoTicket_Origen_Destino",
                table: "FlujoEstadoTicket",
                columns: new[] { "IdEstadoOrigen", "IdEstadoDestino" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_IdTicket",
                table: "Notificacion",
                column: "IdTicket");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_Usuario",
                table: "Notificacion",
                columns: new[] { "IdUsuarioDestino", "Leida", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_PrioridadTicket_Nombre",
                table: "PrioridadTicket",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlaPolitica_Nombre",
                table: "SlaPolitica",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlaRegla_IdCategoriaTicket",
                table: "SlaRegla",
                column: "IdCategoriaTicket");

            migrationBuilder.CreateIndex(
                name: "IX_SlaRegla_IdPrioridadTicket",
                table: "SlaRegla",
                column: "IdPrioridadTicket");

            migrationBuilder.CreateIndex(
                name: "IX_SlaRegla_IdSlaPolitica",
                table: "SlaRegla",
                column: "IdSlaPolitica");

            migrationBuilder.CreateIndex(
                name: "IX_SubcategoriaTicket_Categoria",
                table: "SubcategoriaTicket",
                column: "IdCategoriaTicket");

            migrationBuilder.CreateIndex(
                name: "UX_SubcategoriaTicket_Categoria_Nombre",
                table: "SubcategoriaTicket",
                columns: new[] { "IdCategoriaTicket", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_Codigo",
                table: "Ticket",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_Estado",
                table: "Ticket",
                column: "IdEstadoTicket");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_FechaCreacion",
                table: "Ticket",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_Prioridad",
                table: "Ticket",
                column: "IdPrioridadTicket");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_Subcategoria",
                table: "Ticket",
                column: "IdSubcategoriaTicket");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_UsuarioAsignado",
                table: "Ticket",
                column: "IdUsuarioAsignado");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_UsuarioSolicitante",
                table: "Ticket",
                column: "IdUsuarioSolicitante");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAdjunto_IdUsuarioSubida",
                table: "TicketAdjunto",
                column: "IdUsuarioSubida");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAdjunto_Ticket",
                table: "TicketAdjunto",
                columns: new[] { "IdTicket", "FechaSubida" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketBitacora_IdUsuarioCreacion",
                table: "TicketBitacora",
                column: "IdUsuarioCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_TicketBitacora_Ticket",
                table: "TicketBitacora",
                columns: new[] { "IdTicket", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistorial_IdUsuarioModificacion",
                table: "TicketHistorial",
                column: "IdUsuarioModificacion");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistorial_Ticket",
                table: "TicketHistorial",
                columns: new[] { "IdTicket", "FechaModificacion" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketRelacion_IdTicketRelacionado",
                table: "TicketRelacion",
                column: "IdTicketRelacionado");

            migrationBuilder.CreateIndex(
                name: "IX_TicketRelacion_IdTipoRelacionTicket",
                table: "TicketRelacion",
                column: "IdTipoRelacionTicket");

            migrationBuilder.CreateIndex(
                name: "IX_TicketRelacion_IdUsuarioCreacion",
                table: "TicketRelacion",
                column: "IdUsuarioCreacion");

            migrationBuilder.CreateIndex(
                name: "UX_TicketRelacion_Activo",
                table: "TicketRelacion",
                columns: new[] { "IdTicketOrigen", "IdTicketRelacionado", "IdTipoRelacionTicket" },
                unique: true,
                filter: "[Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSla_FechasLimite",
                table: "TicketSla",
                columns: new[] { "FechaLimitePrimeraRespuesta", "FechaLimiteResolucion" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketSla_IdSlaRegla",
                table: "TicketSla",
                column: "IdSlaRegla");

            migrationBuilder.CreateIndex(
                name: "UX_TicketSla_Ticket_Activo",
                table: "TicketSla",
                column: "IdTicket",
                unique: true,
                filter: "[Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TipoRelacionTicket_Nombre",
                table: "TipoRelacionTicket",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoriaEquipoSoporte");

            migrationBuilder.DropTable(
                name: "CategoriaResponsable");

            migrationBuilder.DropTable(
                name: "EquipoSoporteUsuario");

            migrationBuilder.DropTable(
                name: "FlujoEstadoTicket");

            migrationBuilder.DropTable(
                name: "Notificacion");

            migrationBuilder.DropTable(
                name: "TicketAdjunto");

            migrationBuilder.DropTable(
                name: "TicketBitacora");

            migrationBuilder.DropTable(
                name: "TicketHistorial");

            migrationBuilder.DropTable(
                name: "TicketRelacion");

            migrationBuilder.DropTable(
                name: "TicketSla");

            migrationBuilder.DropTable(
                name: "EquipoSoporte");

            migrationBuilder.DropTable(
                name: "TipoRelacionTicket");

            migrationBuilder.DropTable(
                name: "SlaRegla");

            migrationBuilder.DropTable(
                name: "Ticket");

            migrationBuilder.DropTable(
                name: "SlaPolitica");

            migrationBuilder.DropTable(
                name: "EstadoTicket");

            migrationBuilder.DropTable(
                name: "PrioridadTicket");

            migrationBuilder.DropTable(
                name: "SubcategoriaTicket");

            migrationBuilder.DropTable(
                name: "CategoriaTicket");
        }
    }
}
