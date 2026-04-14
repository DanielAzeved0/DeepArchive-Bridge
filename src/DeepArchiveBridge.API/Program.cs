using DeepArchiveBridge.API.Middleware;
using DeepArchiveBridge.API.Validators;
using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Core.Models;
using DeepArchiveBridge.Data.Context;
using DeepArchiveBridge.Data.Repositories;
using DeepArchiveBridge.Data.Services;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

// API
builder.Services.AddControllers();
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
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
        });
    });
}

var app = builder.Build();

// Middleware
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

if (apiOptions.EnableHealthCheck)
{
    app.MapHealthChecks("/api/health");
}

app.UseAuthorization();
app.MapControllers();

app.Run();
