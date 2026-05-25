using EasyPark.Api.Dtos;
using EasyPark.Api.Exceptions;
using EasyPark.Api.Models;
using EasyPark.Api.Observability;
using EasyPark.Application.Abstractions;

namespace EasyPark.Api.Services;

public class ReservaService
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IUserRepository _userRepository;
    private readonly IVagaRepository _vagaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditEventRepository _auditEventRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public ReservaService(
        IReservaRepository reservaRepository,
        IUserRepository userRepository,
        IVagaRepository vagaRepository,
        IUnitOfWork unitOfWork,
        IAuditEventRepository auditEventRepository,
        ICurrentUserContext currentUserContext)
    {
        _reservaRepository = reservaRepository;
        _userRepository = userRepository;
        _vagaRepository = vagaRepository;
        _unitOfWork = unitOfWork;
        _auditEventRepository = auditEventRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<ReservaOutDto> CreateAsync(ReservaInDto dto, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("ReservaService.Create");

        var actorId = RequireAuthenticatedUserId();
        EnsureOwnership(dto.UsuarioId);
        await EnsureRelacionamentosAsync(dto.UsuarioId, dto.VagaId, cancellationToken);

        var reserva = new Reserva
        {
            UsuarioId = dto.UsuarioId,
            VagaId = dto.VagaId,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "PRE_RESERVA" : dto.Status.Trim().ToUpperInvariant(),
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            Eta = dto.Eta,
            VagaBloqueada = dto.VagaBloqueada,
            ValorPrevisto = dto.ValorPrevisto,
            ValorFinal = dto.ValorFinal
        };

        _reservaRepository.Add(reserva);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("RESERVA_CREATED", reserva.Id, actorId, reserva, cancellationToken);

        return MapToDto(reserva);
    }

    public async Task<ReservaOutDto> FindByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("ReservaService.FindById");

        var reserva = await _reservaRepository.FindByIdAsync(id, cancellationToken: cancellationToken)
            ?? throw new EntityNotFoundException($"Reserva {id} não encontrada");

        EnsureCanAccess(reserva.UsuarioId);
        return MapToDto(reserva);
    }

    public async Task<PagedResultDto<ReservaOutDto>> SearchAsync(
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir,
        long? usuarioId,
        long? vagaId,
        string? status,
        DateTimeOffset? dataInicioDe,
        DateTimeOffset? dataInicioAte,
        CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("ReservaService.Search");

        var actorId = RequireAuthenticatedUserId();
        if (!_currentUserContext.IsAdmin)
        {
            usuarioId = actorId;
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize <= 0 ? 10 : pageSize, 1, 100);
        sortDir = string.IsNullOrWhiteSpace(sortDir) ? "asc" : sortDir.Trim().ToLowerInvariant();

        var result = await _reservaRepository.SearchAsync(page, pageSize, sortBy, sortDir, usuarioId, vagaId, status, dataInicioDe, dataInicioAte, cancellationToken);
        var totalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);

        return new PagedResultDto<ReservaOutDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = result.TotalItems,
            TotalPages = totalPages,
            Items = result.Items.Select(MapToDto).ToList()
        };
    }

    public async Task<ReservaOutDto> UpdateAsync(long id, ReservaInDto dto, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("ReservaService.Update");

        var actorId = RequireAuthenticatedUserId();
        var reserva = await _reservaRepository.FindTrackedByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException($"Reserva {id} não encontrada");

        EnsureCanAccess(reserva.UsuarioId);
        EnsureOwnership(dto.UsuarioId);
        await EnsureRelacionamentosAsync(dto.UsuarioId, dto.VagaId, cancellationToken);

        reserva.UsuarioId = dto.UsuarioId;
        reserva.VagaId = dto.VagaId;
        reserva.Status = string.IsNullOrWhiteSpace(dto.Status) ? reserva.Status : dto.Status.Trim().ToUpperInvariant();
        reserva.DataInicio = dto.DataInicio;
        reserva.DataFim = dto.DataFim;
        reserva.Eta = dto.Eta;
        reserva.VagaBloqueada = dto.VagaBloqueada;
        reserva.ValorPrevisto = dto.ValorPrevisto;
        reserva.ValorFinal = dto.ValorFinal;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("RESERVA_UPDATED", reserva.Id, actorId, reserva, cancellationToken);
        return MapToDto(reserva);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("ReservaService.Delete");

        var actorId = RequireAuthenticatedUserId();
        var reserva = await _reservaRepository.FindTrackedByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException($"Reserva {id} não encontrada");

        EnsureCanAccess(reserva.UsuarioId);
        _reservaRepository.Remove(reserva);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("RESERVA_DELETED", id, actorId, new { Id = id }, cancellationToken);
    }

    private async Task EnsureRelacionamentosAsync(long usuarioId, long vagaId, CancellationToken cancellationToken)
    {
        _ = await _userRepository.FindByIdAsync(usuarioId, cancellationToken: cancellationToken)
            ?? throw new EntityNotFoundException($"Usuário {usuarioId} não encontrado");

        _ = await _vagaRepository.FindByIdAsync(vagaId, cancellationToken: cancellationToken)
            ?? throw new EntityNotFoundException($"Vaga {vagaId} não encontrada");
    }

    private void EnsureCanAccess(long usuarioId)
    {
        if (!_currentUserContext.IsAdmin && _currentUserContext.UserId != usuarioId)
        {
            throw new ForbiddenException("Você não tem acesso a esta reserva.");
        }
    }

    private void EnsureOwnership(long usuarioId)
    {
        if (!_currentUserContext.IsAdmin && _currentUserContext.UserId != usuarioId)
        {
            throw new ForbiddenException("Você não pode operar reservas de outro usuário.");
        }
    }

    private long RequireAuthenticatedUserId()
        => _currentUserContext.UserId ?? throw new UnauthorizedException("Usuário autenticado é obrigatório.");

    private Task WriteAuditAsync(string eventType, long entityId, long actorId, object payload, CancellationToken cancellationToken)
        => _auditEventRepository.WriteAsync(new AuditEventWriteDto(
            eventType,
            nameof(Reserva),
            entityId.ToString(),
            actorId,
            _currentUserContext.CorrelationId,
            payload,
            "ReservaService"), cancellationToken);

    private static ReservaOutDto MapToDto(Reserva reserva)
        => new(
            reserva.Id,
            reserva.UsuarioId,
            reserva.VagaId,
            reserva.Status,
            reserva.DataInicio,
            reserva.DataFim,
            reserva.Eta,
            reserva.VagaBloqueada,
            reserva.ValorPrevisto,
            reserva.ValorFinal);
}
