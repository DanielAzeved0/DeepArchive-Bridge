using DeepArchiveBridge.Core.Models;

namespace DeepArchiveBridge.Core.Interfaces;

/// <summary>
/// Define a estratégia de busca de dados
/// </summary>
public enum EstrategiaArmazenamento
{
    Hot,      // Banco de dados SQLite
    Cold,     // Arquivos em Cold Storage
    Auto      // Sistema decide automaticamente
}

/// <summary>
/// Interface para repository de vendas
/// </summary>
public interface IVendaRepository
{
    Task<List<Venda>> BuscarAsync(
        BuscaVendaRequest request, 
        EstrategiaArmazenamento estrategia = EstrategiaArmazenamento.Auto,
        CancellationToken cancellationToken = default);

    Task<Venda?> BuscarPorIdAsync(
        int id, 
        EstrategiaArmazenamento estrategia = EstrategiaArmazenamento.Auto,
        CancellationToken cancellationToken = default);

    Task<int> CriarAsync(Venda venda, CancellationToken cancellationToken = default);
    
    Task AtualizarAsync(Venda venda, CancellationToken cancellationToken = default);
    
    Task DeletarAsync(int id, CancellationToken cancellationToken = default);
}
