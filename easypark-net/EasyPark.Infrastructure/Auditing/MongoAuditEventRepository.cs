using System.Text.Json;
using EasyPark.Api.Dtos;
using EasyPark.Application.Abstractions;
using EasyPark.Infrastructure.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EasyPark.Infrastructure.Auditing;

public class MongoAuditEventRepository : IAuditEventRepository
{
    private readonly IMongoCollection<AuditEventDocument> _collection;

    public MongoAuditEventRepository(IOptions<MongoOptions> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<AuditEventDocument>(settings.AuditCollectionName);
    }

    public async Task WriteAsync(AuditEventWriteDto auditEvent, CancellationToken cancellationToken = default)
    {
        var document = new AuditEventDocument
        {
            OccurredAt = auditEvent.OccurredAt ?? DateTimeOffset.UtcNow,
            EventType = auditEvent.EventType,
            EntityType = auditEvent.EntityType,
            EntityId = auditEvent.EntityId,
            UserId = auditEvent.UserId,
            CorrelationId = auditEvent.CorrelationId,
            Source = auditEvent.Source,
            PayloadJson = JsonSerializer.Serialize(auditEvent.Payload)
        };

        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public async Task<AuditEventOutDto?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var document = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : Map(document);
    }

    public async Task<PagedQueryResult<AuditEventOutDto>> SearchAsync(
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

        var filter = Builders<AuditEventDocument>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            filter &= Builders<AuditEventDocument>.Filter.Eq(x => x.EventType, eventType.Trim());
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            filter &= Builders<AuditEventDocument>.Filter.Eq(x => x.EntityType, entityType.Trim());
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            filter &= Builders<AuditEventDocument>.Filter.Eq(x => x.EntityId, entityId.Trim());
        }

        if (userId.HasValue)
        {
            filter &= Builders<AuditEventDocument>.Filter.Eq(x => x.UserId, userId.Value);
        }

        var totalItems = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var items = await _collection
            .Find(filter)
            .SortByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedQueryResult<AuditEventOutDto>(items.Select(Map).ToList(), totalItems);
    }

    private static AuditEventOutDto Map(AuditEventDocument document)
        => new(
            document.Id,
            document.OccurredAt,
            document.EventType,
            document.EntityType,
            document.EntityId,
            document.UserId,
            document.CorrelationId,
            document.Source,
            document.PayloadJson);
}
