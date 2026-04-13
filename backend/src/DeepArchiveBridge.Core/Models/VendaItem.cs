using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DeepArchiveBridge.Core.Models;

/// <summary>
/// Representa um item de venda individual
/// </summary>
public class VendaItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    /// <summary>
    /// Descrição/nome do produto
    /// Aceita "descricao" do frontend (que é o que ele envia)
    /// </summary>
    [JsonPropertyName("descricao")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Produto { get; set; } = string.Empty;
    
    [JsonPropertyName("quantidade")]
    public decimal Quantidade { get; set; }
    
    /// <summary>
    /// Preço unitário do item
    /// Aceita "valor" do frontend (que é o que ele envia)
    /// </summary>
    [JsonPropertyName("valor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal PrecoUnitario { get; set; }
    
    /// <summary>
    /// Total calculado (não é enviado pelo frontend)
    /// </summary>
    [JsonIgnore]
    public decimal Total => Quantidade * PrecoUnitario;
}
