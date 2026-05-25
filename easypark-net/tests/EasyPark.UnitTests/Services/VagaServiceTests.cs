using EasyPark.Api.Dtos;
using EasyPark.Api.Exceptions;
using EasyPark.Api.Models;
using EasyPark.Api.Services;
using EasyPark.Application.Abstractions;
using EasyPark.Infrastructure.Auditing;
using EasyPark.Infrastructure.Persistence;
using EasyPark.Infrastructure.Repositories;
using EasyPark.UnitTests.TestSupport;

namespace EasyPark.UnitTests.Services;

public class VagaServiceTests
{
    [Fact]
    public async Task CreateAsync_DadosValidos_CriaVaga()
    {
        await using var context = TestDbContextFactory.Create();
        context.Niveis.Add(new Nivel { Id = 1, EstacionamentoId = 1, Nome = "Terreo" });
        context.TiposVaga.Add(new TipoVaga { Id = 1, Nome = "Comum", TarifaPorMinuto = 0.25m });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new VagaInDto(1, 1, "A-01", true);

        var result = await service.CreateAsync(dto);

        Assert.Equal("A-01", result.Codigo);
        Assert.True(result.Ativa);
        Assert.Equal(1, context.Vagas.Count());
    }

    [Fact]
    public async Task CreateAsync_CodigoDuplicado_LancaBusinessException()
    {
        await using var context = TestDbContextFactory.Create();
        context.Niveis.Add(new Nivel { Id = 1, EstacionamentoId = 1, Nome = "Terreo" });
        context.TiposVaga.Add(new TipoVaga { Id = 1, Nome = "Comum", TarifaPorMinuto = 0.25m });
        context.Vagas.Add(new Vaga { Id = 1, NivelId = 1, TipoVagaId = 1, Codigo = "A-01", Ativa = true });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(new VagaInDto(1, 1, "a-01", true)));

        Assert.Contains("A-01", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetStatusAsync_SemStatus_RetornaDesconhecido()
    {
        await using var context = TestDbContextFactory.Create();
        var service = CreateService(context);

        var result = await service.GetStatusAsync(99);

        Assert.Equal("DESCONHECIDO", result.Status);
        Assert.Null(result.SensorId);
    }

    [Fact]
    public async Task SearchAsync_ComFiltros_RetornaPaginado()
    {
        var service = new VagaService(new FakeSearchVagaRepository(), new FakeUnitOfWork(), new InMemoryAuditEventRepository());

        var result = await service.SearchAsync(1, 10, "codigo", "asc", 1, 1, 1, "LIVRE", "A");

        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal("A-01", result.Items.Single().Codigo);
    }

    [Fact]
    public async Task FindByIdAsync_IdExistente_RetornaVaga()
    {
        await using var context = TestDbContextFactory.Create();
        SeedVagaGraph(context);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.FindByIdAsync(1);

        Assert.Equal("A-01", result.Codigo);
    }

    [Fact]
    public async Task UpdateAsync_DadosValidos_AtualizaVaga()
    {
        await using var context = TestDbContextFactory.Create();
        SeedVagaGraph(context);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.UpdateAsync(1, new VagaInDto(1, 1, "A-02", false));

        Assert.Equal("A-02", result.Codigo);
        Assert.False(result.Ativa);
    }

    [Fact]
    public async Task DeleteAsync_IdExistente_RemoveVaga()
    {
        await using var context = TestDbContextFactory.Create();
        SeedVagaGraph(context);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await service.DeleteAsync(1);

        Assert.Empty(context.Vagas);
    }

    [Fact]
    public async Task FindByEstacionamentoAsync_IdExistente_RetornaVagas()
    {
        await using var context = TestDbContextFactory.Create();
        SeedVagaGraph(context);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.FindByEstacionamentoAsync(1);

        Assert.Single(result);
        Assert.Equal("A-01", result.Single().Codigo);
    }

    private static VagaService CreateService(EasyPark.Api.Data.EasyParkContext context)
        => new(new VagaRepository(context), new EfUnitOfWork(context), new InMemoryAuditEventRepository());

    private static void SeedVagaGraph(EasyPark.Api.Data.EasyParkContext context)
    {
        context.Niveis.Add(new Nivel { Id = 1, EstacionamentoId = 1, Nome = "Terreo" });
        context.TiposVaga.Add(new TipoVaga { Id = 1, Nome = "Comum", TarifaPorMinuto = 0.25m });
        context.Vagas.Add(new Vaga { Id = 1, NivelId = 1, TipoVagaId = 1, Codigo = "A-01", Ativa = true });
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private sealed class FakeSearchVagaRepository : IVagaRepository
    {
        public void Add(Vaga vaga) => throw new NotSupportedException();
        public void Remove(Vaga vaga) => throw new NotSupportedException();
        public Task<bool> NivelExistsAsync(long nivelId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> TipoVagaExistsAsync(long tipoVagaId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> ExistsCodigoNoNivelAsync(long nivelId, string codigo, long? excludingId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<List<Vaga>> FindAllAsync(string? status = null, CancellationToken cancellationToken = default) => Task.FromResult(new List<Vaga>());
        public Task<Vaga?> FindByIdAsync(long id, bool asNoTracking = true, CancellationToken cancellationToken = default) => Task.FromResult<Vaga?>(null);
        public Task<VagaStatus?> GetStatusAsync(long vagaId, CancellationToken cancellationToken = default) => Task.FromResult<VagaStatus?>(null);
        public Task<List<Vaga>> FindByEstacionamentoAsync(long estacionamentoId, CancellationToken cancellationToken = default) => Task.FromResult(new List<Vaga>());

        public Task<PagedQueryResult<Vaga>> SearchAsync(
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
            => Task.FromResult(new PagedQueryResult<Vaga>(
                new List<Vaga> { new() { Id = 1, NivelId = 1, TipoVagaId = 1, Codigo = "A-01", Ativa = true } },
                1));
    }
}
