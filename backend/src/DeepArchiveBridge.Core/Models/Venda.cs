using System.Text.Json.Serialization;

namespace DeepArchiveBridge.Core.Models;

/// <summary>
/// Entidade de Transação de Venda
/// Será armazenada tanto no banco "quente" quanto nos arquivos "frios"
/// </summary>
public class Venda
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("clienteId")]
    public string ClienteId { get; set; } = string.Empty;
    
    [JsonPropertyName("clienteNome")]
    public string ClienteNome { get; set; } = string.Empty;

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }
    
    [JsonPropertyName("dataVenda")]
    public DateTime DataVenda { get; set; }

    [JsonPropertyName("status")]
    public VendaStatus Status { get; set; } = VendaStatus.Pendente;

    [JsonPropertyName("itens")]
    public List<VendaItem> Itens { get; set; } = new();

    [JsonPropertyName("dataCriacao")]
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("dataAtualizacao")]
    public DateTime? DataAtualizacao { get; set; }
}

/// <summary>
/// Status possível de uma venda
/// </summary>
public enum VendaStatus
{
    Pendente = 1,
    Confirmada = 2,
    Entregue = 3,
    Cancelada = 4
}
