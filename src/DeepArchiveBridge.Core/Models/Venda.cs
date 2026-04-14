namespace DeepArchiveBridge.Core.Models;

/// <summary>
/// Entidade de Transação de Venda
/// Será armazenada tanto no banco "quente" quanto nos arquivos "frios"
/// </summary>
public class Venda
{
    public int Id { get; set; }

    public string ClienteId { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;

    public decimal Valor { get; set; }
    public DateTime DataVenda { get; set; }

    public VendaStatus Status { get; set; } = VendaStatus.Pendente;

    public List<VendaItem> Itens { get; set; } = new();

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
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
