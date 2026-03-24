namespace DeepArchiveBridge.Core.Interfaces;

using DeepArchiveBridge.Core.Models;

/// <summary>
/// Interface para serviço de arquivamento automático de dados antigos
/// </summary>
public interface IArchivingService
{
    /// <summary>
    /// Identifica e arquiva vendas com mais de 90 dias
    /// </summary>
    /// <returns>Número de vendas arquivadas</returns>
    Task<int> ArquivarDadosAntigos();

    /// <summary>
    /// Retorna informações sobre dados que serão arquivados
    /// </summary>
    Task<ArquivamentoInfo> ObterInfoArquivamento();

    /// <summary>
    /// Executa o arquivamento com confirmação manual
    /// </summary>
    Task<ResultadoArquivamento> ArquivarComConfirmacao();
}

/// <summary>
/// Informações sobre dados a serem arquivados
/// </summary>
public class ArquivamentoInfo
{
    public int TotalVendas { get; set; }
    public int VendasParaArquivar { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal ValorAArquivar { get; set; }
    public DateTime DataMaisAntiga { get; set; }
    public DateTime DataLimite { get; set; } // 90 dias atrás
    public string Mensagem { get; set; } = string.Empty;
}

/// <summary>
/// Resultado da operação de arquivamento
/// </summary>
public class ResultadoArquivamento
{
    public bool Sucesso { get; set; }
    public int VendasArquivadas { get; set; }
    public int ItensArquivados { get; set; }
    public string ArquivoNome { get; set; } = string.Empty;
    public long TamanhoBytes { get; set; }
    public DateTime DataExecucao { get; set; }
    public TimeSpan Duracao { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}
