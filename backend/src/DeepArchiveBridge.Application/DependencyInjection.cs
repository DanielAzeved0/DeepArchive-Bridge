using DeepArchiveBridge.Application.Services;
using DeepArchiveBridge.Application.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace DeepArchiveBridge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<BuscaVendaRequestValidator>();
        services.AddScoped<VendaValidator>();
        services.AddScoped<VendaItemValidator>();
        services.AddScoped<IVendaApplicationService, VendaApplicationService>();

        return services;
    }
}
