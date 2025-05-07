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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Define composite unique key for RutinaDiaEjercicios if not implicitly handled by your DB schema script
        modelBuilder.Entity<RutinaDiaEjercicio>()
            .HasIndex(rde => new { rde.IdRutina, rde.DiaNumero, rde.OrdenEnDia })
            .IsUnique()
            .HasDatabaseName("UK_RutinaDia_Orden"); // Match DB constraint name

        // Configure other relationships or constraints if needed
        modelBuilder.Entity<Ejercicio>()
            .HasIndex(e => e.Nombre)
            .IsUnique()
            .HasDatabaseName("UQ_Ejercicios_nombre");        

        modelBuilder.Entity<Rutina>()
            .Property(r => r.FechaCreacion)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}