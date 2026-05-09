using System.Text.Json.Serialization;

namespace DeepArchiveBridge.Core.Models;

public class ArquivamentoLog
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("dataExecucao")]
    public DateTime DataExecucao { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("vendasProcessadas")]
    public int VendasProcessadas { get; set; }

    [JsonPropertyName("itensProcessados")]
    public int ItensProcessados { get; set; }

    [JsonPropertyName("valorProcessado")]
    public decimal ValorProcessado { get; set; }

    [JsonPropertyName("duracaoMs")]
    public long DuracaoMs { get; set; }

    [JsonPropertyName("mensagem")]
    public string Mensagem { get; set; } = string.Empty;
}
