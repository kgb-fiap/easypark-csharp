using EasyPark.Api.Dtos;
using EasyPark.Api.Exceptions;
using EasyPark.Api.Models;
using EasyPark.Api.Services;
using EasyPark.Infrastructure.Auditing;
using EasyPark.Infrastructure.Persistence;
using EasyPark.Infrastructure.Repositories;
using EasyPark.UnitTests.TestSupport;

namespace EasyPark.UnitTests.Services;

public class ReservaServiceTests
{
    [Fact]
    public async Task CreateAsync_StatusVazio_CriaPreReserva()
    {
        await using var context = TestDbContextFactory.Create();
        context.Usuarios.Add(new Usuario { Id = 1, Nome = "Gabriel", Email = "gabriel@email.com", PasswordHash = "hash", Role = "Cliente" });
        context.Vagas.Add(new Vaga { Id = 1, NivelId = 1, TipoVagaId = 1, Codigo = "A-01", Ativa = true });
        await context.SaveChangesAsync();

        var service = CreateService(context, new TestCurrentUserContext { UserId = 1, Role = "Cliente" });
        var dto = new ReservaInDto(1, 1, null, DateTimeOffset.UtcNow, null, null, false, 10m, null);

        var result = await service.CreateAsync(dto);

        Assert.Equal("PRE_RESERVA", result.Status);
        Assert.Equal(1, result.UsuarioId);
        Assert.Equal(1, result.VagaId);
    }

    [Fact]
    public async Task CreateAsync_UsuarioInexistente_LancaEntityNotFoundException()
    {
        await using var context = TestDbContextFactory.Create();
        context.Vagas.Add(new Vaga { Id = 1, NivelId = 1, TipoVagaId = 1, Codigo = "A-01", Ativa = true });
        await context.SaveChangesAsync();

        var service = CreateService(context, new TestCurrentUserContext { UserId = 99, Role = "Cliente" });

        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => service.CreateAsync(new ReservaInDto(99, 1, "PRE_RESERVA", null, null, null, false, null, null)));

        Assert.Contains("99", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static ReservaService CreateService(EasyPark.Api.Data.EasyParkContext context, TestCurrentUserContext currentUser)
        => new(
            new ReservaRepository(context),
            new UsuarioRepository(context),
            new VagaRepository(context),
            new EfUnitOfWork(context),
            new InMemoryAuditEventRepository(),
            currentUser);
}
