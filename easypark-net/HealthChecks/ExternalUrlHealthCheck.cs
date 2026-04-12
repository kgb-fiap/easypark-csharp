using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EasyPark.Api.HealthChecks;

public class ExternalUrlHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public ExternalUrlHealthCheck(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var url = _configuration["ExternalServices:Eta:HealthUrl"];
        if (string.IsNullOrWhiteSpace(url))
        {
            return HealthCheckResult.Degraded("URL de health do servico externo nao configurada.");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _httpClientFactory
                .CreateClient("external-health")
                .SendAsync(request, cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Servico externo disponivel.")
                : HealthCheckResult.Unhealthy($"Servico externo retornou HTTP {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha ao validar servico externo.", ex);
        }
    }
}
