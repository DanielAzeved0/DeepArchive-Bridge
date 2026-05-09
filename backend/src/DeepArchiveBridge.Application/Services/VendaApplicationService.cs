using System.Diagnostics;
using System.Text.RegularExpressions;
using DeepArchiveBridge.Application.Validators;
using DeepArchiveBridge.Core.Exceptions;
using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Core.Models;
using Microsoft.Extensions.Logging;

namespace DeepArchiveBridge.Application.Services;

public interface IVendaApplicationService
{
    Task<ApiResponse<List<VendaResponse>>> BuscarAsync(BuscaVendaRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<VendaResponse>> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<VendaNavigationResponse>> BuscarNavegacaoAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<int>> CriarAsync(CreateVendaRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> AtualizarAsync(int id, UpdateVendaRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> AprovarAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> DeletarAsync(int id, CancellationToken cancellationToken = default);
}

public class VendaApplicationService : IVendaApplicationService
{
    private readonly IVendaRepository _repository;
    private readonly ILogger<VendaApplicationService> _logger;
    private readonly BuscaVendaRequestValidator _buscaValidator;
    private readonly VendaValidator _vendaValidator;

    public VendaApplicationService(
        IVendaRepository repository,
        ILogger<VendaApplicationService> logger,
        BuscaVendaRequestValidator buscaValidator,
        VendaValidator vendaValidator)
    {
        _repository = repository;
        _logger = logger;
        _buscaValidator = buscaValidator;
        _vendaValidator = vendaValidator;
    }

    public async Task<ApiResponse<List<VendaResponse>>> BuscarAsync(
        BuscaVendaRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var validationResult = await _buscaValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                "Parametros de busca invalidos",
                validationResult.Errors.Select(e => e.ErrorMessage));
        }

        var vendas = await _repository.BuscarAsync(request, EstrategiaArmazenamento.Auto, cancellationToken);
        stopwatch.Stop();

        _logger.LogInformation("Busca concluida: {Count} vendas encontradas em {ElapsedMs}ms", vendas.Count, stopwatch.ElapsedMilliseconds);

        return new ApiResponse<List<VendaResponse>>
        {
            Sucesso = true,
            Dados = vendas.ConvertAll(VendaResponse.FromVenda),
            Mensagem = $"Encontradas {vendas.Count} vendas",
            Origem = "Bridge",
            TempoMs = stopwatch.ElapsedMilliseconds
        };
    }

    public async Task<ApiResponse<VendaResponse>> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureValidId(id);

        var stopwatch = Stopwatch.StartNew();
        var venda = await _repository.BuscarPorIdAsync(id, EstrategiaArmazenamento.Auto, cancellationToken);
        stopwatch.Stop();

        if (venda == null)
        {
            throw new NotFoundException(nameof(Venda), id);
        }

        return new ApiResponse<VendaResponse>
        {
            Sucesso = true,
            Dados = VendaResponse.FromVenda(venda),
            TempoMs = stopwatch.ElapsedMilliseconds
        };
    }

    public async Task<ApiResponse<VendaNavigationResponse>> BuscarNavegacaoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        EnsureValidId(id);

        var venda = await _repository.BuscarPorIdAsync(id, EstrategiaArmazenamento.Auto, cancellationToken);
        if (venda == null)
        {
            throw new NotFoundException(nameof(Venda), id);
        }

        var navigation = await _repository.BuscarNavegacaoAsync(id, cancellationToken);

        return new ApiResponse<VendaNavigationResponse>
        {
            Sucesso = true,
            Dados = navigation,
            Mensagem = "Navegacao da venda carregada com sucesso"
        };
    }

    public async Task<ApiResponse<int>> CriarAsync(CreateVendaRequest request, CancellationToken cancellationToken = default)
    {
        var venda = request.ToVenda();
        EnsureClienteId(venda);
        await ValidateVendaAsync(venda, cancellationToken);

        var id = await _repository.CriarAsync(venda, cancellationToken);

        return new ApiResponse<int>
        {
            Sucesso = true,
            Dados = id,
            Mensagem = "Venda criada com sucesso"
        };
    }

    public async Task<ApiResponse<object>> AtualizarAsync(
        int id,
        UpdateVendaRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureValidId(id);

        var venda = request.ToVenda(id);
        EnsureClienteId(venda);
        await ValidateVendaAsync(venda, cancellationToken);

        var vendaExistente = await _repository.BuscarPorIdAsync(id, EstrategiaArmazenamento.Auto, cancellationToken);
        if (vendaExistente == null)
        {
            throw new NotFoundException(nameof(Venda), id);
        }

        await _repository.AtualizarAsync(venda, cancellationToken);

        return new ApiResponse<object>
        {
            Sucesso = true,
            Mensagem = "Venda atualizada com sucesso"
        };
    }

    public async Task<ApiResponse<object>> AprovarAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureValidId(id);

        var vendaExistente = await _repository.BuscarPorIdAsync(id, EstrategiaArmazenamento.Auto, cancellationToken);
        if (vendaExistente == null)
        {
            throw new NotFoundException(nameof(Venda), id);
        }

        if (vendaExistente.Status != VendaStatus.Pendente)
        {
            throw new ValidationException(
                "Erro ao aprovar venda",
                new[] { $"Venda deve estar em status 'Pendente' para ser aprovada. Status atual: {vendaExistente.Status}" });
        }

        vendaExistente.Status = VendaStatus.Confirmada;
        await _repository.AtualizarAsync(vendaExistente, cancellationToken);

        return new ApiResponse<object>
        {
            Sucesso = true,
            Mensagem = "Venda aprovada com sucesso"
        };
    }

    public async Task<ApiResponse<object>> DeletarAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureValidId(id);

        var vendaExistente = await _repository.BuscarPorIdAsync(id, EstrategiaArmazenamento.Auto, cancellationToken);
        if (vendaExistente == null)
        {
            throw new NotFoundException(nameof(Venda), id);
        }

        await _repository.DeletarAsync(id, cancellationToken);

        return new ApiResponse<object>
        {
            Sucesso = true,
            Mensagem = "Venda deletada com sucesso"
        };
    }

    private async Task ValidateVendaAsync(Venda venda, CancellationToken cancellationToken)
    {
        var validationResult = await _vendaValidator.ValidateAsync(venda, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                "Dados da venda invalidos",
                validationResult.Errors.Select(e => e.ErrorMessage));
        }
    }

    private static void EnsureValidId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("ID deve ser maior que zero");
        }
    }

    private static void EnsureClienteId(Venda venda)
    {
        if (!string.IsNullOrWhiteSpace(venda.ClienteId) || string.IsNullOrWhiteSpace(venda.ClienteNome))
        {
            return;
        }

        var clienteIdBase = Regex.Replace(venda.ClienteNome.ToLowerInvariant().Trim(), "[^a-z0-9-]+", "-")
            .Trim('-');

        venda.ClienteId = clienteIdBase.Length > 50
            ? clienteIdBase[..50]
            : clienteIdBase;
    }
}
