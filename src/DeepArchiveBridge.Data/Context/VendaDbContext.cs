using Microsoft.EntityFrameworkCore;
using DeepArchiveBridge.Core.Models;

namespace DeepArchiveBridge.Data.Context;

/// <summary>
/// DbContext para armazenamento "Hot" em PostgreSQL
/// Armazena dados de 0 a 90 dias
/// </summary>
public class VendaDbContext : DbContext
{
    public VendaDbContext(DbContextOptions<VendaDbContext> options) : base(options)
    {
    }

    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<VendaItem> VendaItems => Set<VendaItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração de Venda
        modelBuilder.Entity<Venda>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ClienteId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ClienteNome)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Valor)
                .HasPrecision(18, 2);

            entity.Property(e => e.DataVenda)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasConversion<int>();

            entity.HasMany(e => e.Itens)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.DataVenda);
            entity.HasIndex(e => e.ClienteId);
            entity.HasIndex(e => e.Status);
        });

        // Configuração de VendaItem
        modelBuilder.Entity<VendaItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Produto)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Quantidade)
                .HasPrecision(12, 2);

            entity.Property(e => e.PrecoUnitario)
                .HasPrecision(18, 2);
        });
    }
}
