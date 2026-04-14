namespace DeepArchiveBridge.Core.Models;

/// <summary>
/// Request para buscar vendas
/// </summary>
public class BuscaVendaRequest
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string? ClienteId { get; set; }
    public VendaStatus? Status { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 100;
}

/// <summary>
/// Response padrão
/// </summary>
public class ApiResponse<T>
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public T? Dados { get; set; }
    public string? Origem { get; set; } // "Hot" ou "Cold"
    public long TempoMs { get; set; }
}
