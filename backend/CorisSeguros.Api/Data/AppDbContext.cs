using CorisSeguros.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CorisSeguros.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Segurado> Segurados => Set<Segurado>();
    public DbSet<Apolice> Apolices => Set<Apolice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Segurado>(entidade =>
        {
            entidade.ToTable("segurados");
            entidade.Property(s => s.Nome).HasMaxLength(120).IsRequired();
            entidade.Property(s => s.Cpf).HasMaxLength(11).IsFixedLength().IsRequired();
            entidade.Property(s => s.Email).HasMaxLength(150).IsRequired();
            entidade.HasIndex(s => s.Cpf).IsUnique();
        });

        modelBuilder.Entity<Apolice>(entidade =>
        {
            entidade.ToTable("apolices");
            entidade.Property(a => a.Numero).HasMaxLength(20).IsRequired();
            entidade.Property(a => a.Destino).HasMaxLength(30).IsRequired();
            entidade.Property(a => a.Plano).HasMaxLength(20).IsRequired();
            entidade.Property(a => a.Status).HasMaxLength(20).IsRequired();
            entidade.HasIndex(a => a.Numero).IsUnique();
            entidade.HasIndex(a => a.Status);

            // um segurado tem várias apólices (1:N)
            entidade.HasOne(a => a.Segurado)
                .WithMany(s => s.Apolices)
                .HasForeignKey(a => a.SeguradoId);

            // exclusão lógica: as consultas ignoram as apólices excluídas
            entidade.HasQueryFilter(a => a.ExcluidoEm == null);
        });
    }
}
