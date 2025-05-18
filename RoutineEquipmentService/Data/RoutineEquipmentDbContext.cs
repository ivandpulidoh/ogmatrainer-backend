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

    
        modelBuilder.Entity<RutinaDiaEjercicio>()
            .HasIndex(rde => new { rde.IdRutina, rde.DiaNumero, rde.OrdenEnDia })
            .IsUnique()
            .HasDatabaseName("UK_RutinaDia_Orden");

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
            .WithMany(e => e.MaquinasRequeridas)
            .HasForeignKey(em => em.IdEjercicio)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EjercicioMaquina>()
            .HasOne(em => em.MaquinaEjercicio)
            .WithMany(m => m.EjerciciosAsociados)
            .HasForeignKey(em => em.IdMaquina)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EspacioDeportivo>()
            .HasMany(e => e.MaquinasEnEspacio)
            .WithOne(m => m.EspacioDeportivo)
            .HasForeignKey(m => m.IdEspacio)
            .OnDelete(DeleteBehavior.Restrict);

    }
}