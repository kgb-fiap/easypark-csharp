using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EasyPark.Infrastructure.Auditing;

public class AuditEventDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public DateTimeOffset OccurredAt { get; set; }

    public string EventType { get; set; } = null!;

    public string EntityType { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public long? UserId { get; set; }

    public string? CorrelationId { get; set; }

    public string Source { get; set; } = null!;

    public string PayloadJson { get; set; } = null!;
}
