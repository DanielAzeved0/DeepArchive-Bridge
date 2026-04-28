using DeepArchiveBridge.API.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace DeepArchiveBridge.Tests;

public class JwtAuthenticationServiceTests
{
    private const string SecretKey = "SuperSecretKeyThatIsLongEnoughFor256BitSymmetricEncryption";
    private const string Issuer = "DeepArchiveBridge";
    private const string Audience = "DeepArchiveBridge-API";

    [Fact]
    public void GenerateToken_WithValidInputs_ContainsExpectedClaims()
    {
        var service = CreateService();

        var token = service.GenerateToken("user-1", "frontend", "Admin");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal(Issuer, jwt.Issuer);
        Assert.Equal(Audience, jwt.Audiences.Single());
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == "user-1");
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Name && c.Value == "frontend");
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void ValidateToken_WithGeneratedToken_ReturnsTrue()
    {
        var service = CreateService();
        var token = service.GenerateToken("user-2", "tester");

        Assert.True(service.ValidateToken(token));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-jwt")]
    [InlineData("header.payload.signature")]
    public void ValidateToken_WithInvalidToken_ReturnsFalse(string token)
    {
        var service = CreateService();

        Assert.False(service.ValidateToken(token));
    }

    [Fact]
    public void GenerateToken_WithoutSecretKey_Throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Issuer"] = Issuer,
                ["JwtSettings:Audience"] = Audience,
                ["JwtSettings:ExpirationHours"] = "24"
            })
            .Build();

        var service = new JwtAuthenticationService(config, NullLogger<JwtAuthenticationService>.Instance);

        Assert.Throws<InvalidOperationException>(() => service.GenerateToken("user", "name"));
    }

    private static JwtAuthenticationService CreateService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = SecretKey,
                ["JwtSettings:Issuer"] = Issuer,
                ["JwtSettings:Audience"] = Audience,
                ["JwtSettings:ExpirationHours"] = "24"
            })
            .Build();

        return new JwtAuthenticationService(config, NullLogger<JwtAuthenticationService>.Instance);
    }
}
