namespace DeepArchiveBridge.Core.Interfaces;

using DeepArchiveBridge.Core.Models;

/// <summary>
/// Define como o sistema arbitra entre Hot e Cold
/// </summary>
public interface IDataResolver
{
    /// <summary>
    /// Decide se deve usar Hot (0-90 dias) ou Cold (>90 dias)
    /// </summary>
    EstrategiaArmazenamento ResolverEstrategia(DateTime data);

    /// <summary>
    /// Decide se deve usar Hot ou Cold baseado em um range de datas
    /// </summary>
    EstrategiaArmazenamento ResolverEstrategiaRange(DateTime dataInicio, DateTime dataFim);
}

/// <summary>
/// Interface para trabalhar com armazenamento na nuvem (Parquet)
/// </summary>
public interface IColdStorageService
{
    Task<List<Venda>> BuscarVendasAsync(DateTime dataInicio, DateTime dataFim, string? clienteId = null);
    Task SalvarVendasAsync(List<Venda> vendas, DateTime mes);
}

/// <summary>
/// Interface para trabalhar com banco de dados (Hot)
/// </summary>
public interface IHotStorageService
{
    Task<List<Venda>> BuscarVendasAsync(BuscaVendaRequest request);
    Task<Venda?> BuscarVendaPorIdAsync(int id);
    Task<int> CriarVendaAsync(Venda venda);
    Task AtualizarVendaAsync(Venda venda);
    Task DeletarVendaAsync(int id);
}
