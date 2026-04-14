namespace DeepArchiveBridge.Core.Interfaces;

using DeepArchiveBridge.Core.Models;

/// <summary>
/// Interface para trabalhar com Cold Storage (SQLite)
/// </summary>
public interface IColdStorageService
{
    Task<List<Venda>> BuscarVendasAsync(DateTime dataInicio, DateTime dataFim, string? clienteId = null);
    Task SalvarVendasAsync(List<Venda> vendas, DateTime mes);
}
