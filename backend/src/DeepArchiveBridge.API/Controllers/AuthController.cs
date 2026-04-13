using DeepArchiveBridge.API.Services;
using DeepArchiveBridge.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeepArchiveBridge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authenticationService,
        ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Gera um token JWT para acesso à API
    /// </summary>
    /// <param name="clienteId">ID do cliente (opcional)</param>
    /// <returns>Token JWT válido por 24 horas</returns>
    [HttpPost("token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ApiResponse<TokenResponse>> GenerateToken([FromQuery] string? clienteId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(clienteId))
            {
                clienteId = "app-default";
            }

            // Sanitizar input
            clienteId = System.Text.RegularExpressions.Regex.Replace(clienteId, @"[^a-zA-Z0-9\-]", "");

            _logger.LogInformation("Token generation requested for client: {ClienteId}", clienteId);

            var token = _authenticationService.GenerateToken(clienteId, "api-user", "Admin");

            return Ok(new ApiResponse<TokenResponse>
            {
                Sucesso = true,
                Mensagem = "Token gerado com sucesso",
                Dados = new TokenResponse
                {
                    Token = token,
                    ExpiresIn = 86400, // 24 horas em segundos
                    TokenType = "Bearer"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar token");
            return BadRequest(new ApiResponse<TokenResponse>
            {
                Sucesso = false,
                Mensagem = "Erro ao gerar token",
                Dados = null
            });
        }
    }
}

/// <summary>
/// Resposta contendo o token JWT
/// </summary>
public class TokenResponse
{
    /// <summary>Token JWT</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Tempo de expiração em segundos (padrão: 86400 = 24h)</summary>
    public int ExpiresIn { get; set; } = 86400;

    /// <summary>Tipo do token (Bearer)</summary>
    public string TokenType { get; set; } = "Bearer";
}
