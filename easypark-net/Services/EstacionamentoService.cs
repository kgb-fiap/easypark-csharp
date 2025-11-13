using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        var enderecoDto = dto.Endereco ?? throw new ValidationException("Endereço é obrigatório");
        var endereco = await UpsertEnderecoAsync(enderecoDto);

        var est = new Estacionamento
        {
            OperadoraId = dto.OperadoraId,
            Nome = dto.Nome,
            Endereco = endereco,
            CriadoEm = System.DateTimeOffset.UtcNow
        };
        _context.Estacionamentos.Add(est);
        await _context.SaveChangesAsync();
        await _context.Entry(est).Reference(e => e.Endereco).LoadAsync();
        await _context.Entry(est.Endereco).Reference(e => e.Bairro).LoadAsync();
        if (est.Endereco.Bairro is not null)
        {
            await _context.Entry(est.Endereco.Bairro).Reference(b => b.Cidade).LoadAsync();
            if (est.Endereco.Bairro.Cidade is not null)
            {
                await _context.Entry(est.Endereco.Bairro.Cidade).Reference(c => c.Uf).LoadAsync();
            }
        }
        return MapToDto(est);
    }

    public async Task<IEnumerable<EstacionamentoOutDto>> FindAllAsync()
    {
        var list = await WithEndereco(_context.Estacionamentos.AsNoTracking()).ToListAsync();
        return list.Select(MapToDto);
    }

    public async Task<EstacionamentoOutDto> FindByIdAsync(long id)
    {
        var est = await WithEndereco(_context.Estacionamentos.AsNoTracking())
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new EntityNotFoundException($"Estacionamento {id} não encontrado");
        return MapToDto(est);
    }

    public async Task<EstacionamentoOutDto> UpdateAsync(long id, EstacionamentoInDto dto)
    {
        var est = await WithEndereco(_context.Estacionamentos)
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new EntityNotFoundException($"Estacionamento {id} não encontrado");

        var enderecoDto = dto.Endereco ?? throw new ValidationException("Endereço é obrigatório");
        await UpsertEnderecoAsync(enderecoDto, est.Endereco);

        est.OperadoraId = dto.OperadoraId;
        est.Nome = dto.Nome;
        await _context.SaveChangesAsync();
        return MapToDto(est);
    }

    public async Task DeleteAsync(long id)
    {
        var est = await _context.Estacionamentos.FindAsync(id)
            ?? throw new EntityNotFoundException($"Estacionamento {id} não encontrado");
        _context.Estacionamentos.Remove(est);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Estacionamento> WithEndereco(IQueryable<Estacionamento> query)
    {
        return query
            .Include(e => e.Endereco)
                .ThenInclude(e => e!.Bairro)
                    .ThenInclude(b => b!.Cidade)
                        .ThenInclude(c => c!.Uf);
    }

    private async Task<Endereco> UpsertEnderecoAsync(EnderecoInDto dto, Endereco? endereco = null)
    {
        var ufSigla = dto.Uf.Trim().ToUpperInvariant();
        var ufNome = string.IsNullOrWhiteSpace(dto.UfNome) ? ufSigla : dto.UfNome.Trim();

        var uf = await _context.Ufs.FindAsync(ufSigla);
        if (uf is null)
        {
            uf = new Uf { Sigla = ufSigla, Nome = ufNome };
            _context.Ufs.Add(uf);
        }
        else if (!string.IsNullOrWhiteSpace(dto.UfNome) && !string.Equals(uf.Nome, ufNome, System.StringComparison.OrdinalIgnoreCase))
        {
            uf.Nome = ufNome;
        }

        var cidadeNome = dto.Cidade.Trim();
        var cidade = await _context.Cidades
            .FirstOrDefaultAsync(c => c.Nome == cidadeNome && c.UfSigla == ufSigla);
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

        if (endereco is null)
        {
            endereco = new Endereco();
            _context.Enderecos.Add(endereco);
        }

        endereco.Cep = dto.Cep;
        endereco.Logradouro = dto.Logradouro.Trim();
        endereco.Numero = dto.Numero;
        endereco.Complemento = dto.Complemento;
        endereco.Latitude = dto.Latitude;
        endereco.Longitude = dto.Longitude;
        endereco.Bairro = bairro;

        return endereco;
    }

    private static EstacionamentoOutDto MapToDto(Estacionamento est)
    {
        return new EstacionamentoOutDto(est.Id, est.Nome, MapEndereco(est.Endereco));
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
}