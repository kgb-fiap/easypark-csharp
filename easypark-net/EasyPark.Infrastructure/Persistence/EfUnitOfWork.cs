using EasyPark.Api.Data;
using EasyPark.Application.Abstractions;

namespace EasyPark.Infrastructure.Persistence;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly EasyParkContext _context;

    public EfUnitOfWork(EasyParkContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
