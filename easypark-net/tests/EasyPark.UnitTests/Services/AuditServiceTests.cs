using EasyPark.Api.Dtos;
using EasyPark.Api.Services;
using EasyPark.Infrastructure.Auditing;

namespace EasyPark.UnitTests.Services;

public class AuditServiceTests
{
    [Fact]
    public async Task SearchAsync_ComEventoGravado_RetornaEventoFiltrado()
    {
        var repository = new InMemoryAuditEventRepository();
        var service = new AuditService(repository);
        await repository.WriteAsync(new AuditEventWriteDto(
            "RESERVA_CREATED",
            "Reserva",
            "10",
            1,
            "corr-1",
            new { ReservaId = 10 },
            "Test"));

        var result = await service.SearchAsync(1, 10, "RESERVA_CREATED", "Reserva", "10", 1);

        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal("RESERVA_CREATED", result.Items[0].EventType);
    }

    [Fact]
    public async Task FindByIdAsync_IdExistente_RetornaEvento()
    {
        var repository = new InMemoryAuditEventRepository();
        var service = new AuditService(repository);
        await repository.WriteAsync(new AuditEventWriteDto("USER_LOGGED_IN", "Usuario", "1", 1, null, new { Id = 1 }, "Test"));
        var search = await service.SearchAsync(1, 10, null, null, null, null);
        var id = search.Items[0].Id;

        var result = await service.FindByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal("USER_LOGGED_IN", result!.EventType);
    }
}
