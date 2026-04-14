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

    public ArquivamentoController(
        IArchivingService archivingService, 
        ILogger<ArquivamentoController> logger)
    {
        _archivingService = archivingService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém informações sobre dados que serão arquivados
    /// </summary>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ArquivamentoInfo>>> ObterInfo(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obtendo informações de arquivamento");

        var info = await _archivingService.ObterInfoArquivamento();

        return Ok(new ApiResponse<ArquivamentoInfo>
        {
            Sucesso = true,
            Dados = info,
            Mensagem = info.Mensagem
        });
    }

    /// <summary>
    /// Executa o arquivamento com confirmação prévia
    /// </summary>
    [HttpPost("executar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ResultadoArquivamento>>> Executar(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando arquivamento de dados com confirmação");

        var resultado = await _archivingService.ArquivarComConfirmacao();

        return Ok(new ApiResponse<ResultadoArquivamento>
        {
            Sucesso = resultado.Sucesso,
            Dados = resultado,
            Mensagem = resultado.Mensagem
        });
    }

    /// <summary>
    /// Executa o arquivamento automático sem confirmação (para agendamento)
    /// </summary>
    [HttpPost("executar-automatico")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<int>>> ExecutarAutomatico(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando arquivamento automático (sem confirmação)");

        var quantidadeArquivada = await _archivingService.ArquivarDadosAntigos();

        return Ok(new ApiResponse<int>
        {
            Sucesso = true,
            Dados = quantidadeArquivada,
            Mensagem = $"Arquivamento concluído: {quantidadeArquivada} vendas movidas para Cold Storage"
        });
    }
}
