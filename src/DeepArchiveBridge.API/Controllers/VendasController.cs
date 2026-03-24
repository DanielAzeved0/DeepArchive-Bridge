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

    public VendasController(IVendaRepository repository, ILogger<VendasController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Busca vendas com suporte automático a Hot/Cold storage
    /// </summary>
    [HttpPost("buscar")]
    public async Task<ActionResult<ApiResponse<List<Venda>>>> Buscar([FromBody] BuscaVendaRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var vendas = await _repository.BuscarAsync(request, EstrategiaArmazenamento.Auto);
            stopwatch.Stop();

            var response = new ApiResponse<List<Venda>>
            {
                Sucesso = true,
                Dados = vendas,
                Mensagem = $"Encontradas {vendas.Count} vendas",
                Origem = "Bridge", // Pode vir de Hot ou Cold
                TempoMs = stopwatch.ElapsedMilliseconds
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas");
            return BadRequest(new ApiResponse<List<Venda>>
            {
                Sucesso = false,
                Mensagem = ex.Message
            });
        }
    }

    /// <summary>
    /// Busca uma venda específica por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Venda>>> BuscarPorId(int id)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var venda = await _repository.BuscarPorIdAsync(id);
            stopwatch.Stop();

            if (venda == null)
                return NotFound(new ApiResponse<Venda>
                {
                    Sucesso = false,
                    Mensagem = "Venda não encontrada"
                });

            return Ok(new ApiResponse<Venda>
            {
                Sucesso = true,
                Dados = venda,
                TempoMs = stopwatch.ElapsedMilliseconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar venda");
            return BadRequest(new ApiResponse<Venda>
            {
                Sucesso = false,
                Mensagem = ex.Message
            });
        }
    }

    /// <summary>
    /// Cria uma nova venda
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> Criar([FromBody] Venda venda)
    {
        try
        {
            var id = await _repository.CriarAsync(venda);

            return CreatedAtAction(nameof(BuscarPorId), new { id }, new ApiResponse<int>
            {
                Sucesso = true,
                Dados = id,
                Mensagem = "Venda criada com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar venda");
            return BadRequest(new ApiResponse<int>
            {
                Sucesso = false,
                Mensagem = ex.Message
            });
        }
    }

    /// <summary>
    /// Atualiza uma venda existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Atualizar(int id, [FromBody] Venda venda)
    {
        if (venda.Id != id)
            return BadRequest(new ApiResponse<object>
            {
                Sucesso = false,
                Mensagem = "ID não corresponde"
            });

        try
        {
            await _repository.AtualizarAsync(venda);

            return Ok(new ApiResponse<object>
            {
                Sucesso = true,
                Mensagem = "Venda atualizada com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar venda");
            return BadRequest(new ApiResponse<object>
            {
                Sucesso = false,
                Mensagem = ex.Message
            });
        }
    }

    /// <summary>
    /// Deleta uma venda
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Deletar(int id)
    {
        try
        {
            await _repository.DeletarAsync(id);

            return Ok(new ApiResponse<object>
            {
                Sucesso = true,
                Mensagem = "Venda deletada com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar venda");
            return BadRequest(new ApiResponse<object>
            {
                Sucesso = false,
                Mensagem = ex.Message
            });
        }
    }
}
