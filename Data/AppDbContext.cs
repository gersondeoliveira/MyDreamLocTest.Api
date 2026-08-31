using Microsoft.EntityFrameworkCore;
using MyDream.Api.Models;

namespace MyDream.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Moto> Motos => Set<Moto>();
    public DbSet<Condutor> Condutores => Set<Condutor>();
    public DbSet<Locacao> Locacoes => Set<Locacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Moto>(m =>
        {
            m.HasIndex(x => x.Placa).IsUnique();
            // Índice composto que sustenta a query de "disponíveis" com paginação por keyset.
            m.HasIndex(x => new { x.Status, x.Id });
            m.Property(x => x.ValorDiaria).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Condutor>(c =>
        {
            c.HasIndex(x => x.Cnh).IsUnique();
        });

        modelBuilder.Entity<Locacao>(l =>
        {
            l.Property(x => x.ValorTotal).HasPrecision(10, 2);
            l.HasOne(x => x.Moto).WithMany().HasForeignKey(x => x.MotoId);
            l.HasOne(x => x.Condutor).WithMany().HasForeignKey(x => x.CondutorId);
        });
    }
}
