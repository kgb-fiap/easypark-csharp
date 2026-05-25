using EasyPark.Api.Dtos;
using EasyPark.Api.Observability;
using EasyPark.Application.Abstractions;

namespace EasyPark.Api.Services;

public class JobsService
{
    private readonly IJobRepository _jobRepository;
    private readonly IAuditEventRepository _auditEventRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public JobsService(IJobRepository jobRepository, IAuditEventRepository auditEventRepository, ICurrentUserContext currentUserContext)
    {
        _jobRepository = jobRepository;
        _auditEventRepository = auditEventRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<JobCountOutDto> ReservaTimeoutsAsync(CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("JobsService.ReservaTimeouts");
        var result = await _jobRepository.ReservaTimeoutsAsync(cancellationToken);
        await WriteAuditAsync("JOB_RESERVA_TIMEOUTS", new { result.Canceladas }, cancellationToken);
        return result;
    }

    public async Task<JobCountOutDto> PreReservaTimeoutsAsync(CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("JobsService.PreReservaTimeouts");
        var result = await _jobRepository.PreReservaTimeoutsAsync(cancellationToken);
        await WriteAuditAsync("JOB_PRERESERVA_TIMEOUTS", new { result.Canceladas }, cancellationToken);
        return result;
    }

    public async Task<EtaUpdateOutDto> AtualizarEtaAsync(long id, int minutos, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("JobsService.AtualizarEta");
        var result = await _jobRepository.AtualizarEtaAsync(id, minutos, cancellationToken);
        await WriteAuditAsync("JOB_ETA_UPDATED", new { ReservaId = id, Minutos = minutos, result.Status, result.Msg }, cancellationToken);
        return result;
    }

    private Task WriteAuditAsync(string eventType, object payload, CancellationToken cancellationToken)
        => _auditEventRepository.WriteAsync(new AuditEventWriteDto(
            eventType,
            "Job",
            eventType,
            _currentUserContext.UserId,
            _currentUserContext.CorrelationId,
            payload,
            "JobsService"), cancellationToken);
}
