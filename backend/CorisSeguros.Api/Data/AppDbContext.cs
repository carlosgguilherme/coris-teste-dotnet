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
    public DbSet<Canal> Canais => Set<Canal>();
    public DbSet<Campanha> Campanhas => Set<Campanha>();
    public DbSet<Cotacao> Cotacoes => Set<Cotacao>();
    public DbSet<FunilEvento> FunilEventos => Set<FunilEvento>();
    public DbSet<Sinistro> Sinistros => Set<Sinistro>();
    public DbSet<Atendimento> Atendimentos => Set<Atendimento>();

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

            // relacionamento 1:N: um segurado pode ter várias apólices
            entidade.HasOne(a => a.Segurado)
                .WithMany(s => s.Apolices)
                .HasForeignKey(a => a.SeguradoId);

            // filtro global: toda consulta já ignora as apólices excluídas, não preciso lembrar de filtrar
            entidade.HasQueryFilter(a => a.ExcluidoEm == null);
            entidade.HasIndex(a => a.CriadoEm);
        });

        // ---------- tabelas da dashboard ----------

        modelBuilder.Entity<Canal>(entidade =>
        {
            entidade.ToTable("canais");
            entidade.Property(c => c.Codigo).HasMaxLength(20).IsRequired();
            entidade.Property(c => c.Nome).HasMaxLength(60).IsRequired();
            entidade.HasIndex(c => c.Codigo).IsUnique();
        });

        modelBuilder.Entity<Campanha>(entidade =>
        {
            entidade.ToTable("campanhas");
            entidade.Property(c => c.Nome).HasMaxLength(80).IsRequired();
            entidade.Property(c => c.UtmSource).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<Cotacao>(entidade =>
        {
            entidade.ToTable("cotacoes");
            entidade.Property(c => c.Destino).HasMaxLength(30);
            entidade.Property(c => c.Plano).HasMaxLength(20);
            entidade.Property(c => c.Device).HasMaxLength(10);
            entidade.Property(c => c.Status).HasMaxLength(20);
            entidade.Property(c => c.EtapaAbandono).HasMaxLength(20);
            entidade.HasIndex(c => new { c.CriadoEm, c.Status });
        });

        modelBuilder.Entity<FunilEvento>(entidade =>
        {
            entidade.ToTable("funil_eventos");
            entidade.Property(e => e.Etapa).HasMaxLength(20);
            entidade.HasIndex(e => new { e.CotacaoId, e.Etapa });
        });

        modelBuilder.Entity<Sinistro>(entidade =>
        {
            entidade.ToTable("sinistros");
            entidade.Property(s => s.Numero).HasMaxLength(20);
            entidade.Property(s => s.Cobertura).HasMaxLength(30);
            entidade.Property(s => s.Status).HasMaxLength(20);
            entidade.Property(s => s.MotivoNegativa).HasMaxLength(120);
            entidade.HasIndex(s => s.Numero).IsUnique();
            entidade.HasIndex(s => new { s.Status, s.DataAviso });
            // e os sinistros de apólice excluída também somem
            entidade.HasQueryFilter(s => s.Apolice.ExcluidoEm == null);
        });

        modelBuilder.Entity<Atendimento>(entidade =>
        {
            entidade.ToTable("atendimentos");
            entidade.Property(a => a.Canal).HasMaxLength(20);
            entidade.Property(a => a.Tipo).HasMaxLength(40);
            entidade.HasIndex(a => a.Inicio);
        });
    }
}
