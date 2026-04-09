using DeepArchiveBridge.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DeepArchiveBridge.API.Controllers;

/// <summary>
/// Controller para verificação de saúde da API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Obtém o status de saúde da API
    /// </summary>
    /// <returns>Status de saúde com timestamp e versão</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<HealthStatus>> GetHealth()
    {
        var health = new HealthStatus
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            ApiVersion = "1.0",
            Uptime = GC.GetTotalMemory(false) / (1024 * 1024)  // MB
        };

        return Ok(new ApiResponse<HealthStatus>
        {
            Sucesso = true,
            Mensagem = "API está operacional",
            Dados = health
        });
    }

    /// <summary>
    /// Verifica apenas se a API está respondendo (health check simples)
    /// </summary>
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping()
    {
        return Ok(new { status = "pong", timestamp = DateTime.UtcNow });
    }
}

/// <summary>
/// Modelo para informações de saúde da API
/// </summary>
public class HealthStatus
{
    /// <summary>
    /// Status da API (Healthy, Degraded, Unhealthy)
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Timestamp UTC quando a verificação foi feita
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Versão da API
    /// </summary>
    public required string ApiVersion { get; set; }

    /// <summary>
    /// Uso de memória em MB
    /// </summary>
    public long Uptime { get; set; }
}
