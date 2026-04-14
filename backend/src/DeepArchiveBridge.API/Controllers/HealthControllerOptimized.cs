using DeepArchiveBridge.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DeepArchiveBridge.API.Controllers;

/// <summary>
/// Interface para verificações de saúde de dependências
/// </summary>
public interface IDependencyHealthCheck
{
    Task<(bool IsHealthy, string Details)> CheckAsync();
}

/// <summary>
/// Controller para verificação de saúde da API (otimizado)
/// Implementa:
/// - Health checks de dependências (DB, Storage)
/// - Cache de resultados (reduz GC e I/O)
/// - Métricas de performance sem GC forçado
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.AllowAnonymous]
public class HealthControllerOptimized : ControllerBase
{
    private readonly ILogger<HealthControllerOptimized> _logger;
    private readonly IEnumerable<IDependencyHealthCheck> _dependencyChecks;
    private static readonly DateTime ApplicationStartTime = DateTime.UtcNow;
    
    // Cache: resultado anterior + timestamp
    private static (HealthStatus Status, DateTime CachedAt)? _cachedHealth;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(5);
    private static readonly object CacheLock = new object();

    public HealthControllerOptimized(
        ILogger<HealthControllerOptimized> logger,
        IEnumerable<IDependencyHealthCheck> dependencyChecks)
    {
        _logger = logger;
        _dependencyChecks = dependencyChecks ?? Enumerable.Empty<IDependencyHealthCheck>();
    }

    /// <summary>
    /// Obtém o status de saúde da API com cache (5s)
    /// </summary>
    /// <returns>Status de saúde com timestamp, versão e métricas</returns>
    [HttpGet]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ApiResponse<HealthStatus>>> GetHealth()
    {
        // Verificar cache
        lock (CacheLock)
        {
            if (_cachedHealth.HasValue && 
                DateTime.UtcNow - _cachedHealth.Value.CachedAt < CacheDuration)
            {
                _logger.LogDebug("Retornando health check do cache (age: {Age}ms)", 
                    (DateTime.UtcNow - _cachedHealth.Value.CachedAt).TotalMilliseconds);
                
                return Ok(new ApiResponse<HealthStatus>
                {
                    Sucesso = true,
                    Mensagem = "API está operacional (cached)",
                    Dados = _cachedHealth.Value.Status
                });
            }
        }

        var sw = Stopwatch.StartNew();
        
        try
        {
            // Calcular métricas SEM forçar garbage collection
            var uptime = (long)(DateTime.UtcNow - ApplicationStartTime).TotalSeconds;
            var memoryMB = GC.GetTotalMemory(false) / (1024 * 1024); // false = não coleta

            // Verificar saúde de dependências (paralelo)
            var depCheckTasks = _dependencyChecks
                .Select(async check => await check.CheckAsync())
                .ToList();

            var dependencyResults = await Task.WhenAll(depCheckTasks);
            var allDependenciesHealthy = dependencyResults.All(r => r.IsHealthy);

            sw.Stop();

            var overallStatus = allDependenciesHealthy ? "Healthy" : "Degraded";
            var httpStatus = allDependenciesHealthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;

            var health = new HealthStatus
            {
                Status = overallStatus,
                Timestamp = DateTime.UtcNow,
                ApiVersion = "2.0",
                Uptime = uptime,
                MemoryMB = memoryMB,
                CheckDurationMs = sw.ElapsedMilliseconds,
                DependenciesHealthy = dependencyResults.Length,
                DependenciesUnhealthy = dependencyResults.Count(r => !r.IsHealthy)
            };

            // Cachear resultado
            lock (CacheLock)
            {
                _cachedHealth = (health, DateTime.UtcNow);
            }

            _logger.LogInformation(
                "Health check: {Status} | Memory: {MemoryMB}MB | Deps: {Healthy}/{Total} healthy | Duration: {DurationMs}ms",
                health.Status, health.MemoryMB, health.DependenciesHealthy, 
                dependencyResults.Length, sw.ElapsedMilliseconds);

            return StatusCode(httpStatus, new ApiResponse<HealthStatus>
            {
                Sucesso = allDependenciesHealthy,
                Mensagem = $"API {overallStatus}",
                Dados = health
            });
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Erro ao verificar saúde da API: {Message}", ex.Message);

            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new ApiResponse<HealthStatus>
                {
                    Sucesso = false,
                    Mensagem = "Erro ao verificar saúde - verifique logs"
                });
        }
    }

    /// <summary>
    /// Verifica apenas se a API está respondendo (health check simples, sem cache)
    /// Latência: ~1-2ms
    /// </summary>
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping()
    {
        return Ok(new { status = "pong", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Limpar cache manualmente (útil para testes)
    /// </summary>
    [HttpPost("cache/clear")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public IActionResult ClearCache()
    {
        lock (CacheLock)
        {
            _cachedHealth = null;
        }
        return Ok(new { message = "Cache limpo com sucesso" });
    }
}

/// <summary>
/// Modelo de saúde atualizado com métricas de performance
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
    /// Tempo desde o início da aplicação em segundos
    /// </summary>
    public long Uptime { get; set; }

    /// <summary>
    /// Uso de memória em MB (sem forçar GC)
    /// </summary>
    public long MemoryMB { get; set; }

    /// <summary>
    /// Tempo gasto na verificação de saúde (ms)
    /// </summary>
    public long CheckDurationMs { get; set; }

    /// <summary>
    /// Quantidade de dependências saudáveis
    /// </summary>
    public int DependenciesHealthy { get; set; }

    /// <summary>
    /// Quantidade de dependências com problemas
    /// </summary>
    public int DependenciesUnhealthy { get; set; }
}
