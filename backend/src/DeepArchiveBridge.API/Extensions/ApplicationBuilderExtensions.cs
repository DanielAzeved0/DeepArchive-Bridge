using DeepArchiveBridge.API.Middleware;
using DeepArchiveBridge.Core.Models;
using DeepArchiveBridge.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DeepArchiveBridge.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseDeepArchiveDatabase(this WebApplication app)
    {
        if (app.Environment.IsEnvironment("Testing"))
        {
            return app;
        }

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<VendaDbContext>();
        dbContext.Database.Migrate();

        return app;
    }

    public static WebApplication UseDeepArchivePipeline(this WebApplication app)
    {
        var apiOptions = new ApiOptions();
        app.Configuration.GetSection("ApiSettings").Bind(apiOptions);

        app.UseRateLimiting(apiOptions.RateLimitRequestsPerMinute ?? 100);
        app.UseGlobalExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        if (apiOptions.EnableCors)
        {
            app.UseCors("AllowConfiguredOrigins");
        }

        app.UseAuthentication();
        app.UseAuthorization();

        if (apiOptions.EnableHealthCheck)
        {
            app.MapHealthChecks("/api/health");
        }

        app.MapControllers();
        return app;
    }
}
