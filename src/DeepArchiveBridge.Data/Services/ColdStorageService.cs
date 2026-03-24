using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Core.Models;

namespace DeepArchiveBridge.Data.Services;

/// <summary>
/// Serviço para acesso ao armazenamento "Cold" (Azure Blob Storage com Parquet)
/// Implementação stub por enquanto - será implementado na próxima fase
/// </summary>
public class ColdStorageService : IColdStorageService
{
    // TODO: Implementar integração com Azure Blob Storage
    // TODO: Implementar serialização/desserialização de Parquet

    public async Task<List<Venda>> BuscarVendasAsync(DateTime dataInicio, DateTime dataFim, string? clienteId = null)
    {
        // Stub: retorna lista vazia
        // Na próxima fase, isso buscará do Azure Blob Storage
        await Task.Delay(100); // Simula latência
        return new List<Venda>();
    }

    public async Task SalvarVendasAsync(List<Venda> vendas, DateTime mes)
    {
        // Stub: simula salvamento
        // Na próxima fase, isso salvará em Parquet no Azure Blob Storage
        await Task.Delay(200); // Simula latência
    }
}
