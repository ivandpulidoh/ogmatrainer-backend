using BookingManagementService.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingManagementService.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<MaquinaEjercicio> MaquinasEjercicio { get; set; } = null!;
    public DbSet<Clase> Clases { get; set; } = null!;
    public DbSet<ReservaMaquina> ReservasMaquinas { get; set; } = null!;
    public DbSet<ReservaEntrenador> ReservasEntrenador { get; set; } = null!;
    public DbSet<InscripcionClase> InscripcionesClases { get; set; } = null!;
    public DbSet<RutinaReserva> RutinaReservas { get; set; } = null!;
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure composite keys or specific relationships if not covered by annotations
        modelBuilder.Entity<InscripcionClase>()
            .HasIndex(ic => new { ic.IdUsuario, ic.IdClase })
            .IsUnique();

        // Add other configurations (like unique constraints from schema) if needed
    }
}