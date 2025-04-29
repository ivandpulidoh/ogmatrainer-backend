using AuthenticationService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<Usuarios> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.ToTable("Usuarios");

            entity.HasKey(e => e.id_usuario);
            entity.Property(e => e.id_usuario).HasColumnName("id_usuario").ValueGeneratedOnAdd();

            entity.Property(e => e.email).HasColumnName("email").HasMaxLength(100).IsRequired();
            entity.Property(e => e.email).HasColumnType("varchar(100)");
            entity.HasIndex(e => e.email).IsUnique().HasDatabaseName("uk_email");

            entity.Property(e => e.password_hash).HasColumnName("password_hash").HasColumnType("varchar(255)").IsRequired();
        });

        }
    }
}