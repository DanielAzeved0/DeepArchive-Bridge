using DeepArchiveBridge.Core.Exceptions;
using DeepArchiveBridge.Core.Models;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace DeepArchiveBridge.API.Middleware;

/// <summary>
/// Middleware global para tratamento de exceções
/// Mapeia exceções para respostas HTTP apropriadas
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção não tratada: {ExceptionType}", ex.GetType().Name);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Sucesso = false,
            Mensagem = "Erro ao processar a requisição"
        };

        switch (exception)
        {
            case NotFoundException notFound:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.Mensagem = notFound.Message;
                break;

            case ValidationException validation:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Mensagem = validation.Message;
                response.Dados = new { Errors = validation.Errors };
                break;

            case ConflictException conflict:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response.Mensagem = conflict.Message;
                break;

            case UnauthorizedException unauthorized:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                response.Mensagem = unauthorized.Message;
                break;

            case ArgumentException arg:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Mensagem = "Argumento inválido fornecido à API";
                break;

            case TimeoutException:
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                response.Mensagem = "Operação expirou. Tente novamente mais tarde.";
                break;

            case OperationCanceledException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Mensagem = "Operação foi cancelada";
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Mensagem = "Erro interno do servidor";
                response.Dados = new { ErrorId = context.TraceIdentifier };
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Extensão para registrar o middleware global de exceções
/// </summary>
public static class ExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
