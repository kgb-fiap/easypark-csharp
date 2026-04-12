using EasyPark.Api.Dtos;
using EasyPark.Api.Exceptions;
using EasyPark.Api.Models;
using EasyPark.Api.Services;
using EasyPark.UnitTests.TestSupport;

namespace EasyPark.UnitTests.Services;

public class VagaServiceTests
{
    [Fact]
    public async Task CreateAsync_DadosValidos_CriaVaga()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();
        context.Niveis.Add(new Nivel { Id = 1, EstacionamentoId = 1, Nome = "Terreo" });
        context.TiposVaga.Add(new TipoVaga { Id = 1, Nome = "Comum", TarifaPorMinuto = 0.25m });
        await context.SaveChangesAsync();
        var service = new VagaService(context);
        var dto = new VagaInDto(1, 1, "A-01", true);

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.Equal("A-01", result.Codigo);
        Assert.True(result.Ativa);
        Assert.Equal(1, context.Vagas.Count());
    }

    [Fact]
    public async Task CreateAsync_CodigoDuplicado_LancaBusinessException()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();
        context.Niveis.Add(new Nivel { Id = 1, EstacionamentoId = 1, Nome = "Terreo" });
        context.TiposVaga.Add(new TipoVaga { Id = 1, Nome = "Comum", TarifaPorMinuto = 0.25m });
        context.Vagas.Add(new Vaga { Id = 1, NivelId = 1, TipoVagaId = 1, Codigo = "A-01", Ativa = true });
        await context.SaveChangesAsync();
        var service = new VagaService(context);
        var dto = new VagaInDto(1, 1, "a-01", true);

        // Act
        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(dto));

        // Assert
        Assert.Contains("A-01", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetStatusAsync_SemStatus_RetornaDesconhecido()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();
        var service = new VagaService(context);

        // Act
        var result = await service.GetStatusAsync(99);

        // Assert
        Assert.Equal("DESCONHECIDO", result.Status);
        Assert.Null(result.SensorId);
    }
}
