using DeepArchiveBridge.API.Validators;
using DeepArchiveBridge.Core.Exceptions;
using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DeepArchiveBridge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize]
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
        
        _logger.LogInformation($"Busca iniciada: DataInicio={request.DataInicio}, DataFim={request.DataFim}, Skip={request.Skip}, Take={request.Take}");

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

        _logger.LogInformation($"Busca concluída: {vendas.Count} vendas encontradas em {stopwatch.ElapsedMilliseconds}ms");

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
        [FromBody] CreateVendaRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando criação de venda com {ItemCount} itens", request.Itens.Count);
        
        // Converter DTO para entidade de domínio
        var venda = request.ToVenda();

        // Gerar ClienteId automaticamente se vazio
        if (string.IsNullOrWhiteSpace(venda.ClienteId) && !string.IsNullOrWhiteSpace(venda.ClienteNome))
        {
            // Sanitizar: remover caracteres especiais, manter apenas alphanumméricos e hífen
            var clienteIdBase = System.Text.RegularExpressions.Regex.Replace(
                venda.ClienteNome.ToLower().Trim(),
                "[^a-z0-9-]|",
                "-"
            ).Replace("--", "-");
            
            venda.ClienteId = clienteIdBase.Length > 50 
                ? clienteIdBase.Substring(0, 50) 
                : clienteIdBase;
        }

        // Validar venda
        var validationResult = await _vendaValidator.ValidateAsync(venda, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validação falhou com {ErrorCount} erros", validationResult.Errors.Count);
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
        // Validação explícita de ID
        if (id <= 0)
            throw new ArgumentException("ID deve ser maior que zero");
        
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
    /// Aprova uma venda mudando seu status de Pendente para Confirmada
    /// </summary>
    [HttpPost("{id}/aprovar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Aprovar(
        int id,
        CancellationToken cancellationToken = default)
    {
        // Validação explícita de ID
        if (id <= 0)
            throw new ArgumentException("ID deve ser maior que zero");
        
        // Verificar se venda existe
        var vendaExistente = await _repository.BuscarPorIdAsync(id, EstrategiaArmazenamento.Auto, cancellationToken);
        if (vendaExistente == null)
            throw new NotFoundException(nameof(Venda), id);

        // Validar se está em status Pendente
        if (vendaExistente.Status != VendaStatus.Pendente)
            throw new ValidationException(
                "Erro ao aprovar venda",
                new[] { $"Venda deve estar em status 'Pendente' para ser aprovada. Status atual: {vendaExistente.Status}" }
            );

        // Atualizar status para Confirmada
        vendaExistente.Status = VendaStatus.Confirmada;
        await _repository.AtualizarAsync(vendaExistente, cancellationToken);

        _logger.LogInformation("Venda {VendaId} aprovada com sucesso", id);

        return Ok(new ApiResponse<object>
        {
            Sucesso = true,
            Mensagem = "Venda aprovada com sucesso"
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
        // Validação explícita de ID
        if (id <= 0)
            throw new ArgumentException("ID deve ser maior que zero");
        
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
