namespace DeepArchiveBridge.Core.Models;

/// <summary>
/// Representa um item de venda individual
/// </summary>
public class VendaItem
{
    public int Id { get; set; }
    public string Produto { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Total => Quantidade * PrecoUnitario;
}
