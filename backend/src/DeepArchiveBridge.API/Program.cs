using DeepArchiveBridge.API.Middleware;
using DeepArchiveBridge.API.Validators;
using DeepArchiveBridge.API.Services;
using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Core.Models;
using DeepArchiveBridge.Data.Context;
using DeepArchiveBridge.Data.Repositories;
using DeepArchiveBridge.Data.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Banco de Dados SQLite
var sqliteConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=archive.db;Cache=Shared";

builder.Services.AddDbContext<VendaDbContext>(options =>
    options.UseSqlite(sqliteConnectionString)
);

// Injeção de Dependências - Serviços
builder.Services.AddScoped<IColdStorageService, ColdStorageService>();
builder.Services.AddScoped<IVendaRepository, VendaRepository>();
builder.Services.AddScoped<IArchivingService, ArchivingService>();

// Configuração de Opções (Options Pattern)
builder.Services.Configure<ArchivingOptions>(
    builder.Configuration.GetSection("ArchivingSettings")
);
builder.Services.Configure<LoggingOptions>(
    builder.Configuration.GetSection("LoggingSettings")
);
builder.Services.Configure<ApiOptions>(
    builder.Configuration.GetSection("ApiSettings")
);

// Validação com FluentValidation
builder.Services.AddScoped<BuscaVendaRequestValidator>();
builder.Services.AddScoped<VendaValidator>();
builder.Services.AddScoped<VendaItemValidator>();

// Autenticação JWT
builder.Services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));

builder.Services.AddAuthentication(options =>
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

builder.Services.AddAuthorization();

// API com suporte a camelCase JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health Check
builder.Services.AddHealthChecks();

// CORS - Configurado dinamicamente a partir de ApiSettings
var apiOptions = new ApiOptions();
builder.Configuration.GetSection("ApiSettings").Bind(apiOptions);

if (apiOptions.EnableCors)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowConfiguredOrigins", corsBuilder =>
        {
            var origins = apiOptions.AllowedOrigins.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim())
                .ToArray();
            corsBuilder.WithOrigins(origins)
                      .WithMethods("GET", "POST", "PUT", "DELETE")
                      .WithHeaders("Content-Type", "Authorization")
                      .AllowCredentials()
                      .WithExposedHeaders("Content-Length", "X-JSON-Response-Length");
        });
    });
}

var app = builder.Build();

// Middleware na ordem correta
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

// Adicionar autenticação e autorização ANTES dos controllers
app.UseAuthentication();
app.UseAuthorization();

if (apiOptions.EnableHealthCheck)
{
    app.MapHealthChecks("/api/health");
}

app.MapControllers();

app.Run();
