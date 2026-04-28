using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Core.Models;
using DeepArchiveBridge.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DeepArchiveBridge.Data.Services;

/// <summary>
/// Serviço responsável por identificar dados antigos (>90 dias).
/// A implementação atual usa SQLite como armazenamento unificado, então o
/// arquivamento valida a elegibilidade sem remover registros da base ativa.
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

            // Dados totais - trazer para memória para contornar limitação do SQLite com Sum() em decimais
            var todasVendas = await _hotContext.Vendas.ToListAsync();
            var totalVendas = todasVendas.Count;
            var valorTotal = todasVendas.Sum(v => v.Valor);

            // Dados para arquivar
            var vendasParaArquivarList = await _hotContext.Vendas
                .Where(v => v.DataVenda < dataLimite)
                .ToListAsync();

            var vendasParaArquivar = vendasParaArquivarList.Count;
            var valorParaArquivar = vendasParaArquivarList.Sum(v => v.Valor);

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
    /// Mantém os dados no SQLite unificado para evitar perda de registros.
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

            _logger.LogInformation($"Validando {vendasParaArquivar.Count} vendas elegíveis para Cold Storage");

            // No modo SQLite unificado, esta chamada garante que os registros estejam acessíveis
            // pelo serviço de Cold Storage sem remover a origem.
            await _coldStorage.SalvarVendasAsync(vendasParaArquivar, DateTime.UtcNow);

            _logger.LogInformation($"Arquivamento validado: {vendasParaArquivar.Count} vendas disponíveis no SQLite unificado");

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

            // No modo SQLite unificado, esta chamada garante acesso pelo Cold Storage
            // sem remover registros da base ativa.
            await _coldStorage.SalvarVendasAsync(vendasParaArquivar, DateTime.UtcNow);

            stopwatch.Stop();

            resultado.Sucesso = true;
            resultado.VendasArquivadas = vendasParaArquivar.Count;
            resultado.ItensArquivados = totalItens;
            resultado.ArquivoNome = $"archive_{DateTime.UtcNow:yyyyMMdd_HHmmss}.sql";
            resultado.Duracao = stopwatch.Elapsed;
            resultado.Mensagem = $"Arquivamento validado: {vendasParaArquivar.Count} vendas e {totalItens} itens disponíveis no SQLite unificado";
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
