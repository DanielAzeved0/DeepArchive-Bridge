using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DeepArchiveBridge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArquivamentoController : ControllerBase
{
    private readonly IArchivingService _archivingService;
    private readonly ILogger<ArquivamentoController> _logger;

    public ArquivamentoController(IArchivingService archivingService, ILogger<ArquivamentoController> logger)
    {
        _archivingService = archivingService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém informações sobre dados que serão arquivados
    /// </summary>
    [HttpGet("info")]
    public async Task<ActionResult<ApiResponse<ArquivamentoInfo>>> ObterInfo()
    {
        try
        {
            var info = await _archivingService.ObterInfoArquivamento();

            return Ok(new ApiResponse<ArquivamentoInfo>
            {
                Sucesso = true,
                Dados = info,
                Mensagem = info.Mensagem
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter informações de arquivamento");
            return BadRequest(new ApiResponse<ArquivamentoInfo>
            {
                Sucesso = false,
                Mensagem = ex.Message
            });
        }
    }

    /// <summary>
    /// Executa o arquivamento automático de dados com mais de 90 dias
    /// </summary>
    [HttpPost("executar")]
    public async Task<ActionResult<ApiResponse<ResultadoArquivamento>>> Executar()
    {
        try
        {
            _logger.LogInformation("Iniciando arquivamento de dados");

            var resultado = await _archivingService.ArquivarComConfirmacao();

            return Ok(new ApiResponse<ResultadoArquivamento>
            {
                Sucesso = resultado.Sucesso,
                Dados = resultado,
                Mensagem = resultado.Mensagem
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar arquivamento");
            return BadRequest(new ApiResponse<ResultadoArquivamento>
            {
                Sucesso = false,
                Mensagem = ex.Message
            });
        }
    }

    /// <summary>
    /// Executa o arquivamento automático sem confirmação (para agendamento)
    /// </summary>
    [HttpPost("executar-automatico")]
    public async Task<ActionResult<ApiResponse<int>>> ExecutarAutomatico()
    {
        try
        {
            _logger.LogInformation("Iniciando arquivamento automático");

            var quantidadeArquivada = await _archivingService.ArquivarDadosAntigos();

            return Ok(new ApiResponse<int>
            {
                Sucesso = true,
                Dados = quantidadeArquivada,
                Mensagem = $"Arquivamento concluído: {quantidadeArquivada} vendas movidas para Cold Storage"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar arquivamento automático");
            return BadRequest(new ApiResponse<int>
            {
                Sucesso = false,
                Mensagem = ex.Message
            });
        }
    }
}
