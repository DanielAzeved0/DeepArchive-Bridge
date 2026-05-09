using DeepArchiveBridge.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DeepArchiveBridge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly IEnumerable<IDependencyHealthCheck> _dependencyChecks;
    private static readonly DateTime ApplicationStartTime = DateTime.UtcNow;

    public HealthController(
        ILogger<HealthController> logger,
        IEnumerable<IDependencyHealthCheck> dependencyChecks)
    {
        _logger = logger;
        _dependencyChecks = dependencyChecks;
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ApiResponse<HealthStatus>>> GetHealth()
    {
        var stopwatch = Stopwatch.StartNew();

        var uptime = (long)(DateTime.UtcNow - ApplicationStartTime).TotalSeconds;
        var memoryMB = GC.GetTotalMemory(false) / (1024 * 1024);
        var dependencyResults = await Task.WhenAll(_dependencyChecks.Select(check => check.CheckAsync()));
        var dependenciesUnhealthy = dependencyResults.Count(result => !result.IsHealthy);
        var status = dependenciesUnhealthy == 0 ? "Healthy" : "Degraded";

        stopwatch.Stop();

        var health = new HealthStatus
        {
            Status = status,
            Timestamp = DateTime.UtcNow,
            ApiVersion = "2.0",
            Uptime = uptime,
            MemoryMB = memoryMB,
            CheckDurationMs = stopwatch.ElapsedMilliseconds,
            DependenciesHealthy = dependencyResults.Length - dependenciesUnhealthy,
            DependenciesUnhealthy = dependenciesUnhealthy
        };

        _logger.LogInformation(
            "Health check {Status}: {Healthy}/{Total} dependencies healthy in {ElapsedMs}ms",
            status,
            health.DependenciesHealthy,
            dependencyResults.Length,
            stopwatch.ElapsedMilliseconds);

        var httpStatus = dependenciesUnhealthy == 0
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;

        return StatusCode(httpStatus, new ApiResponse<HealthStatus>
        {
            Sucesso = dependenciesUnhealthy == 0,
            Mensagem = $"API {status}",
            Dados = health
        });
    }

    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping()
    {
        return Ok(new { status = "pong", timestamp = DateTime.UtcNow });
    }
}

public class HealthStatus
{
    public required string Status { get; set; }
    public DateTime Timestamp { get; set; }
    public required string ApiVersion { get; set; }
    public long Uptime { get; set; }
    public long MemoryMB { get; set; }
    public long CheckDurationMs { get; set; }
    public int DependenciesHealthy { get; set; }
    public int DependenciesUnhealthy { get; set; }
}
