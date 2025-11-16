using System;
using System.Linq;
using System.Threading.Tasks;
using EasyPark.Api.Data;
using EasyPark.Api.Dtos;
using EasyPark.Api.Exceptions;
using EasyPark.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyPark.Api.Services;

// Serviço responsável pelo ciclo de vida dos pagamentos.
public class PagamentoService
{
    private readonly EasyParkContext _context;

    public PagamentoService(EasyParkContext context)
    {
        _context = context;
    }

    public async Task<PagamentoOutDto> CreateAsync(PagamentoInDto dto)
    {
        if (dto.ReservaId.HasValue)
        {
            _ = await _context.Reservas.FindAsync(dto.ReservaId.Value)
                ?? throw new EntityNotFoundException($"Reserva {dto.ReservaId.Value} não encontrada");
        }

        if (dto.UsuarioId.HasValue)
        {
            _ = await _context.Usuarios.FindAsync(dto.UsuarioId.Value)
                ?? throw new EntityNotFoundException($"Usuário {dto.UsuarioId.Value} não encontrado");
        }

        var pagamento = new Pagamento
        {
            ReservaId = dto.ReservaId,
            UsuarioId = dto.UsuarioId,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "PENDENTE" : dto.Status!.Trim().ToUpperInvariant(),
            Valor = dto.Valor,
            IdempotenciaChave = dto.IdempotenciaChave,
            CriadoEm = DateTimeOffset.UtcNow
        };

        _context.Pagamentos.Add(pagamento);
        await _context.SaveChangesAsync();

        PagamentoPagador? pagador = null;
        if (dto.Pagador is not null)
        {
            pagador = new PagamentoPagador
            {
                PagamentoId = pagamento.Id,
                CpfCnpj = dto.Pagador.CpfCnpj,
                Nome = dto.Pagador.Nome
            };

            if (dto.Pagador.Endereco is not null)
            {
                pagador.Endereco = await UpsertEnderecoAsync(dto.Pagador.Endereco);
            }

            _context.PagamentoPagadores.Add(pagador);
            await _context.SaveChangesAsync();
        }

        PagamentoCartao? cartao = null;
        if (dto.Cartao is not null)
        {
            cartao = new PagamentoCartao
            {
                PagamentoId = pagamento.Id,
                Titular = dto.Cartao.Titular,
                Bandeira = dto.Cartao.Bandeira,
                UltimosDigitos = dto.Cartao.UltimosDigitos,
                TransacaoId = dto.Cartao.TransacaoId
            };
            _context.PagamentoCartoes.Add(cartao);
            await _context.SaveChangesAsync();
        }

        return MapPagamento(pagamento, pagador, cartao);
    }

    public async Task<PagamentoOutDto> FindByIdAsync(long id)
    {
        var pagamento = await _context.Pagamentos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new EntityNotFoundException($"Pagamento {id} não encontrado");

        var pagador = await _context.PagamentoPagadores.AsNoTracking()
            .Include(p => p.Endereco)!
                .ThenInclude(e => e!.Bairro)!
                    .ThenInclude(b => b!.Cidade)!
                        .ThenInclude(c => c!.Uf)
            .FirstOrDefaultAsync(p => p.PagamentoId == id);

        var cartao = await _context.PagamentoCartoes.AsNoTracking().FirstOrDefaultAsync(c => c.PagamentoId == id);

        return MapPagamento(pagamento, pagador, cartao);
    }

