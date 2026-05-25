using EasyPark.Api.Dtos;
using EasyPark.Api.Services;
using EasyPark.Application.Abstractions;
using EasyPark.Infrastructure.Auditing;
using EasyPark.UnitTests.TestSupport;

namespace EasyPark.UnitTests.Services;

public class JobsServiceTests
{
    [Fact]
    public async Task ReservaTimeoutsAsync_RepositoryRetornaQuantidade_AuditaERepasseResultado()
    {
        var audit = new InMemoryAuditEventRepository();
        var service = new JobsService(new FakeJobRepository(), audit, new TestCurrentUserContext { UserId = 7, CorrelationId = "corr-7" });

        var result = await service.ReservaTimeoutsAsync();
        var events = await audit.SearchAsync(1, 10, "JOB_RESERVA_TIMEOUTS", "Job", null, 7);

        Assert.Equal(2, result.Canceladas);
        Assert.Equal(1, events.TotalItems);
    }

    [Fact]
    public async Task PreReservaTimeoutsAsync_RepositoryRetornaQuantidade_AuditaERepasseResultado()
    {
        var audit = new InMemoryAuditEventRepository();
        var service = new JobsService(new FakeJobRepository(), audit, new TestCurrentUserContext { UserId = 7 });

        var result = await service.PreReservaTimeoutsAsync();
        var events = await audit.SearchAsync(1, 10, "JOB_PRERESERVA_TIMEOUTS", "Job", null, 7);

        Assert.Equal(3, result.Canceladas);
        Assert.Equal(1, events.TotalItems);
    }

    [Fact]
    public async Task AtualizarEtaAsync_ParametrosValidos_AuditaERepasseResultado()
    {
        var audit = new InMemoryAuditEventRepository();
        var service = new JobsService(new FakeJobRepository(), audit, new TestCurrentUserContext { UserId = 7 });

        var result = await service.AtualizarEtaAsync(10, 15);
        var events = await audit.SearchAsync(1, 10, "JOB_ETA_UPDATED", "Job", null, 7);

        Assert.Equal("OK", result.Status);
        Assert.Equal("ETA atualizada", result.Msg);
        Assert.Equal(1, events.TotalItems);
    }

    private sealed class FakeJobRepository : IJobRepository
    {
        public Task<JobCountOutDto> ReservaTimeoutsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new JobCountOutDto(2));

        public Task<JobCountOutDto> PreReservaTimeoutsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new JobCountOutDto(3));

        public Task<EtaUpdateOutDto> AtualizarEtaAsync(long id, int minutos, CancellationToken cancellationToken = default)
            => Task.FromResult(new EtaUpdateOutDto("OK", "ETA atualizada"));
    }
}
