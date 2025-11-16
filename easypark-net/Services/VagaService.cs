using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyPark.Api.Data;
using EasyPark.Api.Dtos;
using EasyPark.Api.Exceptions;
using EasyPark.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyPark.Api.Services;

public class VagaService
{
    private readonly EasyParkContext _context;

    public VagaService(EasyParkContext context)
    {
        _context = context;
    }
    
    /// Cria uma nova vaga, verifica a existência do nível e do tipo de vaga e garante que não haja outra vaga com o mesmo código no mesmo nível.    
    public async Task<VagaOutDto> CreateAsync(VagaInDto dto)
    {
        // Busca entidades relacionadas
        _ = await _context.Niveis.FindAsync(dto.NivelId) ?? throw new EntityNotFoundException($"Nível {dto.NivelId} não encontrado");
        _ = await _context.TiposVaga.FindAsync(dto.TipoVagaId) ?? throw new EntityNotFoundException($"Tipo de vaga {dto.TipoVagaId} não encontrado");

        // Checa unicidade por nível+codigo
        bool exists = await _context.Vagas.AnyAsync(v => v.NivelId == dto.NivelId && v.Codigo.ToLower() == dto.Codigo.ToLower());
        if (exists)
        {
            throw new BusinessException($"Já existe uma vaga com código {dto.Codigo} neste nível");
        }

        var vaga = new Vaga
        {
            NivelId = dto.NivelId,
            TipoVagaId = dto.TipoVagaId,
            Codigo = dto.Codigo,
            Ativa = dto.Ativa,
            CriadoEm = DateTimeOffset.UtcNow
        };

        _context.Vagas.Add(vaga);
        await _context.SaveChangesAsync();

        return new VagaOutDto(vaga.Id, vaga.Codigo, vaga.Ativa, vaga.NivelId, vaga.TipoVagaId);
    }

    /// Retorna todas as vagas do sistema. Se um status for especificado, filtra as vagas com base no status atual (via join com VagaStatus).
    public async Task<IEnumerable<VagaOutDto>> FindAllAsync(string? status = null)
    {
        IQueryable<Vaga> query = _context.Vagas.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
        {
            string filter = status.ToUpper();
            query = query.Where(v => _context.VagaStatus.Any(s => s.VagaId == v.Id && s.StatusOcupacao!.ToUpper() == filter));
        }
        var list = await query.ToListAsync();
        return list.Select(v => new VagaOutDto(v.Id, v.Codigo, v.Ativa, v.NivelId, v.TipoVagaId));
    }

