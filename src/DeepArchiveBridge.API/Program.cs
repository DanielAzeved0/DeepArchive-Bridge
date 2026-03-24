using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.Data.Context;
using DeepArchiveBridge.Data.Repositories;
using DeepArchiveBridge.Data.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Banco de Dados
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL") 
    ?? "Host=localhost;Database=deeparchive_bridge;Username=postgres;Password=postgres";

builder.Services.AddDbContext<VendaDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// Injeção de Dependências
builder.Services.AddScoped<IDataResolver, DataResolver>();
builder.Services.AddScoped<IHotStorageService, HotStorageService>();
builder.Services.AddScoped<IColdStorageService, ColdStorageService>();
builder.Services.AddScoped<IVendaRepository, VendaRepository>();
builder.Services.AddScoped<IArchivingService, ArchivingService>();

// API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()
    );
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
