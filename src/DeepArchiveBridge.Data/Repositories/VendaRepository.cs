using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Core.Models;
using DeepArchiveBridge.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DeepArchiveBridge.Data.Repositories;

/// <summary>
/// Repository unificado para SQLite (Cold Storage)
/// Gerencia todas as vendas arquivadas
/// </summary>
public class VendaRepository : IVendaRepository
{
    private readonly VendaDbContext _context;

    public VendaRepository(VendaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Venda>> BuscarAsync(
        BuscaVendaRequest request, 
        EstrategiaArmazenamento estrategia = EstrategiaArmazenamento.Auto,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Vendas.AsNoTracking();

        // Filtro por data
        query = query.Where(v => v.DataVenda >= request.DataInicio && v.DataVenda <= request.DataFim);

        // Filtro por cliente (opcional)
        if (!string.IsNullOrEmpty(request.ClienteId))
        {
            query = query.Where(v => v.ClienteId == request.ClienteId);
        }

        // Filtro por status (opcional)
        if (request.Status.HasValue)
        {
            query = query.Where(v => v.Status == request.Status);
        }

        // Ordenação e paginação
        var vendas = await query
            .Include(v => v.Itens)
            .OrderByDescending(v => v.DataVenda)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        return vendas;
    }

    public async Task<Venda?> BuscarPorIdAsync(
        int id, 
        EstrategiaArmazenamento estrategia = EstrategiaArmazenamento.Auto,
        CancellationToken cancellationToken = default)
    {
        return await _context.Vendas
            .AsNoTracking()
            .Include(v => v.Itens)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<int> CriarAsync(Venda venda, CancellationToken cancellationToken = default)
    {
        venda.DataCriacao = DateTime.UtcNow;
        _context.Vendas.Add(venda);
        await _context.SaveChangesAsync(cancellationToken);
        return venda.Id;
    }

    public async Task AtualizarAsync(Venda venda, CancellationToken cancellationToken = default)
    {
        venda.DataAtualizacao = DateTime.UtcNow;
        _context.Vendas.Update(venda);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletarAsync(int id, CancellationToken cancellationToken = default)
    {
        var venda = await _context.Vendas.FindAsync(
            new object[] { id }, 
            cancellationToken: cancellationToken);
        
        if (venda != null)
        {
            _context.Vendas.Remove(venda);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
