using FluentValidation;
using DeepArchiveBridge.Core.Models;

namespace DeepArchiveBridge.API.Validators;

/// <summary>
/// Validador para requisições de busca de vendas
/// </summary>
public class BuscaVendaRequestValidator : AbstractValidator<BuscaVendaRequest>
{
    public BuscaVendaRequestValidator()
    {
        RuleFor(x => x.DataInicio)
            .NotEmpty().WithMessage("DataInicio é obrigatória")
            .LessThanOrEqualTo(x => x.DataFim)
            .WithMessage("DataInicio deve ser menor ou igual a DataFim")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("DataInicio não pode ser no futuro");

        RuleFor(x => x.DataFim)
            .NotEmpty().WithMessage("DataFim é obrigatória")
            .GreaterThanOrEqualTo(x => x.DataInicio)
            .WithMessage("DataFim deve ser maior ou igual a DataInicio")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("DataFim não pode ser no futuro");

        RuleFor(x => x.ClienteId)
            .MaximumLength(100)
            .WithMessage("ClienteId não pode exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.ClienteId));

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip deve ser maior ou igual a 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("Skip não pode exceder 10000");

        RuleFor(x => x.Take)
            .GreaterThan(0)
            .WithMessage("Take deve ser maior que 0")
            .LessThanOrEqualTo(500)
            .WithMessage("Take não pode exceder 500");
    }
}

/// <summary>
/// Validador para criação de vendas
/// </summary>
public class VendaValidator : AbstractValidator<Venda>
{
    public VendaValidator()
    {
        RuleFor(x => x.ClienteNome)
            .NotEmpty().WithMessage("ClienteNome é obrigatório")
            .MaximumLength(200).WithMessage("ClienteNome não pode exceder 200 caracteres");

        // ClienteId será gerado automaticamente se vazio
        RuleFor(x => x.ClienteId)
            .MaximumLength(100).WithMessage("ClienteId não pode exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.ClienteId));

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("Valor deve ser maior que 0")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Valor deve ter no máximo 2 casas decimais");

        RuleFor(x => x.DataVenda)
            .NotEmpty().WithMessage("DataVenda é obrigatória")
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddYears(-2))
            .WithMessage("DataVenda não pode ser anterior a 2 anos atrás")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("DataVenda não pode ser no futuro");

        RuleFor(x => x.Itens)
            .NotEmpty().WithMessage("Venda deve ter pelo menos 1 item")
            .When(x => x.Itens != null);

        RuleForEach(x => x.Itens)
            .SetValidator(new VendaItemValidator());
    }
}

/// <summary>
/// Validador para itens de venda
/// </summary>
public class VendaItemValidator : AbstractValidator<VendaItem>
{
    public VendaItemValidator()
    {
        RuleFor(x => x.Produto)
            .NotEmpty().WithMessage("Produto é obrigatório")
            .MaximumLength(500).WithMessage("Produto não pode exceder 500 caracteres");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0).WithMessage("Quantidade deve ser maior que 0")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Quantidade deve ter no máximo 2 casas decimais");

        RuleFor(x => x.PrecoUnitario)
            .GreaterThan(0).WithMessage("PrecoUnitario deve ser maior que 0")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("PrecoUnitario deve ter no máximo 2 casas decimais");
    }
}
