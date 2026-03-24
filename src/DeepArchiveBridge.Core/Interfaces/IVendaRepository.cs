using DeepArchiveBridge.Core.Models;

namespace DeepArchiveBridge.Core.Interfaces;

/// <summary>
/// Define a estratégia de busca de dados
/// </summary>
public enum EstrategiaArmazenamento
{
    Hot,      // Banco de dados PostgreSQL
    Cold,     // Arquivos Parquet na nuvem
    Auto      // Sistema decide automaticamente
}

/// <summary>
/// Interface para repository de vendas
/// </summary>
public interface IVendaRepository
{
    Task<List<Venda>> BuscarAsync(BuscaVendaRequest request, EstrategiaArmazenamento estrategia = EstrategiaArmazenamento.Auto);
    Task<Venda?> BuscarPorIdAsync(int id, EstrategiaArmazenamento estrategia = EstrategiaArmazenamento.Auto);
    Task<int> CriarAsync(Venda venda);
    Task AtualizarAsync(Venda venda);
    Task DeletarAsync(int id);
}
