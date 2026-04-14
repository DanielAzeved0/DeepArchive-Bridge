using DeepArchiveBridge.API.Validators;
using DeepArchiveBridge.Core.Exceptions;
using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DeepArchiveBridge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendasController : ControllerBase
{
    private readonly IVendaRepository _repository;
    private readonly ILogger<VendasController> _logger;
    private readonly BuscaVendaRequestValidator _buscaValidator;
    private readonly VendaValidator _vendaValidator;

    public VendasController(
        IVendaRepository repository, 
        ILogger<VendasController> logger,
        BuscaVendaRequestValidator buscaValidator,
        VendaValidator vendaValidator)
    {
        _repository = repository;
        _logger = logger;
        _buscaValidator = buscaValidator;
        _vendaValidator = vendaValidator;
    }

    /// <summary>
    /// Busca vendas com suporte automático a Hot/Cold storage
    /// </summary>
    [HttpPost("buscar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<Venda>>>> Buscar(
        [FromBody] BuscaVendaRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // Validar requisição
        var validationResult = await _buscaValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                "Parâmetros de busca inválidos",
                validationResult.Errors.Select(e => e.ErrorMessage)
            );
        }

        var vendas = await _repository.BuscarAsync(request, EstrategiaArmazenamento.Auto, cancellationToken);
        stopwatch.Stop();

        var response = new ApiResponse<List<Venda>>
        {
            Sucesso = true,
            Dados = vendas,
            Mensagem = $"Encontradas {vendas.Count} vendas",
            Origem = "Bridge",
            TempoMs = stopwatch.ElapsedMilliseconds
        };

        return Ok(response);
    }

    /// <summary>
    /// Busca uma venda específica por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Venda>>> BuscarPorId(
        int id, 
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var venda = await _repository.BuscarPorIdAsync(id, EstrategiaArmazenamento.Auto, cancellationToken);
        stopwatch.Stop();

        if (venda == null)
            throw new NotFoundException(nameof(Venda), id);

        return Ok(new ApiResponse<Venda>
        {
            Sucesso = true,
            Dados = venda,
            TempoMs = stopwatch.ElapsedMilliseconds
        });
    }

    /// <summary>
    /// Cria uma nova venda
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> Criar(
        [FromBody] Venda venda,
        CancellationToken cancellationToken = default)
    {
        // Validar venda
        var validationResult = await _vendaValidator.ValidateAsync(venda, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                "Dados da venda inválidos",
                validationResult.Errors.Select(e => e.ErrorMessage)
            );
        }

        var id = await _repository.CriarAsync(venda, cancellationToken);

        return CreatedAtAction(nameof(BuscarPorId), new { id }, new ApiResponse<int>
        {
            Sucesso = true,
            Dados = id,
            Mensagem = "Venda criada com sucesso"
        });
    }

    /// <summary>
    /// Atualiza uma venda existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Atualizar(
        int id, 
        [FromBody] Venda venda,
        CancellationToken cancellationToken = default)
    {
        if (venda.Id != id)
            throw new ValidationException("ID na URL não corresponde ao ID do corpo da requisição");

        // Validar venda
        var validationResult = await _vendaValidator.ValidateAsync(venda, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                "Dados da venda inválidos",
                validationResult.Errors.Select(e => e.ErrorMessage)
            );
        }

        // Verificar se venda existe
        var vendaExistente = await _repository.BuscarPorIdAsync(id, EstrategiaArmazenamento.Auto, cancellationToken);
        if (vendaExistente == null)
            throw new NotFoundException(nameof(Venda), id);

        await _repository.AtualizarAsync(venda, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Sucesso = true,
            Mensagem = "Venda atualizada com sucesso"
        });
    }

    /// <summary>
    /// Deleta uma venda
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Deletar(
        int id,
        CancellationToken cancellationToken = default)
    {
        // Verificar se venda existe
        var vendaExistente = await _repository.BuscarPorIdAsync(id, EstrategiaArmazenamento.Auto, cancellationToken);
        if (vendaExistente == null)
            throw new NotFoundException(nameof(Venda), id);

        await _repository.DeletarAsync(id, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Sucesso = true,
            Mensagem = "Venda deletada com sucesso"
        });
    }
}
