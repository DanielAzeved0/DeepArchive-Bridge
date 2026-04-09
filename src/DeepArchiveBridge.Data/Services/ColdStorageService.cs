using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Core.Models;
using DeepArchiveBridge.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeepArchiveBridge.Data.Services;

/// <summary>
/// Serviço para acesso ao armazenamento "Cold" em SQLite
/// Armazena dados com mais de 90 dias
/// </summary>
public class ColdStorageService : IColdStorageService
{
    private readonly VendaDbContext _context;
    private readonly ILogger<ColdStorageService> _logger;

    public ColdStorageService(VendaDbContext context, ILogger<ColdStorageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Busca vendas do SQLite em um intervalo de datas
    /// </summary>
    public async Task<List<Venda>> BuscarVendasAsync(DateTime dataInicio, DateTime dataFim, string? clienteId = null)
    {
        try
        {
            _logger.LogInformation($"Buscando vendas no Cold Storage entre {dataInicio:yyyy-MM-dd} e {dataFim:yyyy-MM-dd}");

            var query = _context.Vendas
                .AsNoTracking()
                .Where(v => v.DataVenda >= dataInicio && v.DataVenda <= dataFim);

            if (!string.IsNullOrEmpty(clienteId))
            {
                query = query.Where(v => v.ClienteId == clienteId);
            }

            var vendas = await query
                .Include(v => v.Itens)
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();

            _logger.LogInformation($"Encontradas {vendas.Count} vendas no Cold Storage");
            return vendas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas do Cold Storage");
            throw;
        }
    }

    /// <summary>
    /// Salva vendas no SQLite
    /// </summary>
    public async Task SalvarVendasAsync(List<Venda> vendas, DateTime mes)
    {
        try
        {
            if (!vendas.Any())
            {
                _logger.LogWarning("Nenhuma venda para salvar no Cold Storage");
                return;
            }

            _logger.LogInformation($"Salvando {vendas.Count} vendas no Cold Storage para {mes:yyyy-MM}");

            // Verificar se as vendas já existem no archive
            var idsExistentes = await _context.Vendas
                .Where(v => vendas.Select(x => x.Id).Contains(v.Id))
                .Select(v => v.Id)
                .ToListAsync();

            var vendasNovas = vendas.Where(v => !idsExistentes.Contains(v.Id)).ToList();

            if (vendasNovas.Any())
            {
                await _context.Vendas.AddRangeAsync(vendasNovas);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Salvas {vendasNovas.Count} novas vendas no Cold Storage");
            }
            else
            {
                _logger.LogInformation("Todas as vendas já existem no Cold Storage");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar vendas no Cold Storage");
            throw;
        }
    }

    /// <summary>
    /// Obtém estatísticas do Cold Storage
    /// </summary>
    public async Task<int> ObterTotalVendasArquivadas()
    {
        try
        {
            var total = await _context.Vendas.CountAsync();
            _logger.LogInformation($"Total de vendas no Cold Storage: {total}");
            return total;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter total de vendas do Cold Storage");
            throw;
        }
    }

    /// <summary>
    /// Verifica a saúde do Cold Storage
    /// </summary>
    public async Task<bool> VerificarConexao()
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            _logger.LogInformation("Conexão com Cold Storage OK");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao conectar ao Cold Storage");
            return false;
        }
    }
}

