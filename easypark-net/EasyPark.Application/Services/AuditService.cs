using EasyPark.Api.Dtos;
using EasyPark.Application.Abstractions;

namespace EasyPark.Api.Services;

public class AuditService
{
    private readonly IAuditEventRepository _auditEventRepository;

    public AuditService(IAuditEventRepository auditEventRepository)
    {
        _auditEventRepository = auditEventRepository;
    }

    public Task<AuditEventOutDto?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        => _auditEventRepository.FindByIdAsync(id, cancellationToken);

    public Task<PagedQueryResult<AuditEventOutDto>> SearchAsync(
        int page,
        int pageSize,
        string? eventType,
        string? entityType,
        string? entityId,
        long? userId,
        CancellationToken cancellationToken = default)
        => _auditEventRepository.SearchAsync(page, pageSize, eventType, entityType, entityId, userId, cancellationToken);
}
