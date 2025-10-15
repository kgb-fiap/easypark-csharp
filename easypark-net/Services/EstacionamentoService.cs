using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyPark.Api.Data;
using EasyPark.Api.Dtos;
using EasyPark.Api.Exceptions;
using EasyPark.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyPark.Api.Services;

public class EstacionamentoService
{
    private readonly EasyParkContext _context;
    public EstacionamentoService(EasyParkContext context)
    {
        _context = context;
    }
    
    public async Task<EstacionamentoOutDto> CreateAsync(EstacionamentoInDto dto)
    {
        var est = new Estacionamento
        {
            OperadoraId = dto.OperadoraId,
            Nome = dto.Nome,
            Endereco = dto.Endereco,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            CriadoEm = System.DateTimeOffset.UtcNow
        };
        _context.Estacionamentos.Add(est);
        await _context.SaveChangesAsync();
        return new EstacionamentoOutDto(est.Id, est.Nome, est.Endereco);
    }

    public async Task<IEnumerable<EstacionamentoOutDto>> FindAllAsync()
    {
        var list = await _context.Estacionamentos.AsNoTracking().ToListAsync();
        return list.Select(e => new EstacionamentoOutDto(e.Id, e.Nome, e.Endereco));
    }
    
    public async Task<EstacionamentoOutDto> FindByIdAsync(long id)
    {
        var est = await _context.Estacionamentos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new EntityNotFoundException($"Estacionamento {id} não encontrado");
        return new EstacionamentoOutDto(est.Id, est.Nome, est.Endereco);
    }
    
    public async Task<EstacionamentoOutDto> UpdateAsync(long id, EstacionamentoInDto dto)
    {
        var est = await _context.Estacionamentos.FindAsync(id)
            ?? throw new EntityNotFoundException($"Estacionamento {id} não encontrado");
        est.OperadoraId = dto.OperadoraId;
        est.Nome = dto.Nome;
        est.Endereco = dto.Endereco;
        est.Latitude = dto.Latitude;
        est.Longitude = dto.Longitude;
        await _context.SaveChangesAsync();
        return new EstacionamentoOutDto(est.Id, est.Nome, est.Endereco);
    }

    public async Task DeleteAsync(long id)
    {
        var est = await _context.Estacionamentos.FindAsync(id)
            ?? throw new EntityNotFoundException($"Estacionamento {id} não encontrado");
        _context.Estacionamentos.Remove(est);
        await _context.SaveChangesAsync();
    }
}