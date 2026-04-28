using DeepArchiveBridge.Core.Models;
using DeepArchiveBridge.Core.Interfaces;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace DeepArchiveBridge.Tests;

public class VendaControllerTests : IClassFixture<TestApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly TestApiFactory _factory;

    public VendaControllerTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CriarVenda_SemToken_RetornaUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/vendas", CreateVenda("Cliente sem token"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CriarBuscarEAprovarVenda_ComToken_Funciona()
    {
        var client = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync("/api/vendas", CreateVenda("Cliente Integracao"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createBody = await ReadApiResponse<int>(createResponse);
        Assert.True(createBody.Sucesso);
        Assert.True(createBody.Dados > 0);

        var getResponse = await client.GetAsync($"/api/vendas/{createBody.Dados}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getBody = await ReadApiResponse<Venda>(getResponse);
        Assert.Equal("Cliente Integracao", getBody.Dados?.ClienteNome);

        var approveResponse = await client.PostAsync($"/api/vendas/{createBody.Dados}/aprovar", null);
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);
    }

    [Fact]
    public async Task Arquivamento_DeVendaAntiga_NaoRemoveRegistro()
    {
        var client = await CreateAuthenticatedClient();
        var vendaAntiga = CreateVenda("Cliente Arquivo", DateTime.UtcNow.AddDays(-120));

        var createResponse = await client.PostAsJsonAsync("/api/vendas", vendaAntiga);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadApiResponse<int>(createResponse);

        var infoResponse = await client.GetAsync("/api/arquivamento/info");
        Assert.Equal(HttpStatusCode.OK, infoResponse.StatusCode);
        var info = await ReadApiResponse<ArquivamentoInfo>(infoResponse);
        Assert.True(info.Dados?.VendasParaArquivar >= 1);

        var archiveResponse = await client.PostAsync("/api/arquivamento/executar", null);
        Assert.Equal(HttpStatusCode.OK, archiveResponse.StatusCode);

        var getAfterArchive = await client.GetAsync($"/api/vendas/{created.Dados}");
        Assert.Equal(HttpStatusCode.OK, getAfterArchive.StatusCode);
    }

    private async Task<HttpClient> CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();
        var tokenResponse = await client.PostAsync("/api/auth/token?clienteId=test-client", null);
        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);

        var tokenBody = await ReadApiResponse<TokenResponse>(tokenResponse);
        Assert.NotNull(tokenBody.Dados?.Token);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenBody.Dados.Token);
        return client;
    }

    private static CreateVendaRequest CreateVenda(string clienteNome, DateTime? dataVenda = null)
    {
        return new CreateVendaRequest
        {
            ClienteNome = clienteNome,
            DataVenda = dataVenda ?? DateTime.UtcNow.AddDays(-1),
            Valor = 150.50m,
            Itens = new List<CreateVendaItemRequest>
            {
                new()
                {
                    Descricao = "Produto Teste",
                    Quantidade = 1,
                    Valor = 150.50m
                }
            }
        };
    }

    private static async Task<ApiResponse<T>> ReadApiResponse<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<ApiResponse<T>>(body, JsonOptions);
        Assert.NotNull(parsed);
        return parsed;
    }

    private sealed class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = string.Empty;
    }
}
