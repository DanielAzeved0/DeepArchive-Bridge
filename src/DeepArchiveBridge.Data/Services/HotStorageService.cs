using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Core.Models;
using DeepArchiveBridge.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DeepArchiveBridge.Data.Services;

/// <summary>
/// Serviço para acesso ao armazenamento "Hot" (PostgreSQL)
/// </summary>
public class HotStorageService : IHotStorageService
{
    private readonly VendaDbContext _context;

    public HotStorageService(VendaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Venda>> BuscarVendasAsync(BuscaVendaRequest request)
    {
        var query = _context.Vendas.AsQueryable();

        // Filtros
        query = query.Where(v => v.DataVenda >= request.DataInicio && v.DataVenda <= request.DataFim);

        if (!string.IsNullOrEmpty(request.ClienteId))
            query = query.Where(v => v.ClienteId == request.ClienteId);

        if (request.Status.HasValue)
            query = query.Where(v => v.Status == request.Status);

        // Paginação
        var vendas = await query
            .Include(v => v.Itens)
            .OrderByDescending(v => v.DataVenda)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync();

        return vendas;
    }

    public async Task<Venda?> BuscarVendaPorIdAsync(int id)
    {
        return await _context.Vendas
            .Include(v => v.Itens)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<int> CriarVendaAsync(Venda venda)
    {
        venda.DataCriacao = DateTime.UtcNow;
        _context.Vendas.Add(venda);
        await _context.SaveChangesAsync();
        return venda.Id;
    }

    public async Task AtualizarVendaAsync(Venda venda)
    {
        venda.DataAtualizacao = DateTime.UtcNow;
        _context.Vendas.Update(venda);
        await _context.SaveChangesAsync();
    }

    public async Task DeletarVendaAsync(int id)
    {
        var venda = await _context.Vendas.FindAsync(id);
        if (venda != null)
        {
            _context.Vendas.Remove(venda);
            await _context.SaveChangesAsync();
        }
    }
}
