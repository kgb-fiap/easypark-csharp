using System.Net;
using EasyPark.Api.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Moq.Protected;

namespace EasyPark.UnitTests.HealthChecks;

public class ExternalUrlHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_RespostaSucesso_RetornaHealthy()
    {
        // Arrange
        var healthCheck = CreateHealthCheck(HttpStatusCode.OK);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_RespostaErro_RetornaUnhealthy()
    {
        // Arrange
        var healthCheck = CreateHealthCheck(HttpStatusCode.InternalServerError);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    private static ExternalUrlHealthCheck CreateHealthCheck(HttpStatusCode statusCode)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ExternalServices:Eta:HealthUrl"] = "https://example.com/health"
            })
            .Build();

        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode));

        var client = new HttpClient(handler.Object);
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient("external-health")).Returns(client);

        return new ExternalUrlHealthCheck(configuration, factory.Object);
    }
}
