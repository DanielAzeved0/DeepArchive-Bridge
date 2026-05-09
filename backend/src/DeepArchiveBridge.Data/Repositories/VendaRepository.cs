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

    public async Task<VendaNavigationResponse> BuscarNavegacaoAsync(int id, CancellationToken cancellationToken = default)
    {
        var vendaAtual = await _context.Vendas
            .AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v => new { v.Id, v.DataVenda })
            .FirstOrDefaultAsync(cancellationToken);

        if (vendaAtual == null)
        {
            return new VendaNavigationResponse { VendaId = id };
        }

        var anteriorId = await _context.Vendas
            .AsNoTracking()
            .Where(v => v.DataVenda < vendaAtual.DataVenda || (v.DataVenda == vendaAtual.DataVenda && v.Id < vendaAtual.Id))
            .OrderByDescending(v => v.DataVenda)
            .ThenByDescending(v => v.Id)
            .Select(v => (int?)v.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var proximaId = await _context.Vendas
            .AsNoTracking()
            .Where(v => v.DataVenda > vendaAtual.DataVenda || (v.DataVenda == vendaAtual.DataVenda && v.Id > vendaAtual.Id))
            .OrderBy(v => v.DataVenda)
            .ThenBy(v => v.Id)
            .Select(v => (int?)v.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return new VendaNavigationResponse
        {
            VendaId = id,
            AnteriorId = anteriorId,
            ProximaId = proximaId
        };
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
        var existing = await _context.Vendas
            .Include(v => v.Itens)
            .FirstOrDefaultAsync(v => v.Id == venda.Id, cancellationToken);

        if (existing == null)
        {
            return;
        }

        existing.ClienteId = venda.ClienteId;
        existing.ClienteNome = venda.ClienteNome;
        existing.Valor = venda.Valor;
        existing.DataVenda = venda.DataVenda;
        existing.Status = venda.Status;
        existing.DataAtualizacao = DateTime.UtcNow;

        var incomingIds = venda.Itens
            .Where(i => i.Id > 0)
            .Select(i => i.Id)
            .ToHashSet();

        var itemsToRemove = existing.Itens
            .Where(i => i.Id > 0 && !incomingIds.Contains(i.Id))
            .ToList();

        foreach (var item in itemsToRemove)
        {
            existing.Itens.Remove(item);
        }

        foreach (var item in venda.Itens)
        {
            var existingItem = item.Id > 0
                ? existing.Itens.FirstOrDefault(i => i.Id == item.Id)
                : null;

            if (existingItem == null)
            {
                existing.Itens.Add(new VendaItem
                {
                    Produto = item.Produto,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = item.PrecoUnitario
                });
                continue;
            }

            existingItem.Produto = item.Produto;
            existingItem.Quantidade = item.Quantidade;
            existingItem.PrecoUnitario = item.PrecoUnitario;
        }

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
