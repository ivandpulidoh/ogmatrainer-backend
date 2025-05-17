using MembershipService.Models;
using Microsoft.EntityFrameworkCore;

namespace MembershipService.Data
{
    public class MembershipDbContext : DbContext
    {
        public MembershipDbContext(DbContextOptions<MembershipDbContext> options) : base(options)
        {
        }

        public DbSet<TipoMembresia> TiposMembresia { get; set; }
        public DbSet<Membresia> Membresias { get; set; }
        public DbSet<Pago> Pagos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TipoMembresia>()
                .HasIndex(tm => tm.Nombre)
                .IsUnique();

            modelBuilder.Entity<Membresia>()
                .Property(m => m.FechaInicio)
                .HasColumnType("date");

            modelBuilder.Entity<Membresia>()
                .Property(m => m.FechaFin)
                .HasColumnType("date");

            modelBuilder.Entity<Membresia>()
                .HasOne(m => m.TipoMembresia)
                .WithMany(tm => tm.Membresias)
                .HasForeignKey(m => m.IdTipoMembresia)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pago>(entity =>
            {
                entity.HasIndex(p => p.IdTransaccionExterna)
                      .IsUnique()
                      .HasFilter("[id_transaccion_externa] IS NOT NULL");
                
                entity.HasOne(p => p.Membresia)
                      .WithMany()
                      .HasForeignKey(p => p.IdMembresia)
                      .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}