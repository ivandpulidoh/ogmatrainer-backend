using CapacityControlService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CapacityControlService.Data;

public class CapacityDbContext : DbContext
{
    public CapacityDbContext(DbContextOptions<CapacityDbContext> options) : base(options) { }

    public DbSet<Gimnasio> Gimnasios { get; set; } = null!;
    public DbSet<CheckIn> CheckIns { get; set; } = null!;
    public DbSet<FormularioSintomas> FormulariosSintomas { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Rol> Roles { get; set; } = null!;
    public DbSet<UsuarioRol> UsuarioRoles { get; set; } = null!;
    public DbSet<GimnasioAdministrador> GimnasioAdministradores { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Define composite keys if not using annotations
        modelBuilder.Entity<UsuarioRol>().HasKey(ur => new { ur.IdUsuario, ur.IdRol });
        modelBuilder.Entity<GimnasioAdministrador>().HasKey(ga => new { ga.IdGimnasio, ga.IdUsuario });

        // Configure one-to-one relationship between CheckIn and FormularioSintomas
         modelBuilder.Entity<CheckIn>()
            .HasOne(ci => ci.FormularioSintomas)
            .WithOne(fs => fs.CheckIn)
            .HasForeignKey<FormularioSintomas>(fs => fs.IdCheckin);

        // Add other configurations if needed
    }
}