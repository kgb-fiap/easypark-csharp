using System.Collections.Concurrent;
using System.Text.Json;
using EasyPark.Api.Dtos;
using EasyPark.Application.Abstractions;

namespace EasyPark.Infrastructure.Auditing;

public class InMemoryAuditEventRepository : IAuditEventRepository
{
    private readonly ConcurrentDictionary<string, AuditEventOutDto> _events = new();

    public Task WriteAsync(AuditEventWriteDto auditEvent, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString("N");
        var result = new AuditEventOutDto(
            id,
            auditEvent.OccurredAt ?? DateTimeOffset.UtcNow,
            auditEvent.EventType,
            auditEvent.EntityType,
            auditEvent.EntityId,
            auditEvent.UserId,
            auditEvent.CorrelationId,
            auditEvent.Source,
            JsonSerializer.Serialize(auditEvent.Payload));

        _events[id] = result;
        return Task.CompletedTask;
    }

    public Task<AuditEventOutDto?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _events.TryGetValue(id, out var auditEvent);
        return Task.FromResult(auditEvent);
    }

    public Task<PagedQueryResult<AuditEventOutDto>> SearchAsync(
        int page,
        int pageSize,
        string? eventType,
        string? entityType,
        string? entityId,
        long? userId,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize <= 0 ? 10 : pageSize, 1, 100);

        IEnumerable<AuditEventOutDto> query = _events.Values.OrderByDescending(x => x.OccurredAt);

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            query = query.Where(x => string.Equals(x.EventType, eventType.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(x => string.Equals(x.EntityType, entityType.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            query = query.Where(x => string.Equals(x.EntityId, entityId.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        var totalItems = query.LongCount();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PagedQueryResult<AuditEventOutDto>(items, totalItems));
    }
}
