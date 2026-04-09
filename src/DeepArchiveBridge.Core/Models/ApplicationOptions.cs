namespace DeepArchiveBridge.Core.Models;

/// <summary>
/// Opções de configuração para o serviço de arquivamento
/// Implementa o padrão Options do .NET
/// </summary>
public class ArchivingOptions
{
    /// <summary>
    /// Número de dias para reter dados no Hot Storage (padrão: 90)
    /// </summary>
    public int RetentionDaysHot { get; set; } = 90;

    /// <summary>
    /// Tamanho de página padrão (padrão: 100)
    /// </summary>
    public int DefaultPageSize { get; set; } = 100;

    /// <summary>
    /// Tamanho de página máximo (padrão: 500)
    /// </summary>
    public int MaxPageSize { get; set; } = 500;

    /// <summary>
    /// Timeout para operações de banco de dados em segundos (padrão: 30)
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
}

/// <summary>
/// Opções de configuração para logging
/// </summary>
public class LoggingOptions
{
    /// <summary>
    /// Habilitar logging estruturado em JSON
    /// </summary>
    public bool UseStructuredLogging { get; set; } = true;

    /// <summary>
    /// Habilitar logging de requisições HTTP
    /// </summary>
    public bool LogHttpRequests { get; set; } = true;

    /// <summary>
    /// Nível de log mínimo para armazanar em BD (Default: Information)
    /// </summary>
    public string MinimumLogLevel { get; set; } = "Information";
}

/// <summary>
/// Opções de configuração da API
/// </summary>
public class ApiOptions
{
    /// <summary>
    /// Habilitar CORS
    /// </summary>
    public bool EnableCors { get; set; } = false;

    /// <summary>
    /// Origens permitidas para CORS (separadas por vírgula)
    /// </summary>
    public string AllowedOrigins { get; set; } = "http://localhost:3000,http://localhost:5000";

    /// <summary>
    /// Habilitar Health Check
    /// </summary>
    public bool EnableHealthCheck { get; set; } = true;

    /// <summary>
    /// Versão da API
    /// </summary>
    public string ApiVersion { get; set; } = "1.0";
}
