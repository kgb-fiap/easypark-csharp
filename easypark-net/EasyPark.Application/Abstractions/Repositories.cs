using EasyPark.Api.Dtos;
using EasyPark.Api.Models;

namespace EasyPark.Application.Abstractions;

public interface IEnderecoRepository
{
    Task<Endereco> UpsertAsync(EnderecoInDto dto, Endereco? endereco = null, CancellationToken cancellationToken = default);
}

public interface IEstacionamentoRepository
{
    void Add(Estacionamento estacionamento);
    void Remove(Estacionamento estacionamento);
    Task<List<Estacionamento>> FindAllWithEnderecoAsync(CancellationToken cancellationToken = default);
    Task<Estacionamento?> FindByIdWithEnderecoAsync(long id, bool asNoTracking = true, CancellationToken cancellationToken = default);
    Task<Estacionamento?> FindTrackedByIdWithEnderecoAsync(long id, CancellationToken cancellationToken = default);
    Task<PagedQueryResult<Estacionamento>> SearchAsync(
        int page,
        int pageSize,
        string? sortBy,
        string sortDir,
        string? nome,
        string? ufSigla,
        string? cidadeNome,
        string? bairroNome,
        CancellationToken cancellationToken = default);
}

public interface IVagaRepository
{
    void Add(Vaga vaga);
    void Remove(Vaga vaga);
    Task<bool> NivelExistsAsync(long nivelId, CancellationToken cancellationToken = default);
    Task<bool> TipoVagaExistsAsync(long tipoVagaId, CancellationToken cancellationToken = default);
    Task<bool> ExistsCodigoNoNivelAsync(long nivelId, string codigo, long? excludingId = null, CancellationToken cancellationToken = default);
    Task<List<Vaga>> FindAllAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<Vaga?> FindByIdAsync(long id, bool asNoTracking = true, CancellationToken cancellationToken = default);
    Task<PagedQueryResult<Vaga>> SearchAsync(
        int page,
        int pageSize,
        string? sortBy,
        string sortDir,
        long? estacionamentoId,
        long? nivelId,
        long? tipoVagaId,
        string? status,
        string? codigo,
        CancellationToken cancellationToken = default);
    Task<VagaStatus?> GetStatusAsync(long vagaId, CancellationToken cancellationToken = default);
    Task<List<Vaga>> FindByEstacionamentoAsync(long estacionamentoId, CancellationToken cancellationToken = default);
}

public interface IUserRepository
{
    void Add(Usuario usuario);
    Task<Usuario?> FindByIdAsync(long id, bool asNoTracking = true, CancellationToken cancellationToken = default);
    Task<Usuario?> FindByEmailAsync(string email, bool asNoTracking = true, CancellationToken cancellationToken = default);
}

public interface IReservaRepository
{
    void Add(Reserva reserva);
    void Remove(Reserva reserva);
    Task<Reserva?> FindByIdAsync(long id, bool asNoTracking = true, CancellationToken cancellationToken = default);
    Task<Reserva?> FindTrackedByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<PagedQueryResult<Reserva>> SearchAsync(
        int page,
        int pageSize,
        string? sortBy,
        string sortDir,
        long? usuarioId,
        long? vagaId,
        string? status,
        DateTimeOffset? dataInicioDe,
        DateTimeOffset? dataInicioAte,
        CancellationToken cancellationToken = default);
}

public interface IPagamentoRepository
{
    void Add(Pagamento pagamento);
    void AddPagador(PagamentoPagador pagador);
    void AddCartao(PagamentoCartao cartao);
    Task<Pagamento?> FindByIdAsync(long id, bool asNoTracking = true, CancellationToken cancellationToken = default);
    Task<PagedQueryResult<Pagamento>> SearchAsync(
        int page,
        int pageSize,
        string? sortBy,
        string sortDir,
        long? reservaId,
        long? usuarioId,
        string? status,
        string? metodo,
        CancellationToken cancellationToken = default);
    Task<PagamentoPagador?> FindPagadorByPagamentoIdAsync(long pagamentoId, CancellationToken cancellationToken = default);
    Task<PagamentoCartao?> FindCartaoByPagamentoIdAsync(long pagamentoId, CancellationToken cancellationToken = default);
    Task<Dictionary<long, PagamentoPagador>> FindPagadoresByPagamentoIdsAsync(IEnumerable<long> pagamentoIds, CancellationToken cancellationToken = default);
    Task<Dictionary<long, PagamentoCartao>> FindCartoesByPagamentoIdsAsync(IEnumerable<long> pagamentoIds, CancellationToken cancellationToken = default);
}

public interface IJobRepository
{
    Task<JobCountOutDto> ReservaTimeoutsAsync(CancellationToken cancellationToken = default);
    Task<JobCountOutDto> PreReservaTimeoutsAsync(CancellationToken cancellationToken = default);
    Task<EtaUpdateOutDto> AtualizarEtaAsync(long id, int minutos, CancellationToken cancellationToken = default);
}

public interface IAuditEventRepository
{
    Task WriteAsync(AuditEventWriteDto auditEvent, CancellationToken cancellationToken = default);
    Task<AuditEventOutDto?> FindByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<PagedQueryResult<AuditEventOutDto>> SearchAsync(
        int page,
        int pageSize,
        string? eventType,
        string? entityType,
        string? entityId,
        long? userId,
        CancellationToken cancellationToken = default);
}
