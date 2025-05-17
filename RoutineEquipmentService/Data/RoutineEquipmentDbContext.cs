using Microsoft.EntityFrameworkCore;
using RoutineEquipmentService.Models;

namespace RoutineEquipmentService.Data;

public class RoutineEquipmentDbContext : DbContext
{
    public RoutineEquipmentDbContext(DbContextOptions<RoutineEquipmentDbContext> options) : base(options) { }

    public DbSet<Ejercicio> Ejercicios { get; set; } = null!;
    public DbSet<Rutina> Rutinas { get; set; } = null!;
    public DbSet<RutinaDiaEjercicio> RutinaDiaEjercicios { get; set; } = null!;
    public DbSet<EspacioDeportivo> EspaciosDeportivos { get; set; } = null!;
    public DbSet<MaquinaEjercicio> MaquinasEjercicio { get; set; } = null!;
    public DbSet<Clase> Clases { get; set; } = null!;    
    public DbSet<EjercicioMaquina> EjercicioMaquinas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Define composite unique key for RutinaDiaEjercicios if not implicitly handled by your DB schema script
        modelBuilder.Entity<RutinaDiaEjercicio>()
            .HasIndex(rde => new { rde.IdRutina, rde.DiaNumero, rde.OrdenEnDia })
            .IsUnique()
            .HasDatabaseName("UK_RutinaDia_Orden"); // Match DB constraint name

        modelBuilder.Entity<Ejercicio>()
            .HasIndex(e => e.Nombre)
            .IsUnique()
            .HasDatabaseName("UQ_Ejercicios_nombre");

        modelBuilder.Entity<Rutina>()
            .Property(r => r.FechaCreacion)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<EjercicioMaquina>()
            .HasKey(em => new { em.IdEjercicio, em.IdMaquina });  

        modelBuilder.Entity<EjercicioMaquina>()
            .HasOne(em => em.Ejercicio)
            .WithMany(e => e.MaquinasRequeridas) // Add this navigation property to Ejercicio
            .HasForeignKey(em => em.IdEjercicio)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EjercicioMaquina>()
            .HasOne(em => em.MaquinaEjercicio)
            .WithMany(m => m.EjerciciosAsociados) // Add this navigation property to MaquinaEjercicio
            .HasForeignKey(em => em.IdMaquina)
            .OnDelete(DeleteBehavior.Cascade);

    }
}