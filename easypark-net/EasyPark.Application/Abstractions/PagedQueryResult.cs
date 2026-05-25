namespace EasyPark.Application.Abstractions;

public record PagedQueryResult<T>(IReadOnlyList<T> Items, long TotalItems);
