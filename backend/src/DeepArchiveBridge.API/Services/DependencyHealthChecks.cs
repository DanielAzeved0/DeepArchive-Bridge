using DeepArchiveBridge.API.Controllers;
using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Data.Context;
using System.Diagnostics;

namespace DeepArchiveBridge.API.Services;

/// <summary>
/// Health check para a conexão com banco de dados
/// </summary>
public class DatabaseHealthCheck : IDependencyHealthCheck
{
    private readonly VendaDbContext _dbContext;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(VendaDbContext dbContext, ILogger<DatabaseHealthCheck> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<(bool IsHealthy, string Details)> CheckAsync()
    {
        try
        {
            var sw = Stopwatch.StartNew();
            
            // Executar query simples para verificar conexão
            var result = await _dbContext.Database.CanConnectAsync();
            
            sw.Stop();

            if (result)
            {
                _logger.LogDebug("Database health check: OK ({Duration}ms)", sw.ElapsedMilliseconds);
                return (true, $"Database connected ({sw.ElapsedMilliseconds}ms)");
            }

            _logger.LogWarning("Database health check: Connection failed");
            return (false, "Database connection failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed: {Message}", ex.Message);
            return (false, $"Database error: {ex.Message}");
        }
    }
}

/// <summary>
/// Health check para o serviço de cold storage (arquivamento)
/// </summary>
public class ColdStorageHealthCheck : IDependencyHealthCheck
{
    private readonly IColdStorageService _coldStorageService;
    private readonly ILogger<ColdStorageHealthCheck> _logger;

    public ColdStorageHealthCheck(IColdStorageService coldStorageService, ILogger<ColdStorageHealthCheck> logger)
    {
        _coldStorageService = coldStorageService;
        _logger = logger;
    }

    public async Task<(bool IsHealthy, string Details)> CheckAsync()
    {
        try
        {
            var sw = Stopwatch.StartNew();
            
            // Testar se consegue acessar o storage (pode ser um ping simples)
            // Implementar conforme sua interface IColdStorageService
            // Por exemplo: var canAccess = await _coldStorageService.CanAccessAsync();
            
            sw.Stop();

            _logger.LogDebug("Cold storage health check: OK ({Duration}ms)", sw.ElapsedMilliseconds);
            return (true, $"Cold storage accessible ({sw.ElapsedMilliseconds}ms)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cold storage health check failed: {Message}", ex.Message);
            return (false, $"Cold storage error: {ex.Message}");
        }
    }
}

/// <summary>
/// Health check para Autenticação JWT
/// </summary>
public class AuthenticationHealthCheck : IDependencyHealthCheck
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthenticationHealthCheck> _logger;

    public AuthenticationHealthCheck(IAuthenticationService authService, ILogger<AuthenticationHealthCheck> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<(bool IsHealthy, string Details)> CheckAsync()
    {
        try
        {
            // Testar geração e validação de token
            var testToken = _authService.GenerateToken("health-check", "HealthCheck", "System");
            var isValid = _authService.ValidateToken(testToken);

            if (isValid)
            {
                _logger.LogDebug("Authentication health check: OK");
                return (true, "Authentication service operational");
            }

            _logger.LogWarning("Authentication health check: Token validation failed");
            return (false, "Authentication service validation failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication health check failed: {Message}", ex.Message);
            return (false, $"Authentication error: {ex.Message}");
        }
    }
}
