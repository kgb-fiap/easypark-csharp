namespace EasyPark.Application.Abstractions;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    long? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    string? CorrelationId { get; }
}
