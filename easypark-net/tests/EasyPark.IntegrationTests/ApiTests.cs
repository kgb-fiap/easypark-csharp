using System.Net;
using System.Net.Http.Json;
using EasyPark.IntegrationTests.Support;

namespace EasyPark.IntegrationTests;

[Collection(ApiCollection.Name)]
public class ApiTests
{
    private readonly EasyParkApiFactory _factory;

    public ApiTests(EasyParkApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealthLive_SemAutenticacao_RetornaHealthy()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task GetVagas_SemToken_RetornaUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/vagas");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetVagas_ComJwt_RetornaOk()
    {
        await _factory.ResetDatabaseAsync();
        using var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/vagas");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("A-01", content);
    }

    [Fact]
    public async Task PostReservas_ComPayloadValido_RetornaCreated()
    {
        await _factory.ResetDatabaseAsync();
        using var client = await _factory.CreateAuthenticatedClientAsync();
        var payload = new
        {
            usuarioId = 1,
            vagaId = 1,
            status = "PRE_RESERVA",
            dataInicio = DateTimeOffset.UtcNow,
            dataFim = (DateTimeOffset?)null,
            eta = (DateTimeOffset?)null,
            vagaBloqueada = false,
            valorPrevisto = 12.5m,
            valorFinal = (decimal?)null
        };

        var response = await client.PostAsJsonAsync("/api/reservas", payload);
        var content = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.Created, content);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task GetReservas_IdInexistente_RetornaNotFound()
    {
        await _factory.ResetDatabaseAsync();
        using var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/reservas/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostVagas_ModelStateInvalido_RetornaBadRequest()
    {
        await _factory.ResetDatabaseAsync();
        using var client = await _factory.CreateAuthenticatedClientAsync();
        var payload = new
        {
            nivelId = 1,
            tipoVagaId = 1,
            ativa = true
        };

        var response = await client.PostAsJsonAsync("/api/vagas", payload);
        var content = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.BadRequest, content);
        Assert.Contains("validationErrors", content);
    }

    [Fact]
    public async Task GetMetrics_SemAutenticacao_RetornaMetricasPrometheus()
    {
        using var client = _factory.CreateClient();
        await client.GetAsync("/health/live");

        var response = await client.GetAsync("/metrics");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("# HELP", content);
    }
}
