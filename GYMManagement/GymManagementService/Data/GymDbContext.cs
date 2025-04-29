using Microsoft.EntityFrameworkCore;
using GymManagementService.Models; // Your models namespace

namespace GymManagementService.Data
{
    public class GymDbContext : DbContext
    {
        public GymDbContext(DbContextOptions<GymDbContext> options) : base(options) { }

        // DbSet for each entity you want to manage
        public DbSet<Gimnasio> Gimnasios { get; set; } = null!;
        public DbSet<HorarioGimnasio> HorariosGimnasio { get; set; } = null!;
        public DbSet<GimnasioAdministrador> GimnasioAdministradores { get; set; } = null!;
        public DbSet<EntrenadorGimnasio> EntrenadorGimnasios { get; set; } = null!;
        // Add DbSets for other related entities if needed (e.g., EspaciosDeportivos)

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite primary keys for join tables
            modelBuilder.Entity<GimnasioAdministrador>()
                .HasKey(ga => new { ga.IdGimnasio, ga.IdUsuario }); // Define composite key

            modelBuilder.Entity<EntrenadorGimnasio>()
                .HasKey(eg => new { eg.IdUsuario, eg.IdGimnasio }); // Define composite key

            // Configure relationships (EF Core often infers these, but explicit is clearer)
            // Example: Gimnasio to Horarios (One-to-Many)
            modelBuilder.Entity<Gimnasio>()
                .HasMany(g => g.Horarios)
                .WithOne(h => h.Gimnasio)
                .HasForeignKey(h => h.IdGimnasio)
                .OnDelete(DeleteBehavior.Cascade); // Matches your schema's ON DELETE CASCADE

             // Example: Gimnasio to Admins (Many-to-Many using join entity)
            modelBuilder.Entity<GimnasioAdministrador>()
                .HasOne(ga => ga.Gimnasio)
                .WithMany(g => g.Administradores)
                .HasForeignKey(ga => ga.IdGimnasio)
                .OnDelete(DeleteBehavior.Cascade);

            // Example: Gimnasio to Trainers (Many-to-Many using join entity)
             modelBuilder.Entity<EntrenadorGimnasio>()
                .HasOne(eg => eg.Gimnasio)
                .WithMany(g => g.Entrenadores)
                .HasForeignKey(eg => eg.IdGimnasio)
                .OnDelete(DeleteBehavior.Cascade);


            // Configure UNIQUE constraint for HorariosGimnasio
             modelBuilder.Entity<HorarioGimnasio>()
                .HasIndex(h => new { h.IdGimnasio, h.DiaSemana })
                .IsUnique(); // Matches uk_gimnasio_dia

             // Map TimeOnly correctly for MySQL if default mapping isn't sufficient
             // (Pomelo provider usually handles this well for recent versions)
             modelBuilder.Entity<HorarioGimnasio>()
                .Property(e => e.HoraApertura)
                .HasColumnType("time"); // Be explicit if needed
             modelBuilder.Entity<HorarioGimnasio>()
                .Property(e => e.HoraCierre)
                .HasColumnType("time");

             // Map ENUM to string or integer depending on provider/preference
             // Pomelo can often map enums to strings automatically matching MySQL ENUMs
             modelBuilder.Entity<HorarioGimnasio>()
                .Property(h => h.DiaSemana)
                .HasConversion<string>() // Store as string ('Lunes', 'Martes', etc.)
                .HasMaxLength(10); // Ensure length matches ENUM definition potential


             // Add configurations for other entities and constraints...
        }
    }
}