using EasyPark.Api.Dtos;
using EasyPark.Api.Exceptions;
using EasyPark.Api.Models;
using EasyPark.Api.Services;
using EasyPark.Infrastructure.Auditing;
using EasyPark.Infrastructure.Persistence;
using EasyPark.Infrastructure.Repositories;
using EasyPark.UnitTests.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace EasyPark.UnitTests.Services;

public class EstacionamentoServiceTests
{
    [Fact]
    public async Task CreateAsync_DadosValidos_CriaEstacionamentoComEndereco()
    {
        await using var context = TestDbContextFactory.Create();
        var service = CreateService(context);
        var dto = CreateDto("EasyPark Central");

        var result = await service.CreateAsync(dto);

        Assert.Equal("EasyPark Central", result.Nome);
        Assert.NotNull(result.Endereco);
        Assert.Equal("SP", result.Endereco!.Uf);
        Assert.Equal(1, await context.Estacionamentos.CountAsync());
    }

    [Fact]
    public async Task SearchAsync_ComFiltroNome_RetornaResultadoPaginado()
    {
        await using var context = TestDbContextFactory.Create();
        var service = CreateService(context);
        await service.CreateAsync(CreateDto("EasyPark Central"));
        await service.CreateAsync(CreateDto("Garagem Norte"));

        var result = await service.SearchAsync(1, 1, "nome", "desc", "easy", null, null, null);

        Assert.Equal(1, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal("EasyPark Central", result.Items.Single().Nome);
    }

    [Fact]
    public async Task UpdateAsync_IdExistente_AtualizaNomeEEndereco()
    {
        await using var context = TestDbContextFactory.Create();
        var service = CreateService(context);
        var created = await service.CreateAsync(CreateDto("EasyPark Central"));
        var updatedDto = CreateDto("EasyPark Paulista") with
        {
            Endereco = new EnderecoInDto("01310-100", "Av Paulista", "1578", "CJ 1", "Bela Vista", "Sao Paulo", "SP", "Sao Paulo", -23.561684m, -46.655981m)
        };

        var result = await service.UpdateAsync(created.Id, updatedDto);

        Assert.Equal("EasyPark Paulista", result.Nome);
        Assert.Equal("01310-100", result.Endereco!.Cep);
        Assert.Equal("CJ 1", result.Endereco.Complemento);
    }

    [Fact]
    public async Task DeleteAsync_IdExistente_RemoveEstacionamento()
    {
        await using var context = TestDbContextFactory.Create();
        var service = CreateService(context);
        var created = await service.CreateAsync(CreateDto("EasyPark Central"));

        await service.DeleteAsync(created.Id);

        Assert.Empty(context.Estacionamentos);
    }

    [Fact]
    public async Task FindByIdAsync_IdInexistente_LancaEntityNotFoundException()
    {
        await using var context = TestDbContextFactory.Create();
        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => service.FindByIdAsync(999));

        Assert.Contains("999", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static EstacionamentoInDto CreateDto(string nome)
        => new(
            1,
            nome,
            new EnderecoInDto("01000-000", "Av Paulista", "1000", null, "Bela Vista", "Sao Paulo", "SP", "Sao Paulo", -23.55m, -46.63m));

    private static EstacionamentoService CreateService(EasyPark.Api.Data.EasyParkContext context)
        => new(
            new EstacionamentoRepository(context),
            new EnderecoRepository(context),
            new EfUnitOfWork(context),
            new InMemoryAuditEventRepository());
}
