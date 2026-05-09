using DeepArchiveBridge.Core.Models;
using DeepArchiveBridge.Core.Interfaces;
using DeepArchiveBridge.API.Controllers;
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

        var getBody = await ReadApiResponse<VendaResponse>(getResponse);
        Assert.Equal("Cliente Integracao", getBody.Dados?.ClienteNome);
        Assert.Equal("Produto Teste", getBody.Dados?.Itens.Single().Descricao);

        var approveResponse = await client.PostAsync($"/api/vendas/{createBody.Dados}/aprovar", null);
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);
    }

    [Fact]
    public async Task AtualizarVenda_PreservaDataCriacaoESincronizaItens()
    {
        var client = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync("/api/vendas", CreateVenda("Cliente Original"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadApiResponse<int>(createResponse);

        var getBeforeResponse = await client.GetAsync($"/api/vendas/{created.Dados}");
        Assert.Equal(HttpStatusCode.OK, getBeforeResponse.StatusCode);
        var before = await ReadApiResponse<VendaResponse>(getBeforeResponse);
        Assert.NotNull(before.Dados);

        var update = new UpdateVendaRequest
        {
            ClienteNome = "Cliente Atualizado",
            ClienteId = before.Dados!.ClienteId,
            DataVenda = DateTime.UtcNow.AddDays(-2),
            Valor = 250m,
            Status = VendaStatus.Pendente,
            Itens = new List<UpdateVendaItemRequest>
            {
                new()
                {
                    Id = before.Dados.Itens.Single().Id,
                    Descricao = "Produto Atualizado",
                    Quantidade = 2,
                    Valor = 100m
                },
                new()
                {
                    Descricao = "Produto Novo",
                    Quantidade = 1,
                    Valor = 50m
                }
            }
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/vendas/{created.Dados}", update);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var getAfterResponse = await client.GetAsync($"/api/vendas/{created.Dados}");
        Assert.Equal(HttpStatusCode.OK, getAfterResponse.StatusCode);
        var after = await ReadApiResponse<VendaResponse>(getAfterResponse);

        Assert.NotNull(after.Dados);
        Assert.Equal(before.Dados.DataCriacao, after.Dados!.DataCriacao);
        Assert.NotNull(after.Dados.DataAtualizacao);
        Assert.Equal("Cliente Atualizado", after.Dados.ClienteNome);
        Assert.Equal(2, after.Dados.Itens.Count);
        Assert.Contains(after.Dados.Itens, item => item.Descricao == "Produto Atualizado" && item.Quantidade == 2);
        Assert.Contains(after.Dados.Itens, item => item.Descricao == "Produto Novo" && item.Valor == 50m);
    }

    [Fact]
    public async Task AprovarVenda_JaConfirmada_RetornaBadRequest()
    {
        var client = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync("/api/vendas", CreateVenda("Cliente Aprovacao"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadApiResponse<int>(createResponse);

        var approveResponse = await client.PostAsync($"/api/vendas/{created.Dados}/aprovar", null);
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);

        var secondApproveResponse = await client.PostAsync($"/api/vendas/{created.Dados}/aprovar", null);
        Assert.Equal(HttpStatusCode.BadRequest, secondApproveResponse.StatusCode);
    }

    [Fact]
    public async Task BuscarVenda_Inexistente_RetornaNotFound()
    {
        var client = await CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/vendas/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CriarVenda_Invalida_RetornaBadRequest()
    {
        var client = await CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/vendas", new CreateVendaRequest
        {
            ClienteNome = "",
            DataVenda = DateTime.UtcNow.AddDays(-1),
            Valor = 0,
            Itens = new List<CreateVendaItemRequest>()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadApiResponse<object>(response);
        Assert.False(body.Sucesso);
        Assert.False(string.IsNullOrWhiteSpace(body.TraceId));
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

    [Fact]
    public async Task Arquivamento_GravaHistoricoReal()
    {
        var client = await CreateAuthenticatedClient();
        var vendaAntiga = CreateVenda("Cliente Log Arquivo", DateTime.UtcNow.AddDays(-130));

        var createResponse = await client.PostAsJsonAsync("/api/vendas", vendaAntiga);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var archiveResponse = await client.PostAsync("/api/arquivamento/executar", null);
        Assert.Equal(HttpStatusCode.OK, archiveResponse.StatusCode);

        var logsResponse = await client.GetAsync("/api/arquivamento/logs");
        Assert.Equal(HttpStatusCode.OK, logsResponse.StatusCode);
        var logs = await ReadApiResponse<List<ArquivamentoLog>>(logsResponse);

        Assert.True(logs.Dados?.Count > 0);
        Assert.Contains(logs.Dados!, log => log.Status == "sucesso" && log.VendasProcessadas >= 1);

        var ultimoResponse = await client.GetAsync("/api/arquivamento/ultimo");
        Assert.Equal(HttpStatusCode.OK, ultimoResponse.StatusCode);
        var ultimo = await ReadApiResponse<ArquivamentoLog>(ultimoResponse);

        Assert.NotNull(ultimo.Dados);
        Assert.Equal("sucesso", ultimo.Dados!.Status);
    }

    [Fact]
    public async Task NavegacaoVenda_RetornaAnteriorEProxima()
    {
        var client = await CreateAuthenticatedClient();

        var primeiraResponse = await client.PostAsJsonAsync("/api/vendas", CreateVenda("Cliente Nav 1", DateTime.UtcNow.AddDays(-3)));
        var segundaResponse = await client.PostAsJsonAsync("/api/vendas", CreateVenda("Cliente Nav 2", DateTime.UtcNow.AddDays(-2)));
        var terceiraResponse = await client.PostAsJsonAsync("/api/vendas", CreateVenda("Cliente Nav 3", DateTime.UtcNow.AddDays(-1)));

        Assert.Equal(HttpStatusCode.Created, primeiraResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, segundaResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, terceiraResponse.StatusCode);

        var primeira = await ReadApiResponse<int>(primeiraResponse);
        var segunda = await ReadApiResponse<int>(segundaResponse);
        var terceira = await ReadApiResponse<int>(terceiraResponse);

        var navigationResponse = await client.GetAsync($"/api/vendas/{segunda.Dados}/navigation");
        Assert.Equal(HttpStatusCode.OK, navigationResponse.StatusCode);
        var navigation = await ReadApiResponse<VendaNavigationResponse>(navigationResponse);

        Assert.Equal(segunda.Dados, navigation.Dados?.VendaId);
        Assert.Equal(primeira.Dados, navigation.Dados?.AnteriorId);
        Assert.Equal(terceira.Dados, navigation.Dados?.ProximaId);
    }

    [Fact]
    public async Task Health_RetornaDependenciasRegistradas()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadApiResponse<HealthStatus>(response);

        Assert.True(body.Sucesso);
        Assert.Equal("Healthy", body.Dados?.Status);
        Assert.True(body.Dados?.DependenciesHealthy >= 1);
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
