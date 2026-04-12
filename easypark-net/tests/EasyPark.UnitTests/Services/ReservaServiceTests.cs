using EasyPark.Api.Dtos;
using EasyPark.Api.Exceptions;
using EasyPark.Api.Models;
using EasyPark.Api.Services;
using EasyPark.UnitTests.TestSupport;

namespace EasyPark.UnitTests.Services;

public class ReservaServiceTests
{
    [Fact]
    public async Task CreateAsync_StatusVazio_CriaPreReserva()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();
        context.Usuarios.Add(new Usuario { Id = 1, Nome = "Gabriel", Email = "gabriel@email.com" });
        context.Vagas.Add(new Vaga { Id = 1, NivelId = 1, TipoVagaId = 1, Codigo = "A-01", Ativa = true });
        await context.SaveChangesAsync();
        var service = new ReservaService(context);
        var dto = new ReservaInDto(1, 1, null, DateTimeOffset.UtcNow, null, null, false, 10m, null);

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.Equal("PRE_RESERVA", result.Status);
        Assert.Equal(1, result.UsuarioId);
        Assert.Equal(1, result.VagaId);
    }

    [Fact]
    public async Task CreateAsync_UsuarioInexistente_LancaEntityNotFoundException()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();
        context.Vagas.Add(new Vaga { Id = 1, NivelId = 1, TipoVagaId = 1, Codigo = "A-01", Ativa = true });
        await context.SaveChangesAsync();
        var service = new ReservaService(context);
        var dto = new ReservaInDto(99, 1, "PRE_RESERVA", null, null, null, false, null, null);

        // Act
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => service.CreateAsync(dto));

        // Assert
        Assert.Contains("Usuario 99", RemoveDiacritics(exception.Message), StringComparison.OrdinalIgnoreCase);
    }

    private static string RemoveDiacritics(string text)
    {
        return text
            .Replace("Usuário", "Usuario", StringComparison.OrdinalIgnoreCase)
            .Replace("não", "nao", StringComparison.OrdinalIgnoreCase);
    }
}
