using DeepArchiveBridge.Application.Services;
using DeepArchiveBridge.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DeepArchiveBridge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class VendasController : ControllerBase
{
    private readonly IVendaApplicationService _vendaService;

    public VendasController(IVendaApplicationService vendaService)
    {
        _vendaService = vendaService;
    }

    [HttpPost("buscar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<VendaResponse>>>> Buscar(
        [FromBody] BuscaVendaRequest request,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _vendaService.BuscarAsync(request, cancellationToken));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VendaResponse>>> BuscarPorId(
        int id,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _vendaService.BuscarPorIdAsync(id, cancellationToken));
    }

    [HttpGet("{id}/navigation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VendaNavigationResponse>>> BuscarNavegacao(
        int id,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _vendaService.BuscarNavegacaoAsync(id, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> Criar(
        [FromBody] CreateVendaRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _vendaService.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(BuscarPorId), new { id = response.Dados }, response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Atualizar(
        int id,
        [FromBody] UpdateVendaRequest request,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _vendaService.AtualizarAsync(id, request, cancellationToken));
    }

    [HttpPost("{id}/aprovar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Aprovar(
        int id,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _vendaService.AprovarAsync(id, cancellationToken));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Deletar(
        int id,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _vendaService.DeletarAsync(id, cancellationToken));
    }
}
