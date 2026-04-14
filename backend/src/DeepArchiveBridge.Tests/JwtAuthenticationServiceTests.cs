using DeepArchiveBridge.API.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace DeepArchiveBridge.Tests;

/// <summary>
/// Testes para JwtAuthenticationService
/// Cobre: geração de tokens, validação, expiração, configurações
/// </summary>
public class JwtAuthenticationServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<JwtAuthenticationService>> _mockLogger;
    private readonly IAuthenticationService _authService;

    private const string ValidSecretKey = "SuperSecretKeyThatIsLongEnoughFor256BitSymmetricEncryption";
    private const string ValidIssuer = "DeepArchiveBridge";
    private const string ValidAudience = "DeepArchiveAPI";
    private const int ExpirationHours = 24;

    public JwtAuthenticationServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<JwtAuthenticationService>>();
        
        ConfigureMocks();
        _authService = new JwtAuthenticationService(_mockConfiguration.Object, _mockLogger.Object);
    }

    private void ConfigureMocks()
    {
        var jwtSectionMock = new Mock<IConfigurationSection>();
        
        jwtSectionMock.Setup(x => x["SecretKey"]).Returns(ValidSecretKey);
        jwtSectionMock.Setup(x => x["Issuer"]).Returns(ValidIssuer);
        jwtSectionMock.Setup(x => x["Audience"]).Returns(ValidAudience);
        jwtSectionMock.Setup(x => x["ExpirationHours"]).Returns(ExpirationHours.ToString());
        
        _mockConfiguration.Setup(x => x.GetSection("JwtSettings"))
            .Returns(jwtSectionMock.Object);
    }

    #region GenerateToken Tests

    [Fact]
    public void GenerateToken_WithValidInputs_ShouldReturnValidToken()
    {
        // Arrange
        string userId = "user123";
        string userName = "john.doe";
        string role = "Admin";

        // Act
        var token = _authService.GenerateToken(userId, userName, role);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
        
        Assert.NotNull(jwtToken);
        Assert.Equal(ValidIssuer, jwtToken.Issuer);
        Assert.Equal(ValidAudience, jwtToken.Audiences.First());
    }

    [Fact]
    public void GenerateToken_ShouldIncludeCorrectClaims()
    {
        // Arrange
        string userId = "user123";
        string userName = "john.doe";
        string role = "Manager";

        // Act
        var token = _authService.GenerateToken(userId, userName, role);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        var userIdClaim = jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
        var userNameClaim = jwtToken.Claims.First(c => c.Type == ClaimTypes.Name);
        var roleClaim = jwtToken.Claims.First(c => c.Type == ClaimTypes.Role);

        Assert.Equal(userId, userIdClaim.Value);
        Assert.Equal(userName, userNameClaim.Value);
        Assert.Equal(role, roleClaim.Value);
    }

    [Fact]
    public void GenerateToken_WithDefaultRole_ShouldUseUserRole()
    {
        // Arrange
        string userId = "user456";
        string userName = "jane.smith";
        // role não fornecido, deve usar "User" como padrão

        // Act
        var token = _authService.GenerateToken(userId, userName);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        var roleClaim = jwtToken.Claims.First(c => c.Type == ClaimTypes.Role);
        Assert.Equal("User", roleClaim.Value);
    }

    [Fact]
    public void GenerateToken_ShouldHaveCorrectExpiration()
    {
        // Arrange
        string userId = "user789";
        string userName = "test.user";

        // Act
        var token = _authService.GenerateToken(userId, userName);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        var expirationTime = jwtToken.ValidTo;
        var now = DateTime.UtcNow;
        var expectedExpiration = now.AddHours(ExpirationHours);

        // Verificar se a expiração está dentro de 1 minuto da esperada
        Assert.True((expirationTime - expectedExpiration).TotalSeconds < 60);
    }

    [Fact]
    public void GenerateToken_WithDifferentRoles_ShouldProduceCorrectClaims()
    {
        // Teste com múltiplos roles
        string[][] testCases = new[]
        {
            new[] { "user1", "name1", "Admin" },
            new[] { "user2", "name2", "Manager" },
            new[] { "user3", "name3", "Viewer" }
        };

        foreach (var testCase in testCases)
        {
            // Act
            var token = _authService.GenerateToken(testCase[0], testCase[1], testCase[2]);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            var roleClaim = jwtToken.Claims.First(c => c.Type == ClaimTypes.Role);
            Assert.Equal(testCase[2], roleClaim.Value);
        }
    }

    [Fact]
    public void GenerateToken_MissingSecretKey_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x["SecretKey"]).Returns((string)null);
        mockConfig.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSection.Object);
        
        var service = new JwtAuthenticationService(mockConfig.Object, _mockLogger.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            service.GenerateToken("user", "name"));
    }

    #endregion

    #region ValidateToken Tests

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var validToken = _authService.GenerateToken("user1", "name1", "User");

        // Act
        var result = _authService.ValidateToken(validToken);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateToken_WithInvalidSignature_ShouldReturnFalse()
    {
        // Arrange
        var validToken = _authService.GenerateToken("user1", "name1", "User");
        // Corromper o token modificando os últimos caracteres
        var invalidToken = validToken.Substring(0, validToken.Length - 10) + "corrupted!";

        // Act
        var result = _authService.ValidateToken(invalidToken);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ShouldReturnFalse()
    {
        // Arrange - Criar um token com expiração muito baixa
        var mockConfigExpired = new Mock<IConfiguration>();
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x["SecretKey"]).Returns(ValidSecretKey);
        jwtSection.Setup(x => x["Issuer"]).Returns(ValidIssuer);
        jwtSection.Setup(x => x["Audience"]).Returns(ValidAudience);
        jwtSection.Setup(x => x["ExpirationHours"]).Returns((-1).ToString()); // Expirado
        
        mockConfigExpired.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSection.Object);
        var serviceExpired = new JwtAuthenticationService(mockConfigExpired.Object, _mockLogger.Object);
        
        var expiredToken = serviceExpired.GenerateToken("user1", "name1");
        
        // Esperar um pouco para garantir que expirou
        System.Threading.Thread.Sleep(100);

        // Act
        var result = _authService.ValidateToken(expiredToken);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateToken_WithMalformedToken_ShouldReturnFalse()
    {
        // Arrange
        string malformedToken = "not.a.valid.jwt.token";

        // Act
        var result = _authService.ValidateToken(malformedToken);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateToken_WithEmptyToken_ShouldReturnFalse()
    {
        // Arrange
        string emptyToken = string.Empty;

        // Act
        var result = _authService.ValidateToken(emptyToken);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateToken_WithNullToken_ShouldReturnFalse()
    {
        // Arrange & Act & Assert
        var result = _authService.ValidateToken(null);
        Assert.False(result);
    }

    [Fact]
    public void ValidateToken_ShouldLogWarningOnInvalidToken()
    {
        // Arrange
        string invalidToken = "invalid.token.here";

        // Act
        _authService.ValidateToken(invalidToken);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Token validation failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void GenerateToken_Then_ValidateToken_ShouldSucceed()
    {
        // Arrange
        string userId = "integration_user";
        string userName = "integration_test";
        string role = "Tester";

        // Act
        var generatedToken = _authService.GenerateToken(userId, userName, role);
        var isValid = _authService.ValidateToken(generatedToken);

        // Assert
        Assert.True(isValid, "Token generated and validated should return true");
    }

    [Fact]
    public void MultipleTokenGeneration_ShouldProduceDifferentTokens()
    {
        // Arrange & Act
        var token1 = _authService.GenerateToken("user1", "name1");
        var token2 = _authService.GenerateToken("user1", "name1");

        // Assert
        Assert.NotEqual(token1, token2);
        Assert.True(_authService.ValidateToken(token1));
        Assert.True(_authService.ValidateToken(token2));
    }

    #endregion
}
