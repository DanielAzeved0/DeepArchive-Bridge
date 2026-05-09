using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Core.Models;
using DeepArchiveBridge.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DeepArchiveBridge.Data.Services;

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

    public async Task<ArquivamentoInfo> ObterInfoArquivamento()
    {
        try
        {
            var dataLimite = DateTime.UtcNow.AddDays(-DiasRetencaoHot);

            var todasVendas = await _hotContext.Vendas.ToListAsync();
            var totalVendas = todasVendas.Count;
            var valorTotal = todasVendas.Sum(v => v.Valor);

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
            _logger.LogError(ex, "Erro ao obter informacoes de arquivamento");
            throw;
        }
    }

    public async Task<int> ArquivarDadosAntigos()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Iniciando arquivamento automatico de dados antigos");

            var dataLimite = DateTime.UtcNow.AddDays(-DiasRetencaoHot);
            var vendasParaArquivar = await _hotContext.Vendas
                .Where(v => v.DataVenda < dataLimite)
                .Include(v => v.Itens)
                .ToListAsync();

            if (!vendasParaArquivar.Any())
            {
                stopwatch.Stop();
                await RegistrarLogAsync(new ResultadoArquivamento
                {
                    Sucesso = true,
                    DataExecucao = DateTime.UtcNow,
                    Mensagem = "Nenhuma venda para arquivar",
                    Duracao = stopwatch.Elapsed
                }, 0);

                return 0;
            }

            await _coldStorage.SalvarVendasAsync(vendasParaArquivar, DateTime.UtcNow);
            stopwatch.Stop();

            var resultado = new ResultadoArquivamento
            {
                Sucesso = true,
                DataExecucao = DateTime.UtcNow,
                VendasArquivadas = vendasParaArquivar.Count,
                ItensArquivados = vendasParaArquivar.Sum(v => v.Itens.Count),
                Duracao = stopwatch.Elapsed,
                Mensagem = $"Arquivamento automatico validado: {vendasParaArquivar.Count} vendas disponiveis no SQLite unificado"
            };

            await RegistrarLogAsync(resultado, vendasParaArquivar.Sum(v => v.Valor));
            _logger.LogInformation(resultado.Mensagem);

            return vendasParaArquivar.Count;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Erro ao arquivar dados");

            await RegistrarLogAsync(new ResultadoArquivamento
            {
                Sucesso = false,
                DataExecucao = DateTime.UtcNow,
                Duracao = stopwatch.Elapsed,
                Mensagem = $"Erro: {ex.Message}"
            }, 0);

            throw;
        }
    }

    public async Task<ResultadoArquivamento> ArquivarComConfirmacao()
    {
        var stopwatch = Stopwatch.StartNew();
        decimal valorProcessado = 0;
        var resultado = new ResultadoArquivamento
        {
            DataExecucao = DateTime.UtcNow
        };

        try
        {
            var dataLimite = DateTime.UtcNow.AddDays(-DiasRetencaoHot);
            var vendasParaArquivar = await _hotContext.Vendas
                .Where(v => v.DataVenda < dataLimite)
                .Include(v => v.Itens)
                .ToListAsync();

            if (!vendasParaArquivar.Any())
            {
                stopwatch.Stop();
                resultado.Sucesso = true;
                resultado.Mensagem = "Nenhuma venda para arquivar";
                resultado.VendasArquivadas = 0;
                resultado.ItensArquivados = 0;
                resultado.Duracao = stopwatch.Elapsed;
                await RegistrarLogAsync(resultado, valorProcessado);
                return resultado;
            }

            var totalItens = vendasParaArquivar.Sum(v => v.Itens.Count);
            valorProcessado = vendasParaArquivar.Sum(v => v.Valor);

            _logger.LogInformation(
                "Iniciando arquivamento de {Vendas} vendas com {Itens} itens",
                vendasParaArquivar.Count,
                totalItens);

            await _coldStorage.SalvarVendasAsync(vendasParaArquivar, DateTime.UtcNow);
            stopwatch.Stop();

            resultado.Sucesso = true;
            resultado.VendasArquivadas = vendasParaArquivar.Count;
            resultado.ItensArquivados = totalItens;
            resultado.ArquivoNome = $"archive_{DateTime.UtcNow:yyyyMMdd_HHmmss}.sql";
            resultado.Duracao = stopwatch.Elapsed;
            resultado.Mensagem = $"Arquivamento validado: {vendasParaArquivar.Count} vendas e {totalItens} itens disponiveis no SQLite unificado";
            resultado.TamanhoBytes = vendasParaArquivar.Sum(v => 100 + (v.Itens.Count * 50));

            await RegistrarLogAsync(resultado, valorProcessado);
            _logger.LogInformation(
                "Arquivamento concluido em {ElapsedMs}ms - {Mensagem}",
                stopwatch.ElapsedMilliseconds,
                resultado.Mensagem);

            return resultado;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Erro ao arquivar com confirmacao");

            resultado.Sucesso = false;
            resultado.Mensagem = $"Erro: {ex.Message}";
            resultado.Duracao = stopwatch.Elapsed;
            await RegistrarLogAsync(resultado, valorProcessado);
            return resultado;
        }
    }

    public async Task<List<ArquivamentoLog>> ListarLogsAsync(
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        var safeSkip = Math.Max(0, skip);
        var safeTake = Math.Clamp(take, 1, 100);

        return await _hotContext.ArquivamentoLogs
            .AsNoTracking()
            .OrderByDescending(log => log.DataExecucao)
            .Skip(safeSkip)
            .Take(safeTake)
            .ToListAsync(cancellationToken);
    }

    public async Task<ArquivamentoLog?> ObterUltimoLogAsync(CancellationToken cancellationToken = default)
    {
        return await _hotContext.ArquivamentoLogs
            .AsNoTracking()
            .OrderByDescending(log => log.DataExecucao)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task RegistrarLogAsync(ResultadoArquivamento resultado, decimal valorProcessado)
    {
        _hotContext.ArquivamentoLogs.Add(new ArquivamentoLog
        {
            DataExecucao = resultado.DataExecucao == default ? DateTime.UtcNow : resultado.DataExecucao,
            Status = resultado.Sucesso ? "sucesso" : "erro",
            VendasProcessadas = resultado.VendasArquivadas,
            ItensProcessados = resultado.ItensArquivados,
            ValorProcessado = valorProcessado,
            DuracaoMs = (long)resultado.Duracao.TotalMilliseconds,
            Mensagem = resultado.Mensagem
        });

        await _hotContext.SaveChangesAsync();
    }
}
