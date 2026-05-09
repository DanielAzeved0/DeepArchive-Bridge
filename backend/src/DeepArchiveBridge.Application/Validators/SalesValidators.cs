using DeepArchiveBridge.Core.Models;
using FluentValidation;

namespace DeepArchiveBridge.Application.Validators;

public class BuscaVendaRequestValidator : AbstractValidator<BuscaVendaRequest>
{
    public BuscaVendaRequestValidator()
    {
        RuleFor(x => x.DataInicio)
            .NotEmpty().WithMessage("DataInicio e obrigatoria")
            .LessThanOrEqualTo(x => x.DataFim)
            .WithMessage("DataInicio deve ser menor ou igual a DataFim")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("DataInicio nao pode ser no futuro");

        RuleFor(x => x.DataFim)
            .NotEmpty().WithMessage("DataFim e obrigatoria")
            .GreaterThanOrEqualTo(x => x.DataInicio)
            .WithMessage("DataFim deve ser maior ou igual a DataInicio")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("DataFim nao pode ser no futuro");

        RuleFor(x => x.ClienteId)
            .MaximumLength(100)
            .WithMessage("ClienteId nao pode exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.ClienteId));

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip deve ser maior ou igual a 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("Skip nao pode exceder 10000");

        RuleFor(x => x.Take)
            .GreaterThan(0)
            .WithMessage("Take deve ser maior que 0")
            .LessThanOrEqualTo(500)
            .WithMessage("Take nao pode exceder 500");
    }
}

public class VendaValidator : AbstractValidator<Venda>
{
    public VendaValidator()
    {
        RuleFor(x => x.ClienteNome)
            .NotEmpty().WithMessage("ClienteNome e obrigatorio")
            .MaximumLength(200).WithMessage("ClienteNome nao pode exceder 200 caracteres");

        RuleFor(x => x.ClienteId)
            .MaximumLength(100).WithMessage("ClienteId nao pode exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.ClienteId));

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("Valor deve ser maior que 0")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Valor deve ter no maximo 2 casas decimais");

        RuleFor(x => x.DataVenda)
            .NotEmpty().WithMessage("DataVenda e obrigatoria")
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddYears(-2))
            .WithMessage("DataVenda nao pode ser anterior a 2 anos atras")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("DataVenda nao pode ser no futuro");

        RuleFor(x => x.Itens)
            .NotEmpty().WithMessage("Venda deve ter pelo menos 1 item");

        RuleForEach(x => x.Itens)
            .SetValidator(new VendaItemValidator());
    }
}

public class VendaItemValidator : AbstractValidator<VendaItem>
{
    public VendaItemValidator()
    {
        RuleFor(x => x.Produto)
            .NotEmpty().WithMessage("Produto e obrigatorio")
            .MaximumLength(500).WithMessage("Produto nao pode exceder 500 caracteres");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0).WithMessage("Quantidade deve ser maior que 0")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Quantidade deve ter no maximo 2 casas decimais");

        RuleFor(x => x.PrecoUnitario)
            .GreaterThan(0).WithMessage("PrecoUnitario deve ser maior que 0")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("PrecoUnitario deve ter no maximo 2 casas decimais");
    }
}