    public async Task<PagedResultDto<VagaOutDto>> SearchAsync(
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir,
        long? estacionamentoId,
        long? nivelId,
        long? tipoVagaId,
        string? status,
        string? codigo)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize <= 0 ? 10 : pageSize, 1, 100);
        sortDir = string.IsNullOrWhiteSpace(sortDir) ? "asc" : sortDir.Trim().ToLowerInvariant();

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
            var statusFilter = status.Trim().ToUpperInvariant();
            query = query.Where(x => x.Status != null && x.Status.StatusOcupacao != null && x.Status.StatusOcupacao.ToUpper() == statusFilter);
        }

        if (!string.IsNullOrWhiteSpace(codigo))
        {
            var codigoFilter = codigo.Trim().ToUpperInvariant();
            query = query.Where(x => x.Vaga.Codigo.ToUpper().Contains(codigoFilter));
        }

        query = ApplyOrdering(query, sortBy, sortDir);

        var totalItems = await query.LongCountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var vagas = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(x => x.Vaga).ToListAsync();

        return new PagedResultDto<VagaOutDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = vagas.Select(v => new VagaOutDto(v.Id, v.Codigo, v.Ativa, v.NivelId, v.TipoVagaId)).ToList()
        };
    }


    /// Retorna os dados de uma vaga pelo identificador. Lança EntityNotFoundException se a vaga não existir.
    public async Task<VagaOutDto> FindByIdAsync(long id)
    {
        var vaga = await _context.Vagas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new EntityNotFoundException($"Vaga {id} não encontrada");
        return new VagaOutDto(vaga.Id, vaga.Codigo, vaga.Ativa, vaga.NivelId, vaga.TipoVagaId);
    }

    /// Atualiza os dados de uma vaga existente. Garante que as associações existam e que não haja conflito de código dentro do mesmo nível se o código ou o nível forem alterados.
    public async Task<VagaOutDto> UpdateAsync(long id, VagaInDto dto)
    {
        var vaga = await _context.Vagas.FindAsync(id) ?? throw new EntityNotFoundException($"Vaga {id} não encontrada");

        // Verifica existência das entidades relacionadas
        _ = await _context.Niveis.FindAsync(dto.NivelId) ?? throw new EntityNotFoundException($"Nível {dto.NivelId} não encontrado");
        _ = await _context.TiposVaga.FindAsync(dto.TipoVagaId) ?? throw new EntityNotFoundException($"Tipo de vaga {dto.TipoVagaId} não encontrado");

        bool changedCodigo = !string.Equals(vaga.Codigo, dto.Codigo, StringComparison.OrdinalIgnoreCase);
        bool changedNivel = vaga.NivelId != dto.NivelId;
        if (changedCodigo || changedNivel)
        {
            bool exists = await _context.Vagas.AnyAsync(v => v.Id != id && v.NivelId == dto.NivelId && v.Codigo.ToLower() == dto.Codigo.ToLower());
            if (exists)
            {
                throw new BusinessException($"Já existe uma vaga com código {dto.Codigo} neste nível");
            }
        }

        vaga.NivelId = dto.NivelId;
        vaga.TipoVagaId = dto.TipoVagaId;
        vaga.Codigo = dto.Codigo;
        vaga.Ativa = dto.Ativa;
        await _context.SaveChangesAsync();

        return new VagaOutDto(vaga.Id, vaga.Codigo, vaga.Ativa, vaga.NivelId, vaga.TipoVagaId);
    }

    /// Remove uma vaga do sistema. Lança exceção se a vaga não existir. Remoção cascata via DbContext.
    public async Task DeleteAsync(long id)
    {
        var vaga = await _context.Vagas.FindAsync(id) ?? throw new EntityNotFoundException($"Vaga {id} não encontrada");
        _context.Vagas.Remove(vaga);
        await _context.SaveChangesAsync();
    }

   
    /// Retorna o status atual de uma vaga consultando a tabela VagaStatus. Se não houver registro, considera o status desconhecido.
    public async Task<VagaStatusOutDto> GetStatusAsync(long id)
    {
        var status = await _context.VagaStatus.AsNoTracking().FirstOrDefaultAsync(s => s.VagaId == id);
        if (status == null)
        {
            return new VagaStatusOutDto("DESCONHECIDO", null, null);
        }
        return new VagaStatusOutDto(status.StatusOcupacao ?? "DESCONHECIDO", status.UltimoOcorrido, status.SensorId);
    }

    /// Lista todas as vagas pertencentes a um determinado estacionamento, realizando join entre Vaga e Nivel.
    public async Task<IEnumerable<VagaOutDto>> FindByEstacionamentoAsync(long estacionamentoId)
    {
        var query = from v in _context.Vagas.AsNoTracking()
                    join n in _context.Niveis.AsNoTracking() on v.NivelId equals n.Id
                    where n.EstacionamentoId == estacionamentoId
                    select new VagaOutDto(v.Id, v.Codigo, v.Ativa, v.NivelId, v.TipoVagaId);
        return await query.ToListAsync();
    }
    private static IQueryable<VagaSearchProjection> ApplyOrdering(IQueryable<VagaSearchProjection> query, string? sortBy, string sortDir)
    {
        var ascending = sortDir != "desc";
        var key = string.IsNullOrWhiteSpace(sortBy) ? "codigo" : sortBy.Trim().ToLowerInvariant();

        return key switch
        {
            "nivel" => ascending
                ? query.OrderBy(x => x.Nivel.Nome)
                : query.OrderByDescending(x => x.Nivel.Nome),
            "tipo" => ascending
                ? query.OrderBy(x => x.Tipo.Nome)
                : query.OrderByDescending(x => x.Tipo.Nome),
            _ => ascending
                ? query.OrderBy(x => x.Vaga.Codigo)
                : query.OrderByDescending(x => x.Vaga.Codigo)
        };
    }

    private record VagaSearchProjection(Vaga Vaga, Nivel Nivel, TipoVaga Tipo, VagaStatus? Status);
}