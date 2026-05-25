using EasyPark.Api.Data;
using EasyPark.Api.Models;
using EasyPark.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EasyPark.Infrastructure.Repositories;

public class ReservaRepository : IReservaRepository
{
    private readonly EasyParkContext _context;

    public ReservaRepository(EasyParkContext context)
    {
        _context = context;
    }

    public void Add(Reserva reserva) => _context.Reservas.Add(reserva);

    public void Remove(Reserva reserva) => _context.Reservas.Remove(reserva);

    public Task<Reserva?> FindByIdAsync(long id, bool asNoTracking = true, CancellationToken cancellationToken = default)
        => (asNoTracking ? _context.Reservas.AsNoTracking() : _context.Reservas).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<Reserva?> FindTrackedByIdAsync(long id, CancellationToken cancellationToken = default)
        => _context.Reservas.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<PagedQueryResult<Reserva>> SearchAsync(
        int page,
        int pageSize,
        string? sortBy,
        string sortDir,
        long? usuarioId,
        long? vagaId,
        string? status,
        DateTimeOffset? dataInicioDe,
        DateTimeOffset? dataInicioAte,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Reserva> query = _context.Reservas.AsNoTracking();

        if (usuarioId.HasValue)
        {
            query = query.Where(r => r.UsuarioId == usuarioId.Value);
        }

        if (vagaId.HasValue)
        {
            query = query.Where(r => r.VagaId == vagaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var filter = status.Trim().ToUpperInvariant();
            query = query.Where(r => r.Status != null && r.Status.ToUpper() == filter);
        }

        if (dataInicioDe.HasValue)
        {
            query = query.Where(r => r.DataInicio >= dataInicioDe.Value);
        }

        if (dataInicioAte.HasValue)
        {
            query = query.Where(r => r.DataInicio <= dataInicioAte.Value);
        }

        query = ApplyOrdering(query, sortBy, sortDir);

        var totalItems = await query.LongCountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedQueryResult<Reserva>(items, totalItems);
    }

    private static IQueryable<Reserva> ApplyOrdering(IQueryable<Reserva> query, string? sortBy, string sortDir)
    {
        var ascending = sortDir != "desc";
        var key = string.IsNullOrWhiteSpace(sortBy) ? "data" : sortBy.Trim().ToLowerInvariant();

        return key switch
        {
            "usuario" => ascending ? query.OrderBy(r => r.UsuarioId) : query.OrderByDescending(r => r.UsuarioId),
            "vaga" => ascending ? query.OrderBy(r => r.VagaId) : query.OrderByDescending(r => r.VagaId),
            "status" => ascending ? query.OrderBy(r => r.Status) : query.OrderByDescending(r => r.Status),
            _ => ascending ? query.OrderBy(r => r.DataInicio) : query.OrderByDescending(r => r.DataInicio)
        };
    }
}
