namespace EasyPark.Api.Dtos;

public record AuditEventWriteDto(
    string EventType,
    string EntityType,
    string EntityId,
    long? UserId,
    string? CorrelationId,
    object Payload,
    string Source,
    DateTimeOffset? OccurredAt = null);

public record AuditEventOutDto(
    string Id,
    DateTimeOffset OccurredAt,
    string EventType,
    string EntityType,
    string EntityId,
    long? UserId,
    string? CorrelationId,
    string Source,
    string PayloadJson);
