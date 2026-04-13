using System.Collections.Concurrent;

namespace DeepArchiveBridge.API.Middleware;

/// <summary>
/// Middleware de rate limiting para proteção contra força bruta e DDoS
/// Implementa limite de requisições por IP
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly int _requestsPerMinute;
    private readonly ConcurrentDictionary<string, (int count, DateTime resetTime)> _requests;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, int requestsPerMinute = 100)
    {
        _next = next;
        _logger = logger;
        _requestsPerMinute = requestsPerMinute;
        _requests = new ConcurrentDictionary<string, (int, DateTime)>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTime.UtcNow;

        // Limpar IP antigo se expirado
        if (_requests.TryGetValue(clientIp, out var requestData))
        {
            if (now >= requestData.resetTime)
            {
                _requests.TryUpdate(clientIp, (1, now.AddMinutes(1)), requestData);
            }
            else if (requestData.count >= _requestsPerMinute)
            {
                _logger.LogWarning("Rate limit excedido para IP: {ClientIp}", clientIp);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Rate limit excedido. Tente novamente mais tarde.");
                return;
            }
            else
            {
                _requests.TryUpdate(clientIp, (requestData.count + 1, requestData.resetTime), requestData);
            }
        }
        else
        {
            _requests.TryAdd(clientIp, (1, now.AddMinutes(1)));
        }

        await _next(context);
    }
}

public static class RateLimitingExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app, int requestsPerMinute = 100)
    {
        return app.UseMiddleware<RateLimitingMiddleware>(requestsPerMinute);
    }
}
