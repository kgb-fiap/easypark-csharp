using EasyPark.Api.Data;
using EasyPark.Api.Models;
using EasyPark.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EasyPark.Infrastructure.Repositories;

public class PagamentoRepository : IPagamentoRepository
{
    private readonly EasyParkContext _context;

    public PagamentoRepository(EasyParkContext context)
    {
        _context = context;
    }

    public void Add(Pagamento pagamento) => _context.Pagamentos.Add(pagamento);

    public void AddPagador(PagamentoPagador pagador) => _context.PagamentoPagadores.Add(pagador);

    public void AddCartao(PagamentoCartao cartao) => _context.PagamentoCartoes.Add(cartao);

    public Task<Pagamento?> FindByIdAsync(long id, bool asNoTracking = true, CancellationToken cancellationToken = default)
        => (asNoTracking ? _context.Pagamentos.AsNoTracking() : _context.Pagamentos).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<PagedQueryResult<Pagamento>> SearchAsync(
        int page,
        int pageSize,
        string? sortBy,
        string sortDir,
        long? reservaId,
        long? usuarioId,
        string? status,
        string? metodo,
        CancellationToken cancellationToken = default)
    {
        var query = from p in _context.Pagamentos.AsNoTracking()
                    join c in _context.PagamentoCartoes.AsNoTracking() on p.Id equals c.PagamentoId into cartaoGroup
                    from c in cartaoGroup.DefaultIfEmpty()
                    select new PagamentoSearchProjection(p, c != null);

        if (reservaId.HasValue)
        {
            query = query.Where(x => x.Pagamento.ReservaId == reservaId.Value);
        }

        if (usuarioId.HasValue)
        {
            query = query.Where(x => x.Pagamento.UsuarioId == usuarioId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var filter = status.Trim().ToUpperInvariant();
            query = query.Where(x => x.Pagamento.Status != null && x.Pagamento.Status.ToUpper() == filter);
        }

        if (!string.IsNullOrWhiteSpace(metodo))
        {
            var metodoFilter = metodo.Trim().ToLowerInvariant();
            query = metodoFilter switch
            {
                "cartao" => query.Where(x => x.PossuiCartao),
                "manual" or "offline" => query.Where(x => !x.PossuiCartao),
                _ => query
            };
        }

        query = ApplyOrdering(query, sortBy, sortDir);

        var totalItems = await query.LongCountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(x => x.Pagamento).ToListAsync(cancellationToken);
        return new PagedQueryResult<Pagamento>(items, totalItems);
    }

    public Task<PagamentoPagador?> FindPagadorByPagamentoIdAsync(long pagamentoId, CancellationToken cancellationToken = default)
        => _context.PagamentoPagadores.AsNoTracking()
            .Include(p => p.Endereco)!
                .ThenInclude(e => e!.Bairro)!
                    .ThenInclude(b => b!.Cidade)!
                        .ThenInclude(c => c!.Uf)
            .FirstOrDefaultAsync(p => p.PagamentoId == pagamentoId, cancellationToken);

    public Task<PagamentoCartao?> FindCartaoByPagamentoIdAsync(long pagamentoId, CancellationToken cancellationToken = default)
        => _context.PagamentoCartoes.AsNoTracking().FirstOrDefaultAsync(x => x.PagamentoId == pagamentoId, cancellationToken);

    public async Task<Dictionary<long, PagamentoPagador>> FindPagadoresByPagamentoIdsAsync(IEnumerable<long> pagamentoIds, CancellationToken cancellationToken = default)
    {
        var ids = pagamentoIds.ToList();
        var items = await _context.PagamentoPagadores.AsNoTracking()
            .Where(p => ids.Contains(p.PagamentoId))
            .Include(p => p.Endereco)!
                .ThenInclude(e => e!.Bairro)!
                    .ThenInclude(b => b!.Cidade)!
                        .ThenInclude(c => c!.Uf)
            .ToListAsync(cancellationToken);

        return items.ToDictionary(x => x.PagamentoId, x => x);
    }

    public async Task<Dictionary<long, PagamentoCartao>> FindCartoesByPagamentoIdsAsync(IEnumerable<long> pagamentoIds, CancellationToken cancellationToken = default)
    {
        var ids = pagamentoIds.ToList();
        var items = await _context.PagamentoCartoes.AsNoTracking()
            .Where(c => ids.Contains(c.PagamentoId))
            .ToListAsync(cancellationToken);

        return items.ToDictionary(x => x.PagamentoId, x => x);
    }

    private static IQueryable<PagamentoSearchProjection> ApplyOrdering(IQueryable<PagamentoSearchProjection> query, string? sortBy, string sortDir)
    {
        var ascending = sortDir != "desc";
        var key = string.IsNullOrWhiteSpace(sortBy) ? "data" : sortBy.Trim().ToLowerInvariant();

        return key switch
        {
            "valor" => ascending ? query.OrderBy(x => x.Pagamento.Valor) : query.OrderByDescending(x => x.Pagamento.Valor),
            "status" => ascending ? query.OrderBy(x => x.Pagamento.Status) : query.OrderByDescending(x => x.Pagamento.Status),
            "reserva" => ascending ? query.OrderBy(x => x.Pagamento.ReservaId) : query.OrderByDescending(x => x.Pagamento.ReservaId),
            "usuario" => ascending ? query.OrderBy(x => x.Pagamento.UsuarioId) : query.OrderByDescending(x => x.Pagamento.UsuarioId),
            _ => ascending ? query.OrderBy(x => x.Pagamento.CriadoEm) : query.OrderByDescending(x => x.Pagamento.CriadoEm)
        };
    }

    private record PagamentoSearchProjection(Pagamento Pagamento, bool PossuiCartao);
}
