using EasyPark.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EasyPark.Api.HealthChecks;

public class OracleHealthCheck : IHealthCheck
{
    private readonly EasyParkContext _context;

    public OracleHealthCheck(EasyParkContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Conexao com Oracle disponivel.")
                : HealthCheckResult.Unhealthy("Nao foi possivel conectar ao Oracle.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha ao validar conexao com Oracle.", ex);
        }
    }
}
