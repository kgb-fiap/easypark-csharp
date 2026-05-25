using EasyPark.Api.Data;
using EasyPark.Api.Models;
using EasyPark.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EasyPark.Infrastructure.Repositories;

public class VagaRepository : IVagaRepository
{
    private readonly EasyParkContext _context;

    public VagaRepository(EasyParkContext context)
    {
        _context = context;
    }

    public void Add(Vaga vaga) => _context.Vagas.Add(vaga);

    public void Remove(Vaga vaga) => _context.Vagas.Remove(vaga);

    public Task<bool> NivelExistsAsync(long nivelId, CancellationToken cancellationToken = default)
        => _context.Niveis.AnyAsync(x => x.Id == nivelId, cancellationToken);

    public Task<bool> TipoVagaExistsAsync(long tipoVagaId, CancellationToken cancellationToken = default)
        => _context.TiposVaga.AnyAsync(x => x.Id == tipoVagaId, cancellationToken);

    public Task<bool> ExistsCodigoNoNivelAsync(long nivelId, string codigo, long? excludingId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Vagas.AsQueryable().Where(v => v.NivelId == nivelId && v.Codigo.ToLower() == codigo.ToLower());
        if (excludingId.HasValue)
        {
            query = query.Where(v => v.Id != excludingId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task<List<Vaga>> FindAllAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Vaga> query = _context.Vagas.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
        {
            var filter = status.Trim().ToUpperInvariant();
            query = query.Where(v => _context.VagaStatus.Any(s => s.VagaId == v.Id && s.StatusOcupacao!.ToUpper() == filter));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<Vaga?> FindByIdAsync(long id, bool asNoTracking = true, CancellationToken cancellationToken = default)
        => (asNoTracking ? _context.Vagas.AsNoTracking() : _context.Vagas).FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<PagedQueryResult<Vaga>> SearchAsync(
        int page,
        int pageSize,
        string? sortBy,
        string sortDir,
        long? estacionamentoId,
        long? nivelId,
        long? tipoVagaId,
        string? status,
        string? codigo,
        CancellationToken cancellationToken = default)
    {
        var query = from v in _context.Vagas.AsNoTracking()
                    join n in _context.Niveis.AsNoTracking() on v.NivelId equals n.Id
                    join t in _context.TiposVaga.AsNoTracking() on v.TipoVagaId equals t.Id
                    join vs in _context.VagaStatus.AsNoTracking() on v.Id equals vs.VagaId into statusGroup
                    from vs in statusGroup.DefaultIfEmpty()
                    select new VagaSearchProjection(v, n, t, vs);

        if (estacionamentoId.HasValue)
        {
            query = query.Where(x => x.Nivel.EstacionamentoId == estacionamentoId.Value);
        }

        if (nivelId.HasValue)
        {
            query = query.Where(x => x.Vaga.NivelId == nivelId.Value);
        }

        if (tipoVagaId.HasValue)
        {
            query = query.Where(x => x.Vaga.TipoVagaId == tipoVagaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var filter = status.Trim().ToUpperInvariant();
            query = query.Where(x => x.Status != null && x.Status.StatusOcupacao != null && x.Status.StatusOcupacao.ToUpper() == filter);
        }

        if (!string.IsNullOrWhiteSpace(codigo))
        {
            var filter = codigo.Trim().ToUpperInvariant();
            query = query.Where(x => x.Vaga.Codigo.ToUpper().Contains(filter));
        }

        query = ApplyOrdering(query, sortBy, sortDir);

        var totalItems = await query.LongCountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(x => x.Vaga).ToListAsync(cancellationToken);

        return new PagedQueryResult<Vaga>(items, totalItems);
    }

    public Task<VagaStatus?> GetStatusAsync(long vagaId, CancellationToken cancellationToken = default)
        => _context.VagaStatus.AsNoTracking().FirstOrDefaultAsync(x => x.VagaId == vagaId, cancellationToken);

    public async Task<List<Vaga>> FindByEstacionamentoAsync(long estacionamentoId, CancellationToken cancellationToken = default)
    {
        var query = from v in _context.Vagas.AsNoTracking()
                    join n in _context.Niveis.AsNoTracking() on v.NivelId equals n.Id
                    where n.EstacionamentoId == estacionamentoId
                    select v;

        return await query.ToListAsync(cancellationToken);
    }

    private static IQueryable<VagaSearchProjection> ApplyOrdering(IQueryable<VagaSearchProjection> query, string? sortBy, string sortDir)
    {
        var ascending = sortDir != "desc";
        var key = string.IsNullOrWhiteSpace(sortBy) ? "codigo" : sortBy.Trim().ToLowerInvariant();

        return key switch
        {
            "nivel" => ascending ? query.OrderBy(x => x.Nivel.Nome) : query.OrderByDescending(x => x.Nivel.Nome),
            "tipo" => ascending ? query.OrderBy(x => x.Tipo.Nome) : query.OrderByDescending(x => x.Tipo.Nome),
            _ => ascending ? query.OrderBy(x => x.Vaga.Codigo) : query.OrderByDescending(x => x.Vaga.Codigo)
        };
    }

    private record VagaSearchProjection(Vaga Vaga, Nivel Nivel, TipoVaga Tipo, VagaStatus? Status);
}
