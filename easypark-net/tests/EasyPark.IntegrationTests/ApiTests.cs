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
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/live");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task GetVagas_SemApiKey_RetornaUnauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/vagas");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetVagas_ComApiKey_RetornaOk()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        using var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/vagas");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("A-01", content);
    }

    [Fact]
    public async Task PostReservas_ComPayloadValido_RetornaCreated()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        using var client = _factory.CreateAuthenticatedClient();
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

        // Act
        var response = await client.PostAsJsonAsync("/api/reservas", payload);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Created, content);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task GetReservas_IdInexistente_RetornaNotFound()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        using var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/reservas/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostVagas_ModelStateInvalido_RetornaBadRequest()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        using var client = _factory.CreateAuthenticatedClient();
        var payload = new
        {
            nivelId = 1,
            tipoVagaId = 1,
            ativa = true
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/vagas", payload);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest, content);
        Assert.Contains("validationErrors", content);
    }

    [Fact]
    public async Task GetMetrics_SemAutenticacao_RetornaMetricasPrometheus()
    {
        // Arrange
        using var client = _factory.CreateClient();
        await client.GetAsync("/health/live");

        // Act
        var response = await client.GetAsync("/metrics");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("# HELP", content);
    }
}
