using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Core.Models;

namespace DeepArchiveBridge.Data.Services;

/// <summary>
/// Implementação da lógica que arbitra entre Hot (0-90 dias) e Cold (>90 dias)
/// </summary>
public class DataResolver : IDataResolver
{
    private const int DiasHot = 90;

    public EstrategiaArmazenamento ResolverEstrategia(DateTime data)
    {
        var diasDecorridos = (DateTime.UtcNow - data).Days;
        return diasDecorridos <= DiasHot ? EstrategiaArmazenamento.Hot : EstrategiaArmazenamento.Cold;
    }

    public EstrategiaArmazenamento ResolverEstrategiaRange(DateTime dataInicio, DateTime dataFim)
    {
        var diasInicio = (DateTime.UtcNow - dataInicio).Days;
        var diasFim = (DateTime.UtcNow - dataFim).Days;

        // Se o fim do range está dentro dos 90 dias, usa Hot
        if (diasFim <= DiasHot)
            return EstrategiaArmazenamento.Hot;

        // Se o início está fora dos 90 dias, usa Cold
        if (diasInicio > DiasHot)
            return EstrategiaArmazenamento.Cold;

        // Se estão em ranges diferentes, precisa de ambos
        // Por enquanto, vamos preferir Hot para completude
        return EstrategiaArmazenamento.Hot;
    }
}
