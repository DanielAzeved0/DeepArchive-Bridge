using DeepArchiveBridge.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DeepArchiveBridge.Tests;

public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    private const string TestSecretKey = "SuperSecretKeyThatIsLongEnoughFor256BitSymmetricEncryption";
    private const string TestIssuer = "DeepArchiveBridge";
    private const string TestAudience = "DeepArchiveBridge-API";

    private SqliteConnection? _connection;

    public TestApiFactory()
    {
        Environment.SetEnvironmentVariable("JwtSettings__SecretKey", TestSecretKey);
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", TestIssuer);
        Environment.SetEnvironmentVariable("JwtSettings__Audience", TestAudience);
        Environment.SetEnvironmentVariable("JwtSettings__ExpirationHours", "24");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SQLite"] = "Data Source=:memory:",
                ["JwtSettings:SecretKey"] = TestSecretKey,
                ["JwtSettings:Issuer"] = TestIssuer,
                ["JwtSettings:Audience"] = TestAudience,
                ["JwtSettings:ExpirationHours"] = "24",
                ["ApiSettings:EnableCors"] = "false",
                ["ApiSettings:EnableHealthCheck"] = "true",
                ["ApiSettings:RateLimitRequestsPerMinute"] = "1000"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<VendaDbContext>>();

            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            services.AddSingleton(_connection);

            services.AddDbContext<VendaDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<VendaDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}
