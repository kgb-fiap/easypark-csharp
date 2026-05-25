using EasyPark.Api.Data;
using EasyPark.Api.Models;
using EasyPark.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EasyPark.Infrastructure.Repositories;

public class EstacionamentoRepository : IEstacionamentoRepository
{
    private readonly EasyParkContext _context;

    public EstacionamentoRepository(EasyParkContext context)
    {
        _context = context;
    }

    public void Add(Estacionamento estacionamento) => _context.Estacionamentos.Add(estacionamento);

    public void Remove(Estacionamento estacionamento) => _context.Estacionamentos.Remove(estacionamento);

    public Task<List<Estacionamento>> FindAllWithEnderecoAsync(CancellationToken cancellationToken = default)
        => WithEndereco(_context.Estacionamentos.AsNoTracking()).ToListAsync(cancellationToken);

    public Task<Estacionamento?> FindByIdWithEnderecoAsync(long id, bool asNoTracking = true, CancellationToken cancellationToken = default)
        => WithEndereco(asNoTracking ? _context.Estacionamentos.AsNoTracking() : _context.Estacionamentos)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<Estacionamento?> FindTrackedByIdWithEnderecoAsync(long id, CancellationToken cancellationToken = default)
        => WithEndereco(_context.Estacionamentos).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<PagedQueryResult<Estacionamento>> SearchAsync(
        int page,
        int pageSize,
        string? sortBy,
        string sortDir,
        string? nome,
        string? ufSigla,
        string? cidadeNome,
        string? bairroNome,
        CancellationToken cancellationToken = default)
    {
        var query = WithEndereco(_context.Estacionamentos.AsNoTracking());

        if (!string.IsNullOrWhiteSpace(nome))
        {
            var filter = nome.Trim().ToUpperInvariant();
            query = query.Where(e => e.Nome.ToUpper().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(ufSigla))
        {
            var sigla = ufSigla.Trim().ToUpperInvariant();
            query = query.Where(e => e.Endereco != null && e.Endereco.Bairro != null && e.Endereco.Bairro.Cidade != null && e.Endereco.Bairro.Cidade.Uf != null && e.Endereco.Bairro.Cidade.Uf.Sigla == sigla);
        }

        if (!string.IsNullOrWhiteSpace(cidadeNome))
        {
            var filter = cidadeNome.Trim().ToUpperInvariant();
            query = query.Where(e => e.Endereco != null && e.Endereco.Bairro != null && e.Endereco.Bairro.Cidade != null && e.Endereco.Bairro.Cidade.Nome.ToUpper().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(bairroNome))
        {
            var filter = bairroNome.Trim().ToUpperInvariant();
            query = query.Where(e => e.Endereco != null && e.Endereco.Bairro != null && e.Endereco.Bairro.Nome.ToUpper().Contains(filter));
        }

        query = ApplyOrdering(query, sortBy, sortDir);

        var totalItems = await query.LongCountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedQueryResult<Estacionamento>(items, totalItems);
    }

    private static IQueryable<Estacionamento> WithEndereco(IQueryable<Estacionamento> query)
        => query.Include(e => e.Endereco)
            .ThenInclude(e => e!.Bairro)
            .ThenInclude(b => b!.Cidade)
            .ThenInclude(c => c!.Uf);

    private static IQueryable<Estacionamento> ApplyOrdering(IQueryable<Estacionamento> query, string? sortBy, string sortDir)
    {
        var ascending = sortDir != "desc";
        var key = string.IsNullOrWhiteSpace(sortBy) ? "nome" : sortBy.Trim().ToLowerInvariant();

        return key switch
        {
            "ufsla" or "ufsigla" => ascending
                ? query.OrderBy(e => e.Endereco!.Bairro!.Cidade!.Uf!.Sigla ?? string.Empty)
                : query.OrderByDescending(e => e.Endereco!.Bairro!.Cidade!.Uf!.Sigla ?? string.Empty),
            "cidadenome" or "cidade" => ascending
                ? query.OrderBy(e => e.Endereco!.Bairro!.Cidade!.Nome ?? string.Empty)
                : query.OrderByDescending(e => e.Endereco!.Bairro!.Cidade!.Nome ?? string.Empty),
            _ => ascending
                ? query.OrderBy(e => e.Nome)
                : query.OrderByDescending(e => e.Nome)
        };
    }
}