    public async Task<PagedResultDto<PagamentoOutDto>> SearchAsync(
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir,
        long? reservaId,
        long? usuarioId,
        string? status,
        string? metodo)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize <= 0 ? 10 : pageSize, 1, 100);
        sortDir = string.IsNullOrWhiteSpace(sortDir) ? "asc" : sortDir.Trim().ToLowerInvariant();

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
            var statusFilter = status.Trim().ToUpperInvariant();
            query = query.Where(x => x.Pagamento.Status != null && x.Pagamento.Status.ToUpper() == statusFilter);
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

        var totalItems = await query.LongCountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var pagamentos = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(x => x.Pagamento).ToListAsync();

        var pagamentoIds = pagamentos.Select(pg => pg.Id).ToList();

        var pagadores = await _context.PagamentoPagadores.AsNoTracking()
            .Where(p => pagamentoIds.Contains(p.PagamentoId))
            .Include(p => p.Endereco)!
                .ThenInclude(e => e!.Bairro)!
                    .ThenInclude(b => b!.Cidade)!
                        .ThenInclude(c => c!.Uf)
            .ToListAsync();

        var cartoes = await _context.PagamentoCartoes.AsNoTracking()
            .Where(c => pagamentoIds.Contains(c.PagamentoId))
            .ToListAsync();

        var items = pagamentos
            .Select(pg => MapPagamento(
                pg,
                pagadores.FirstOrDefault(p => p.PagamentoId == pg.Id),
                cartoes.FirstOrDefault(c => c.PagamentoId == pg.Id)))
            .ToList();

        return new PagedResultDto<PagamentoOutDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = items
        };
    }

    private async Task<Endereco> UpsertEnderecoAsync(EnderecoInDto dto)
    {
        var ufSigla = dto.Uf.Trim().ToUpperInvariant();
        var ufNome = string.IsNullOrWhiteSpace(dto.UfNome) ? ufSigla : dto.UfNome.Trim();

        var uf = await _context.Ufs.FindAsync(ufSigla);
        if (uf is null)
        {
            uf = new Uf { Sigla = ufSigla, Nome = ufNome };
            _context.Ufs.Add(uf);
        }
        else if (!string.IsNullOrWhiteSpace(dto.UfNome) && !string.Equals(uf.Nome, ufNome, StringComparison.OrdinalIgnoreCase))
        {
            uf.Nome = ufNome;
        }

        var cidadeNome = dto.Cidade.Trim();
        var cidade = await _context.Cidades.FirstOrDefaultAsync(c => c.Nome == cidadeNome && c.UfSigla == ufSigla);
        if (cidade is null)
        {
            cidade = new Cidade { Nome = cidadeNome, UfSigla = ufSigla, Uf = uf };
            _context.Cidades.Add(cidade);
        }

        Bairro? bairro = null;
        if (cidade.Id > 0)
        {
            bairro = await _context.Bairros.FirstOrDefaultAsync(b => b.Nome == dto.Bairro && b.CidadeId == cidade.Id);
        }

        if (bairro is null)
        {
            bairro = new Bairro { Nome = dto.Bairro.Trim(), Cidade = cidade };
            _context.Bairros.Add(bairro);
        }

        var endereco = new Endereco
        {
            Cep = dto.Cep,
            Logradouro = dto.Logradouro.Trim(),
            Numero = dto.Numero,
            Complemento = dto.Complemento,
            Bairro = bairro,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        };

        _context.Enderecos.Add(endereco);
        await _context.SaveChangesAsync();
        return endereco;
    }

    private static IQueryable<PagamentoSearchProjection> ApplyOrdering(IQueryable<PagamentoSearchProjection> query, string? sortBy, string sortDir)
    {
        var ascending = sortDir != "desc";
        var key = string.IsNullOrWhiteSpace(sortBy) ? "data" : sortBy.Trim().ToLowerInvariant();

        return key switch
        {
            "valor" => ascending
                ? query.OrderBy(x => x.Pagamento.Valor)
                : query.OrderByDescending(x => x.Pagamento.Valor),
            "status" => ascending
                ? query.OrderBy(x => x.Pagamento.Status)
                : query.OrderByDescending(x => x.Pagamento.Status),
            "reserva" => ascending
                ? query.OrderBy(x => x.Pagamento.ReservaId)
                : query.OrderByDescending(x => x.Pagamento.ReservaId),
            "usuario" => ascending
                ? query.OrderBy(x => x.Pagamento.UsuarioId)
                : query.OrderByDescending(x => x.Pagamento.UsuarioId),
            _ => ascending
                ? query.OrderBy(x => x.Pagamento.CriadoEm)
                : query.OrderByDescending(x => x.Pagamento.CriadoEm)
        };
    }

    private static PagamentoOutDto MapPagamento(Pagamento pagamento, PagamentoPagador? pagador, PagamentoCartao? cartao)
    {
        return new PagamentoOutDto(
            pagamento.Id,
            pagamento.ReservaId,
            pagamento.UsuarioId,
            pagamento.Status,
            pagamento.Valor,
            pagamento.IdempotenciaChave,
            pagamento.CriadoEm.ToString("O"),
            pagador == null ? null : new PagamentoPagadorOutDto(
                pagador.CpfCnpj,
                pagador.Nome,
                MapEndereco(pagador.Endereco)),
            cartao == null ? null : new PagamentoCartaoOutDto(
                cartao.Titular,
                cartao.Bandeira,
                cartao.UltimosDigitos,
                cartao.TransacaoId));
    }

    private static EnderecoOutDto? MapEndereco(Endereco? endereco)
    {
        if (endereco is null)
        {
            return null;
        }

        var bairro = endereco.Bairro;
        var cidade = bairro?.Cidade;
        var uf = cidade?.Uf;

        return new EnderecoOutDto(
            endereco.Id,
            endereco.Cep,
            endereco.Logradouro,
            endereco.Numero,
            endereco.Complemento,
            bairro?.Nome,
            cidade?.Nome,
            uf?.Sigla,
            uf?.Nome,
            endereco.Latitude,
            endereco.Longitude);
    }

    private record PagamentoSearchProjection(Pagamento Pagamento, bool PossuiCartao);
}