using System;
using System.Linq;
using System.Threading.Tasks;
using EasyPark.Api.Data;
using EasyPark.Api.Dtos;
using EasyPark.Api.Exceptions;
using EasyPark.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyPark.Api.Services;

/// Serviço responsável pelas regras de negócio de reservas.
public class ReservaService
{
    private readonly EasyParkContext _context;

    public ReservaService(EasyParkContext context)
    {
        _context = context;
    }

    public async Task<ReservaOutDto> CreateAsync(ReservaInDto dto)
    {
        await EnsureRelacionamentosAsync(dto.UsuarioId, dto.VagaId);

        var reserva = new Reserva
        {
            UsuarioId = dto.UsuarioId,
            VagaId = dto.VagaId,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "PRE_RESERVA" : dto.Status.Trim().ToUpperInvariant(),
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            Eta = dto.Eta,
            VagaBloqueada = dto.VagaBloqueada,
            ValorPrevisto = dto.ValorPrevisto,
            ValorFinal = dto.ValorFinal
        };

        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
        return MapToDto(reserva);
    }

    public async Task<ReservaOutDto> FindByIdAsync(long id)
    {
        var reserva = await _context.Reservas.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new EntityNotFoundException($"Reserva {id} não encontrada");
        return MapToDto(reserva);
    }

    public async Task<PagedResultDto<ReservaOutDto>> SearchAsync(
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir,
        long? usuarioId,
        long? vagaId,
        string? status,
        DateTimeOffset? dataInicioDe,
        DateTimeOffset? dataInicioAte)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize <= 0 ? 10 : pageSize, 1, 100);
        sortDir = string.IsNullOrWhiteSpace(sortDir) ? "asc" : sortDir.Trim().ToLowerInvariant();

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
            var statusFilter = status.Trim().ToUpperInvariant();
            query = query.Where(r => r.Status != null && r.Status.ToUpper() == statusFilter);
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

        var totalItems = await query.LongCountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResultDto<ReservaOutDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = items.Select(MapToDto).ToList()
        };
    }

    public async Task<ReservaOutDto> UpdateAsync(long id, ReservaInDto dto)
    {
        var reserva = await _context.Reservas.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new EntityNotFoundException($"Reserva {id} não encontrada");

        await EnsureRelacionamentosAsync(dto.UsuarioId, dto.VagaId);

        reserva.UsuarioId = dto.UsuarioId;
        reserva.VagaId = dto.VagaId;
        reserva.Status = string.IsNullOrWhiteSpace(dto.Status) ? reserva.Status : dto.Status.Trim().ToUpperInvariant();
        reserva.DataInicio = dto.DataInicio;
        reserva.DataFim = dto.DataFim;
        reserva.Eta = dto.Eta;
        reserva.VagaBloqueada = dto.VagaBloqueada;
        reserva.ValorPrevisto = dto.ValorPrevisto;
        reserva.ValorFinal = dto.ValorFinal;

        await _context.SaveChangesAsync();
        return MapToDto(reserva);
    }

    public async Task DeleteAsync(long id)
    {
        var reserva = await _context.Reservas.FindAsync(id)
            ?? throw new EntityNotFoundException($"Reserva {id} não encontrada");
        _context.Reservas.Remove(reserva);
        await _context.SaveChangesAsync();
    }

    private async Task EnsureRelacionamentosAsync(long usuarioId, long vagaId)
    {
        _ = await _context.Usuarios.FindAsync(usuarioId) ?? throw new EntityNotFoundException($"Usuário {usuarioId} não encontrado");
        _ = await _context.Vagas.FindAsync(vagaId) ?? throw new EntityNotFoundException($"Vaga {vagaId} não encontrada");
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

    private static ReservaOutDto MapToDto(Reserva reserva)
    {
        return new ReservaOutDto(
            reserva.Id,
            reserva.UsuarioId,
            reserva.VagaId,
            reserva.Status,
            reserva.DataInicio,
            reserva.DataFim,
            reserva.Eta,
            reserva.VagaBloqueada,
            reserva.ValorPrevisto,
            reserva.ValorFinal);
    }
}