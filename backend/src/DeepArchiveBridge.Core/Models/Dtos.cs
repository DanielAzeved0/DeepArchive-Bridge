using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DeepArchiveBridge.Core.Models;

/// <summary>
/// Request para buscar vendas
/// </summary>
public class BuscaVendaRequest
{
    [JsonPropertyName("dataInicio")]
    public DateTime DataInicio { get; set; }

    [JsonPropertyName("dataFim")]
    public DateTime DataFim { get; set; }

    [JsonPropertyName("clienteId")]
    public string? ClienteId { get; set; }

    [JsonPropertyName("status")]
    public VendaStatus? Status { get; set; }

    [JsonPropertyName("skip")]
    public int Skip { get; set; } = 0;

    [JsonPropertyName("take")]
    public int Take { get; set; } = 100;
}

/// <summary>
/// DTO para criação de venda (aceita JSON do frontend conforme enviado)
/// Frontend envia: clienteNome, dataVenda, valor, itens[]
/// Sem necessidade de wrapper
/// </summary>
public class CreateVendaRequest
{
    [Required(ErrorMessage = "ClienteNome é obrigatório")]
    [StringLength(200, ErrorMessage = "ClienteNome não pode exceder 200 caracteres")]
    [JsonPropertyName("clienteNome")]
    public string ClienteNome { get; set; } = string.Empty;

    [JsonPropertyName("clienteId")]
    [StringLength(100, ErrorMessage = "ClienteId não pode exceder 100 caracteres")]
    public string? ClienteId { get; set; }

    [Required(ErrorMessage = "DataVenda é obrigatória")]
    [JsonPropertyName("dataVenda")]
    public DateTime DataVenda { get; set; }

    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que 0")]
    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [Required(ErrorMessage = "Itens são obrigatórios")]
    [MinLength(1, ErrorMessage = "Venda deve ter pelo menos 1 item")]
    [JsonPropertyName("itens")]
    public List<CreateVendaItemRequest> Itens { get; set; } = new();

    [JsonPropertyName("status")]
    public VendaStatus Status { get; set; } = VendaStatus.Pendente;

    /// <summary>
    /// Converte este DTO para a entidade Venda
    /// </summary>
    public Venda ToVenda()
    {
        return new Venda
        {
            ClienteId = string.IsNullOrWhiteSpace(ClienteId) ? string.Empty : ClienteId,
            ClienteNome = ClienteNome,
            DataVenda = DataVenda,
            Valor = Valor,
            Status = Status,
            Itens = Itens.ConvertAll(i => i.ToVendaItem())
        };
    }
}

/// <summary>
/// DTO para item de venda na criação (frontend envia: descricao, quantidade, valor)
/// </summary>
public class CreateVendaItemRequest
{
    [Required(ErrorMessage = "Descrição/Produto é obrigatório")]
    [StringLength(500, ErrorMessage = "Descrição não pode exceder 500 caracteres")]
    [JsonPropertyName("descricao")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Quantidade é obrigatória")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantidade deve ser maior que 0")]
    [JsonPropertyName("quantidade")]
    public decimal Quantidade { get; set; }

    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que 0")]
    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    /// <summary>
    /// Converte para VendaItem (modelo de domínio do backend)
    /// </summary>
    public VendaItem ToVendaItem()
    {
        return new VendaItem
        {
            Produto = Descricao,
            Quantidade = Quantidade,
            PrecoUnitario = Valor
        };
    }
}

/// <summary>
/// Response padrão
/// </summary>
public class ApiResponse<T>
{
    [JsonPropertyName("sucesso")]
    public bool Sucesso { get; set; }

    [JsonPropertyName("mensagem")]
    public string? Mensagem { get; set; }

    [JsonPropertyName("dados")]
    public T? Dados { get; set; }

    [JsonPropertyName("origem")]
    public string? Origem { get; set; } // "Hot" ou "Cold"

    [JsonPropertyName("tempoMs")]
    public long TempoMs { get; set; }
}
