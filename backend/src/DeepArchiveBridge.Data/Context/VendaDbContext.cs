using Microsoft.EntityFrameworkCore;
using DeepArchiveBridge.Core.Models;

namespace DeepArchiveBridge.Data.Context;

/// <summary>
/// DbContext unificado para SQLite (Cold/Archive Storage)
/// Armazena todos os dados de vendas em armazenamento frio
/// </summary>
public class VendaDbContext : DbContext
{
    public VendaDbContext(DbContextOptions<VendaDbContext> options) : base(options)
    {
    }

    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<VendaItem> VendaItems => Set<VendaItem>();
    public DbSet<ArquivamentoLog> ArquivamentoLogs => Set<ArquivamentoLog>();

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

        modelBuilder.Entity<ArquivamentoLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DataExecucao)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(32);

            entity.Property(e => e.ValorProcessado)
                .HasPrecision(18, 2);

            entity.Property(e => e.Mensagem)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasIndex(e => e.DataExecucao);
            entity.HasIndex(e => e.Status);
        });
    }
}
