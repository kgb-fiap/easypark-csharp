using EasyPark.Infrastructure.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EasyPark.Api.HealthChecks;

public class MongoHealthCheck : IHealthCheck
{
    private readonly MongoOptions _options;

    public MongoHealthCheck(IOptions<MongoOptions> options)
    {
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            return HealthCheckResult.Degraded("MongoDB não configurado; auditoria persistida em memória.");
        }

        try
        {
            var client = new MongoClient(_options.ConnectionString);
            var database = client.GetDatabase(_options.DatabaseName);
            await database.RunCommandAsync((Command<BsonDocument>)"{ping:1}", cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("MongoDB disponível.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha ao validar conexão com MongoDB.", ex);
        }
    }
}
