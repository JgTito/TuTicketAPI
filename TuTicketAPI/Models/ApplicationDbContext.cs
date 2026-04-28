using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TuTicketAPI.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<EstadoTicket> EstadoTickets => Set<EstadoTicket>();
        public DbSet<PrioridadTicket> PrioridadTickets => Set<PrioridadTicket>();
        public DbSet<CategoriaTicket> CategoriaTickets => Set<CategoriaTicket>();
        public DbSet<SubcategoriaTicket> SubcategoriaTickets => Set<SubcategoriaTicket>();
        public DbSet<EquipoSoporte> EquipoSoportes => Set<EquipoSoporte>();
        public DbSet<EquipoSoporteUsuario> EquipoSoporteUsuarios => Set<EquipoSoporteUsuario>();
        public DbSet<CategoriaResponsable> CategoriaResponsables => Set<CategoriaResponsable>();
        public DbSet<CategoriaEquipoSoporte> CategoriaEquipoSoportes => Set<CategoriaEquipoSoporte>();
        public DbSet<FlujoEstadoTicket> FlujoEstadoTickets => Set<FlujoEstadoTicket>();
        public DbSet<SlaPolitica> SlaPoliticas => Set<SlaPolitica>();
        public DbSet<SlaRegla> SlaReglas => Set<SlaRegla>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<TicketSla> TicketSlas => Set<TicketSla>();
        public DbSet<TicketHistorial> TicketHistoriales => Set<TicketHistorial>();
        public DbSet<TicketBitacora> TicketBitacoras => Set<TicketBitacora>();
        public DbSet<TicketAdjunto> TicketAdjuntos => Set<TicketAdjunto>();
        public DbSet<TipoRelacionTicket> TipoRelacionTickets => Set<TipoRelacionTicket>();
        public DbSet<TicketRelacion> TicketRelaciones => Set<TicketRelacion>();
        public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<EstadoTicket>(entity =>
            {
                entity.ToTable("EstadoTicket");
                entity.HasKey(e => e.IdEstadoTicket);
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(300);
                entity.Property(e => e.EsEstadoFinal).HasDefaultValue(false);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            builder.Entity<PrioridadTicket>(entity =>
            {
                entity.ToTable("PrioridadTicket");
                entity.HasKey(e => e.IdPrioridadTicket);
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(300);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            builder.Entity<CategoriaTicket>(entity =>
            {
                entity.ToTable("CategoriaTicket");
                entity.HasKey(e => e.IdCategoriaTicket);
                entity.Property(e => e.Nombre).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(300);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            builder.Entity<SubcategoriaTicket>(entity =>
            {
                entity.ToTable("SubcategoriaTicket");
                entity.HasKey(e => e.IdSubcategoriaTicket);
                entity.Property(e => e.Nombre).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(300);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasIndex(e => e.IdCategoriaTicket).HasDatabaseName("IX_SubcategoriaTicket_Categoria");
                entity.HasIndex(e => new { e.IdCategoriaTicket, e.Nombre })
                    .IsUnique()
                    .HasDatabaseName("UX_SubcategoriaTicket_Categoria_Nombre");
                entity.HasOne(e => e.CategoriaTicket)
                    .WithMany(e => e.Subcategorias)
                    .HasForeignKey(e => e.IdCategoriaTicket)
                    .HasConstraintName("FK_SubcategoriaTicket_CategoriaTicket")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<EquipoSoporte>(entity =>
            {
                entity.ToTable("EquipoSoporte");
                entity.HasKey(e => e.IdEquipoSoporte);
                entity.Property(e => e.Nombre).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(300);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            builder.Entity<EquipoSoporteUsuario>(entity =>
            {
                entity.ToTable("EquipoSoporteUsuario");
                entity.HasKey(e => e.IdEquipoSoporteUsuario);
                entity.Property(e => e.IdUsuario).HasMaxLength(450).IsRequired();
                entity.Property(e => e.EsLider).HasDefaultValue(false);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
                entity.HasIndex(e => e.IdUsuario).HasDatabaseName("IX_EquipoSoporteUsuario_Usuario");
                entity.HasIndex(e => new { e.IdEquipoSoporte, e.IdUsuario })
                    .IsUnique()
                    .HasFilter("[Activo] = 1")
                    .HasDatabaseName("UX_EquipoSoporteUsuario_Equipo_Usuario_Activo");
                entity.HasOne(e => e.EquipoSoporte)
                    .WithMany(e => e.Usuarios)
                    .HasForeignKey(e => e.IdEquipoSoporte)
                    .HasConstraintName("FK_EquipoSoporteUsuario_EquipoSoporte")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuario)
                    .HasConstraintName("FK_EquipoSoporteUsuario_AspNetUsers")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<CategoriaResponsable>(entity =>
            {
                entity.ToTable("CategoriaResponsable");
                entity.HasKey(e => e.IdCategoriaResponsable);
                entity.Property(e => e.IdUsuarioResponsable).HasMaxLength(450).IsRequired();
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
                entity.HasIndex(e => e.IdUsuarioResponsable).HasDatabaseName("IX_CategoriaResponsable_UsuarioResponsable");
                entity.HasIndex(e => e.IdCategoriaTicket)
                    .IsUnique()
                    .HasFilter("[Activo] = 1")
                    .HasDatabaseName("UX_CategoriaResponsable_Activo");
                entity.HasOne(e => e.CategoriaTicket)
                    .WithMany(e => e.Responsables)
                    .HasForeignKey(e => e.IdCategoriaTicket)
                    .HasConstraintName("FK_CategoriaResponsable_CategoriaTicket")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.UsuarioResponsable)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuarioResponsable)
                    .HasConstraintName("FK_CategoriaResponsable_AspNetUsers")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<CategoriaEquipoSoporte>(entity =>
            {
                entity.ToTable("CategoriaEquipoSoporte");
                entity.HasKey(e => e.IdCategoriaEquipoSoporte);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasIndex(e => e.IdCategoriaTicket).HasDatabaseName("IX_CategoriaEquipoSoporte_Categoria");
                entity.HasIndex(e => e.IdEquipoSoporte).HasDatabaseName("IX_CategoriaEquipoSoporte_Equipo");
                entity.HasIndex(e => new { e.IdCategoriaTicket, e.IdEquipoSoporte })
                    .IsUnique()
                    .HasFilter("[Activo] = 1")
                    .HasDatabaseName("UX_CategoriaEquipoSoporte_Categoria_Equipo_Activo");
                entity.HasOne(e => e.CategoriaTicket)
                    .WithMany(e => e.EquiposSoporte)
                    .HasForeignKey(e => e.IdCategoriaTicket)
                    .HasConstraintName("FK_CategoriaEquipoSoporte_CategoriaTicket")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.EquipoSoporte)
                    .WithMany(e => e.Categorias)
                    .HasForeignKey(e => e.IdEquipoSoporte)
                    .HasConstraintName("FK_CategoriaEquipoSoporte_EquipoSoporte")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<FlujoEstadoTicket>(entity =>
            {
                entity.ToTable("FlujoEstadoTicket", table =>
                    table.HasCheckConstraint("CK_FlujoEstadoTicket_EstadosDiferentes", "[IdEstadoOrigen] <> [IdEstadoDestino]"));
                entity.HasKey(e => e.IdFlujoEstadoTicket);
                entity.Property(e => e.RequiereComentario).HasDefaultValue(false);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasIndex(e => new { e.IdEstadoOrigen, e.IdEstadoDestino })
                    .IsUnique()
                    .HasFilter("[Activo] = 1")
                    .HasDatabaseName("UX_FlujoEstadoTicket_Origen_Destino_Activo");
                entity.HasOne(e => e.EstadoOrigen)
                    .WithMany(e => e.FlujosOrigen)
                    .HasForeignKey(e => e.IdEstadoOrigen)
                    .HasConstraintName("FK_FlujoEstadoTicket_EstadoOrigen")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.EstadoDestino)
                    .WithMany(e => e.FlujosDestino)
                    .HasForeignKey(e => e.IdEstadoDestino)
                    .HasConstraintName("FK_FlujoEstadoTicket_EstadoDestino")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<SlaPolitica>(entity =>
            {
                entity.ToTable("SlaPolitica");
                entity.HasKey(e => e.IdSlaPolitica);
                entity.Property(e => e.Nombre).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(300);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            builder.Entity<SlaRegla>(entity =>
            {
                entity.ToTable("SlaRegla", table =>
                {
                    table.HasCheckConstraint("CK_SlaRegla_MinutosPrimeraRespuesta", "[MinutosPrimeraRespuesta] > 0");
                    table.HasCheckConstraint("CK_SlaRegla_MinutosResolucion", "[MinutosResolucion] > 0");
                });
                entity.HasKey(e => e.IdSlaRegla);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasIndex(e => e.IdSlaPolitica)
                    .HasDatabaseName("IX_SlaRegla_IdSlaPolitica");
                entity.HasIndex(e => new { e.IdSlaPolitica, e.IdPrioridadTicket, e.IdCategoriaTicket })
                    .IsUnique()
                    .HasFilter("[Activo] = 1 AND [IdCategoriaTicket] IS NOT NULL")
                    .HasDatabaseName("UX_SlaRegla_Politica_Prioridad_Categoria_Activo");
                entity.HasIndex(e => new { e.IdSlaPolitica, e.IdPrioridadTicket })
                    .IsUnique()
                    .HasFilter("[Activo] = 1 AND [IdCategoriaTicket] IS NULL")
                    .HasDatabaseName("UX_SlaRegla_Politica_Prioridad_Global_Activo");
                entity.HasOne(e => e.SlaPolitica)
                    .WithMany(e => e.SlaReglas)
                    .HasForeignKey(e => e.IdSlaPolitica)
                    .HasConstraintName("FK_SlaRegla_SlaPolitica")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.PrioridadTicket)
                    .WithMany(e => e.SlaReglas)
                    .HasForeignKey(e => e.IdPrioridadTicket)
                    .HasConstraintName("FK_SlaRegla_PrioridadTicket")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.CategoriaTicket)
                    .WithMany(e => e.SlaReglas)
                    .HasForeignKey(e => e.IdCategoriaTicket)
                    .HasConstraintName("FK_SlaRegla_CategoriaTicket")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<Ticket>(entity =>
            {
                entity.ToTable("Ticket", table =>
                    table.HasCheckConstraint("CK_Ticket_CantidadReaperturas", "[CantidadReaperturas] >= 0"));
                entity.HasKey(e => e.IdTicket);
                entity.Property(e => e.Codigo).HasMaxLength(30).IsRequired();
                entity.Property(e => e.Titulo).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Descripcion).IsRequired();
                entity.Property(e => e.IdUsuarioSolicitante).HasMaxLength(450).IsRequired();
                entity.Property(e => e.IdUsuarioAsignado).HasMaxLength(450);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
                entity.Property(e => e.CantidadReaperturas).HasDefaultValue(0);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.HasIndex(e => e.IdEstadoTicket).HasDatabaseName("IX_Ticket_Estado");
                entity.HasIndex(e => e.IdPrioridadTicket).HasDatabaseName("IX_Ticket_Prioridad");
                entity.HasIndex(e => e.IdSubcategoriaTicket).HasDatabaseName("IX_Ticket_Subcategoria");
                entity.HasIndex(e => e.IdUsuarioSolicitante).HasDatabaseName("IX_Ticket_UsuarioSolicitante");
                entity.HasIndex(e => e.IdUsuarioAsignado).HasDatabaseName("IX_Ticket_UsuarioAsignado");
                entity.HasIndex(e => e.FechaCreacion).HasDatabaseName("IX_Ticket_FechaCreacion");
                entity.HasOne(e => e.EstadoTicket)
                    .WithMany(e => e.Tickets)
                    .HasForeignKey(e => e.IdEstadoTicket)
                    .HasConstraintName("FK_Ticket_EstadoTicket")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.PrioridadTicket)
                    .WithMany(e => e.Tickets)
                    .HasForeignKey(e => e.IdPrioridadTicket)
                    .HasConstraintName("FK_Ticket_PrioridadTicket")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.SubcategoriaTicket)
                    .WithMany(e => e.Tickets)
                    .HasForeignKey(e => e.IdSubcategoriaTicket)
                    .HasConstraintName("FK_Ticket_SubcategoriaTicket")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.UsuarioSolicitante)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuarioSolicitante)
                    .HasConstraintName("FK_Ticket_UsuarioSolicitante_AspNetUsers")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.UsuarioAsignado)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuarioAsignado)
                    .HasConstraintName("FK_Ticket_UsuarioAsignado_AspNetUsers")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<TicketSla>(entity =>
            {
                entity.ToTable("TicketSla");
                entity.HasKey(e => e.IdTicketSla);
                entity.Property(e => e.FechaInicio).HasDefaultValueSql("SYSDATETIME()");
                entity.Property(e => e.PrimeraRespuestaVencida).HasDefaultValue(false);
                entity.Property(e => e.ResolucionVencida).HasDefaultValue(false);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasIndex(e => e.IdTicket).HasDatabaseName("IX_TicketSla_Ticket");
                entity.HasIndex(e => e.IdTicket)
                    .IsUnique()
                    .HasFilter("[Activo] = 1")
                    .HasDatabaseName("UX_TicketSla_Ticket_Activo");
                entity.HasIndex(e => new { e.FechaLimitePrimeraRespuesta, e.FechaLimiteResolucion })
                    .HasDatabaseName("IX_TicketSla_FechasLimite");
                entity.HasOne(e => e.Ticket)
                    .WithMany(e => e.Slas)
                    .HasForeignKey(e => e.IdTicket)
                    .HasConstraintName("FK_TicketSla_Ticket")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.SlaRegla)
                    .WithMany(e => e.TicketSlas)
                    .HasForeignKey(e => e.IdSlaRegla)
                    .HasConstraintName("FK_TicketSla_SlaRegla")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<TicketHistorial>(entity =>
            {
                entity.ToTable("TicketHistorial");
                entity.HasKey(e => e.IdTicketHistorial);
                entity.Property(e => e.CampoModificado).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Comentario).HasMaxLength(500);
                entity.Property(e => e.IdUsuarioModificacion).HasMaxLength(450).IsRequired();
                entity.Property(e => e.FechaModificacion).HasDefaultValueSql("SYSDATETIME()");
                entity.HasIndex(e => new { e.IdTicket, e.FechaModificacion }).HasDatabaseName("IX_TicketHistorial_Ticket");
                entity.HasOne(e => e.Ticket)
                    .WithMany(e => e.Historiales)
                    .HasForeignKey(e => e.IdTicket)
                    .HasConstraintName("FK_TicketHistorial_Ticket")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.UsuarioModificacion)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuarioModificacion)
                    .HasConstraintName("FK_TicketHistorial_AspNetUsers")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<TicketBitacora>(entity =>
            {
                entity.ToTable("TicketBitacora");
                entity.HasKey(e => e.IdTicketBitacora);
                entity.Property(e => e.Comentario).IsRequired();
                entity.Property(e => e.EsInterno).HasDefaultValue(false);
                entity.Property(e => e.IdUsuarioCreacion).HasMaxLength(450).IsRequired();
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasIndex(e => new { e.IdTicket, e.FechaCreacion }).HasDatabaseName("IX_TicketBitacora_Ticket");
                entity.HasOne(e => e.Ticket)
                    .WithMany(e => e.Bitacoras)
                    .HasForeignKey(e => e.IdTicket)
                    .HasConstraintName("FK_TicketBitacora_Ticket")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.UsuarioCreacion)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuarioCreacion)
                    .HasConstraintName("FK_TicketBitacora_AspNetUsers")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<TicketAdjunto>(entity =>
            {
                entity.ToTable("TicketAdjunto", table =>
                    table.HasCheckConstraint("CK_TicketAdjunto_PesoBytes", "[PesoBytes] IS NULL OR [PesoBytes] >= 0"));
                entity.HasKey(e => e.IdTicketAdjunto);
                entity.Property(e => e.NombreArchivoOriginal).HasMaxLength(255).IsRequired();
                entity.Property(e => e.NombreArchivoGuardado).HasMaxLength(255).IsRequired();
                entity.Property(e => e.RutaArchivo).HasMaxLength(500).IsRequired();
                entity.Property(e => e.TipoContenido).HasMaxLength(100);
                entity.Property(e => e.Extension).HasMaxLength(20);
                entity.Property(e => e.IdUsuarioSubida).HasMaxLength(450).IsRequired();
                entity.Property(e => e.FechaSubida).HasDefaultValueSql("SYSDATETIME()");
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasIndex(e => new { e.IdTicket, e.FechaSubida }).HasDatabaseName("IX_TicketAdjunto_Ticket");
                entity.HasOne(e => e.Ticket)
                    .WithMany(e => e.Adjuntos)
                    .HasForeignKey(e => e.IdTicket)
                    .HasConstraintName("FK_TicketAdjunto_Ticket")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.UsuarioSubida)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuarioSubida)
                    .HasConstraintName("FK_TicketAdjunto_AspNetUsers")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<TipoRelacionTicket>(entity =>
            {
                entity.ToTable("TipoRelacionTicket");
                entity.HasKey(e => e.IdTipoRelacionTicket);
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(250);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            builder.Entity<TicketRelacion>(entity =>
            {
                entity.ToTable("TicketRelacion", table =>
                    table.HasCheckConstraint("CK_TicketRelacion_NoMismoTicket", "[IdTicketOrigen] <> [IdTicketRelacionado]"));
                entity.HasKey(e => e.IdTicketRelacion);
                entity.Property(e => e.Observacion).HasMaxLength(500);
                entity.Property(e => e.IdUsuarioCreacion).HasMaxLength(450).IsRequired();
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasIndex(e => new { e.IdTicketOrigen, e.IdTicketRelacionado, e.IdTipoRelacionTicket })
                    .IsUnique()
                    .HasFilter("[Activo] = 1")
                    .HasDatabaseName("UX_TicketRelacion_Activo");
                entity.HasOne(e => e.TicketOrigen)
                    .WithMany(e => e.RelacionesOrigen)
                    .HasForeignKey(e => e.IdTicketOrigen)
                    .HasConstraintName("FK_TicketRelacion_TicketOrigen")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.TicketRelacionado)
                    .WithMany(e => e.RelacionesDestino)
                    .HasForeignKey(e => e.IdTicketRelacionado)
                    .HasConstraintName("FK_TicketRelacion_TicketRelacionado")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.TipoRelacionTicket)
                    .WithMany(e => e.TicketRelaciones)
                    .HasForeignKey(e => e.IdTipoRelacionTicket)
                    .HasConstraintName("FK_TicketRelacion_TipoRelacionTicket")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.UsuarioCreacion)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuarioCreacion)
                    .HasConstraintName("FK_TicketRelacion_AspNetUsers")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<Notificacion>(entity =>
            {
                entity.ToTable("Notificacion");
                entity.HasKey(e => e.IdNotificacion);
                entity.Property(e => e.IdUsuarioDestino).HasMaxLength(450).IsRequired();
                entity.Property(e => e.Titulo).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Mensaje).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Leida).HasDefaultValue(false);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
                entity.HasIndex(e => new { e.IdUsuarioDestino, e.Leida, e.FechaCreacion }).HasDatabaseName("IX_Notificacion_Usuario");
                entity.HasOne(e => e.UsuarioDestino)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuarioDestino)
                    .HasConstraintName("FK_Notificacion_AspNetUsers")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Ticket)
                    .WithMany(e => e.Notificaciones)
                    .HasForeignKey(e => e.IdTicket)
                    .HasConstraintName("FK_Notificacion_Ticket")
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
