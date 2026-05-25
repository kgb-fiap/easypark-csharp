using EasyPark.Api.Data;
using EasyPark.Api.Dtos;
using EasyPark.Api.Models;
using EasyPark.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EasyPark.Infrastructure.Repositories;

public class EnderecoRepository : IEnderecoRepository
{
    private readonly EasyParkContext _context;

    public EnderecoRepository(EasyParkContext context)
    {
        _context = context;
    }

    public async Task<Endereco> UpsertAsync(EnderecoInDto dto, Endereco? endereco = null, CancellationToken cancellationToken = default)
    {
        var ufSigla = dto.Uf.Trim().ToUpperInvariant();
        var ufNome = string.IsNullOrWhiteSpace(dto.UfNome) ? ufSigla : dto.UfNome.Trim();

        var uf = await _context.Ufs.FindAsync([ufSigla], cancellationToken);
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
        var cidade = await _context.Cidades.FirstOrDefaultAsync(c => c.Nome == cidadeNome && c.UfSigla == ufSigla, cancellationToken);
        if (cidade is null)
        {
            cidade = new Cidade { Nome = cidadeNome, UfSigla = ufSigla, Uf = uf };
            _context.Cidades.Add(cidade);
        }

        Bairro? bairro = null;
        if (cidade.Id > 0)
        {
            bairro = await _context.Bairros.FirstOrDefaultAsync(b => b.Nome == dto.Bairro && b.CidadeId == cidade.Id, cancellationToken);
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
        endereco.Bairro = bairro;
        endereco.Latitude = dto.Latitude;
        endereco.Longitude = dto.Longitude;

        return endereco;
    }
}
