using Microsoft.EntityFrameworkCore;
using UserManagementService.Models;

namespace UserManagementService.Data
{
    public class UserManagementDbContext : DbContext
    {
        public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Rol> Roles { get; set; } = null!;
        public DbSet<UsuarioRol> UsuarioRoles { get; set; } = null!;
        public DbSet<PersonalInformation> PersonalInformation { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Composite Key for UsuarioRol
            modelBuilder.Entity<UsuarioRol>()
                .HasKey(ur => new { ur.IdUsuario, ur.IdRol });

            // Configure the one-to-many relationship between Usuario and UsuarioRol
            modelBuilder.Entity<UsuarioRol>()
                .HasOne(ur => ur.Usuario)
                .WithMany(u => u.UsuarioRoles)
                .HasForeignKey(ur => ur.IdUsuario);

            // Configure the one-to-many relationship between Rol and UsuarioRol
            modelBuilder.Entity<UsuarioRol>()
                .HasOne(ur => ur.Rol)
                .WithMany(r => r.UsuarioRoles)
                .HasForeignKey(ur => ur.IdRol);

            // Configure the one-to-one relationship between Usuario and PersonalInformation
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.PersonalInformation)
                .WithOne(pi => pi.Usuario)
                .HasForeignKey<PersonalInformation>(pi => pi.IdUsuario); // FK is on PersonalInformation

            // Seed initial Roles if needed (optional)
            modelBuilder.Entity<Rol>().HasData(
                new Rol { IdRol = 1, NombreRol = "Cliente" },
                new Rol { IdRol = 2, NombreRol = "Entrenador" },
                new Rol { IdRol = 3, NombreRol = "Administrador" },
                new Rol { IdRol = 4, NombreRol = "AdminGimnasio" }
            );

            // Add any other specific configurations (indexes, constraints) if needed
             modelBuilder.Entity<Usuario>()
               .HasIndex(u => u.Email)
               .IsUnique(); // Ensure Email is unique if not already enforced by DB

              // Convert DateOnly to Date for MySQL
              modelBuilder.Entity<Usuario>()
                  .Property(u => u.FechaNacimiento)
                  .HasConversion(
                      v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null, // Convert DateOnly to DateTime for DB
                      v => v.HasValue ? DateOnly.FromDateTime(v.Value) : (DateOnly?)null);      // Convert DateTime from DB to DateOnly
        }
    }
}