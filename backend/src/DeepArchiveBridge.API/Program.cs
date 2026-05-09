using DeepArchiveBridge.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDeepArchiveOptions(builder.Configuration)
    .AddDeepArchiveDatabase(builder.Configuration)
    .AddDeepArchiveServices()
    .AddDeepArchiveAuthentication(builder.Configuration)
    .AddDeepArchiveApi(builder.Configuration);

var app = builder.Build();

app.UseDeepArchiveDatabase();
app.UseDeepArchivePipeline();

app.Run();

public partial class Program { }
