using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Notificacion> Notificaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Notificacion entity / Notificaciones table
            modelBuilder.Entity<Notificacion>(entity =>
            {
                // Explicitly map to the 'Notificaciones' table if needed (often inferred)
                entity.ToTable("Notificaciones");

                // Configure the primary key (already done via [Key] attribute, but can be explicit)
                entity.HasKey(e => e.Id);

                // Configure Guid -> uniqueidentifier mapping (EF Core usually handles this well for SQL Server)
                // entity.Property(e => e.Id).HasColumnType("uniqueidentifier");

                // Configure string lengths (already done via attributes)
                entity.Property(e => e.Tipo).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Nombre).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Descripcion).IsRequired(false); // Explicitly optional

                // Configure required DateTime
                entity.Property(e => e.Fecha).IsRequired();

                // Configure required int
                 entity.Property(e => e.IdUsuario).IsRequired();

            });
        }
    }
}