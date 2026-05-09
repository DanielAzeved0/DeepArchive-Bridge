using System.Text;
using System.Text.Json.Serialization;
using DeepArchiveBridge.Application;
using DeepArchiveBridge.API.Controllers;
using DeepArchiveBridge.API.Services;
using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Core.Models;
using DeepArchiveBridge.Data.Context;
using DeepArchiveBridge.Data.Repositories;
using DeepArchiveBridge.Data.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DeepArchiveBridge.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeepArchiveOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ArchivingOptions>(configuration.GetSection("ArchivingSettings"));
        services.Configure<LoggingOptions>(configuration.GetSection("LoggingSettings"));
        services.Configure<ApiOptions>(configuration.GetSection("ApiSettings"));
        return services;
    }

    public static IServiceCollection AddDeepArchiveDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var sqliteConnectionString = configuration.GetConnectionString("SQLite")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=archive.db;Cache=Shared";

        services.AddDbContext<VendaDbContext>(options => options.UseSqlite(sqliteConnectionString));
        return services;
    }

    public static IServiceCollection AddDeepArchiveServices(this IServiceCollection services)
    {
        services.AddScoped<IColdStorageService, ColdStorageService>();
        services.AddScoped<IVendaRepository, VendaRepository>();
        services.AddScoped<IArchivingService, ArchivingService>();
        services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
        services.AddScoped<IDependencyHealthCheck, DatabaseHealthCheck>();
        services.AddScoped<IDependencyHealthCheck, ColdStorageHealthCheck>();
        services.AddScoped<IDependencyHealthCheck, AuthenticationHealthCheck>();
        services.AddApplicationServices();
        return services;
    }

    public static IServiceCollection AddDeepArchiveAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(
            jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddDeepArchiveApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHealthChecks();
        services.AddDeepArchiveCors(configuration);
        return services;
    }

    private static IServiceCollection AddDeepArchiveCors(this IServiceCollection services, IConfiguration configuration)
    {
        var apiOptions = new ApiOptions();
        configuration.GetSection("ApiSettings").Bind(apiOptions);

        if (!apiOptions.EnableCors)
        {
            return services;
        }

        services.AddCors(options =>
        {
            options.AddPolicy("AllowConfiguredOrigins", corsBuilder =>
            {
                corsBuilder.WithOrigins(apiOptions.AllowedOrigins)
                    .WithMethods("GET", "POST", "PUT", "DELETE")
                    .WithHeaders("Content-Type", "Authorization")
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Length", "X-JSON-Response-Length");
            });
        });

        return services;
    }
}
