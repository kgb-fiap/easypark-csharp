using EasyPark.Application.Abstractions;

namespace EasyPark.UnitTests.TestSupport;

public class TestCurrentUserContext : ICurrentUserContext
{
    public bool IsAuthenticated { get; init; } = true;
    public bool IsAdmin { get; init; }
    public long? UserId { get; init; }
    public string? Email { get; init; }
    public string? Role { get; init; }
    public string? CorrelationId { get; init; } = "unit-test";
}
