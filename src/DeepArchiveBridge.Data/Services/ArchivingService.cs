using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Core.Models;
using DeepArchiveBridge.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DeepArchiveBridge.Data.Services;

/// <summary>
/// Serviço responsável por arquivar automaticamente dados antigos (>90 dias)
/// Move dados do PostgreSQL (Hot) para SQLite (Cold)
/// </summary>
public class ArchivingService : IArchivingService
{
    private readonly VendaDbContext _hotContext;
    private readonly IColdStorageService _coldStorage;
    private readonly ILogger<ArchivingService> _logger;
    private const int DiasRetencaoHot = 90;

    public ArchivingService(
        VendaDbContext hotContext, 
        IColdStorageService coldStorage,
        ILogger<ArchivingService> logger)
    {
        _hotContext = hotContext;
        _coldStorage = coldStorage;
        _logger = logger;
    }

    /// <summary>
    /// Obtém informações sobre dados que serão arquivados
    /// </summary>
    public async Task<ArquivamentoInfo> ObterInfoArquivamento()
    {
        try
        {
            var dataLimite = DateTime.UtcNow.AddDays(-DiasRetencaoHot);

            // Dados totais
            var totalVendas = await _hotContext.Vendas.CountAsync();
            var valorTotal = await _hotContext.Vendas.SumAsync(v => (decimal?)v.Valor) ?? 0m;

            // Dados para arquivar
            var vendasParaArquivar = await _hotContext.Vendas
                .Where(v => v.DataVenda < dataLimite)
                .CountAsync();

            var valorParaArquivar = await _hotContext.Vendas
                .Where(v => v.DataVenda < dataLimite)
                .SumAsync(v => (decimal?)v.Valor) ?? 0m;

            var dataMaisAntiga = await _hotContext.Vendas
                .OrderBy(v => v.DataVenda)
                .Select(v => v.DataVenda)
                .FirstOrDefaultAsync();

            var info = new ArquivamentoInfo
            {
                TotalVendas = totalVendas,
                VendasParaArquivar = vendasParaArquivar,
                ValorTotal = valorTotal,
                ValorAArquivar = valorParaArquivar,
                DataMaisAntiga = dataMaisAntiga,
                DataLimite = dataLimite,
                Mensagem = $"Encontradas {vendasParaArquivar} vendas para arquivar (de {totalVendas} total)"
            };

            _logger.LogInformation(info.Mensagem);
            return info;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter informações de arquivamento");
            throw;
        }
    }

    /// <summary>
    /// Arquiva automaticamente dados com mais de 90 dias
    /// Move do Hot Storage (PostgreSQL) para Cold Storage (SQLite)
    /// </summary>
    public async Task<int> ArquivarDadosAntigos()
    {
        try
        {
            _logger.LogInformation("Iniciando arquivamento automático de dados antigos");

            var dataLimite = DateTime.UtcNow.AddDays(-DiasRetencaoHot);

            // Identifica vendas para arquivar
            var vendasParaArquivar = await _hotContext.Vendas
                .Where(v => v.DataVenda < dataLimite)
                .Include(v => v.Itens)
                .ToListAsync();

            if (!vendasParaArquivar.Any())
            {
                _logger.LogInformation("Nenhuma venda para arquivar");
                return 0;
            }

            _logger.LogInformation($"Arquivando {vendasParaArquivar.Count} vendas para Cold Storage");

            // Salva em SQLite (Cold Storage)
            await _coldStorage.SalvarVendasAsync(vendasParaArquivar, DateTime.UtcNow);

            // Remove do banco "quente" (PostgreSQL)
            _hotContext.Vendas.RemoveRange(vendasParaArquivar);
            await _hotContext.SaveChangesAsync();

            _logger.LogInformation($"Arquivamento concluído: {vendasParaArquivar.Count} vendas movidas para Cold Storage");

            return vendasParaArquivar.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao arquivar dados");
            throw;
        }
    }

    /// <summary>
    /// Arquivamento com confirmação manual
    /// </summary>
    public async Task<ResultadoArquivamento> ArquivarComConfirmacao()
    {
        var stopwatch = Stopwatch.StartNew();
        var resultado = new ResultadoArquivamento
        {
            DataExecucao = DateTime.UtcNow
        };

        try
        {
            var dataLimite = DateTime.UtcNow.AddDays(-DiasRetencaoHot);

            // Identifica vendas para arquivar
            var vendasParaArquivar = await _hotContext.Vendas
                .Where(v => v.DataVenda < dataLimite)
                .Include(v => v.Itens)
                .ToListAsync();

            if (!vendasParaArquivar.Any())
            {
                resultado.Sucesso = true;
                resultado.Mensagem = "Nenhuma venda para arquivar";
                resultado.VendasArquivadas = 0;
                resultado.ItensArquivados = 0;
                return resultado;
            }

            // Conta itens
            var totalItens = vendasParaArquivar.Sum(v => v.Itens.Count);

            _logger.LogInformation($"Iniciando arquivamento de {vendasParaArquivar.Count} vendas com {totalItens} itens");

            // Salva em SQLite (Cold Storage)
            await _coldStorage.SalvarVendasAsync(vendasParaArquivar, DateTime.UtcNow);

            // Remove do banco "quente"
            _hotContext.Vendas.RemoveRange(vendasParaArquivar);
            await _hotContext.SaveChangesAsync();

            stopwatch.Stop();

            resultado.Sucesso = true;
            resultado.VendasArquivadas = vendasParaArquivar.Count;
            resultado.ItensArquivados = totalItens;
            resultado.ArquivoNome = $"archive_{DateTime.UtcNow:yyyyMMdd_HHmmss}.sql";
            resultado.Duracao = stopwatch.Elapsed;
            resultado.Mensagem = $"Arquivamento concluído: {vendasParaArquivar.Count} vendas e {totalItens} itens movidos para Cold Storage (SQLite)";
            resultado.TamanhoBytes = vendasParaArquivar.Sum(v => 100 + (v.Itens.Count * 50)); // Estimativa

            _logger.LogInformation($"Arquivamento concluído em {stopwatch.ElapsedMilliseconds}ms - {resultado.Mensagem}");

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao arquivar com confirmação");
            stopwatch.Stop();
            resultado.Sucesso = false;
            resultado.Mensagem = $"Erro: {ex.Message}";
            resultado.Duracao = stopwatch.Elapsed;
            return resultado;
        }
    }
}
