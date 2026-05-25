using EasyPark.Api.Dtos;
using EasyPark.Api.Exceptions;
using EasyPark.Api.Models;
using EasyPark.Api.Observability;
using EasyPark.Application.Abstractions;

namespace EasyPark.Api.Services;

public class VagaService
{
    private readonly IVagaRepository _vagaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditEventRepository _auditEventRepository;

    public VagaService(IVagaRepository vagaRepository, IUnitOfWork unitOfWork, IAuditEventRepository auditEventRepository)
    {
        _vagaRepository = vagaRepository;
        _unitOfWork = unitOfWork;
        _auditEventRepository = auditEventRepository;
    }

    public async Task<VagaOutDto> CreateAsync(VagaInDto dto, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("VagaService.Create");
        activity?.SetTag("vaga.codigo", dto.Codigo);

        if (!await _vagaRepository.NivelExistsAsync(dto.NivelId, cancellationToken))
        {
            throw new EntityNotFoundException($"Nível {dto.NivelId} não encontrado");
        }

        if (!await _vagaRepository.TipoVagaExistsAsync(dto.TipoVagaId, cancellationToken))
        {
            throw new EntityNotFoundException($"Tipo de vaga {dto.TipoVagaId} não encontrado");
        }

        if (await _vagaRepository.ExistsCodigoNoNivelAsync(dto.NivelId, dto.Codigo, cancellationToken: cancellationToken))
        {
            throw new BusinessException($"Já existe uma vaga com código {dto.Codigo} neste nível");
        }

        var vaga = new Vaga
        {
            NivelId = dto.NivelId,
            TipoVagaId = dto.TipoVagaId,
            Codigo = dto.Codigo,
            Ativa = dto.Ativa,
            CriadoEm = DateTimeOffset.UtcNow
        };

        _vagaRepository.Add(vaga);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("VAGA_CREATED", vaga.Id, new { vaga.Id, vaga.Codigo, vaga.NivelId, vaga.TipoVagaId }, cancellationToken);

        return MapToDto(vaga);
    }

    public async Task<IEnumerable<VagaOutDto>> FindAllAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("VagaService.FindAll");
        var items = await _vagaRepository.FindAllAsync(status, cancellationToken);
        return items.Select(MapToDto);
    }

    public async Task<PagedResultDto<VagaOutDto>> SearchAsync(
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir,
        long? estacionamentoId,
        long? nivelId,
        long? tipoVagaId,
        string? status,
        string? codigo,
        CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("VagaService.Search");
        activity?.SetTag("page", page);
        activity?.SetTag("page.size", pageSize);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize <= 0 ? 10 : pageSize, 1, 100);
        sortDir = string.IsNullOrWhiteSpace(sortDir) ? "asc" : sortDir.Trim().ToLowerInvariant();

        var result = await _vagaRepository.SearchAsync(page, pageSize, sortBy, sortDir, estacionamentoId, nivelId, tipoVagaId, status, codigo, cancellationToken);
        var totalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);

        return new PagedResultDto<VagaOutDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = result.TotalItems,
            TotalPages = totalPages,
            Items = result.Items.Select(MapToDto).ToList()
        };
    }

    public async Task<VagaOutDto> FindByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("VagaService.FindById");
        var vaga = await _vagaRepository.FindByIdAsync(id, cancellationToken: cancellationToken)
            ?? throw new EntityNotFoundException($"Vaga {id} não encontrada");
        return MapToDto(vaga);
    }

    public async Task<VagaOutDto> UpdateAsync(long id, VagaInDto dto, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("VagaService.Update");

        var vaga = await _vagaRepository.FindByIdAsync(id, asNoTracking: false, cancellationToken: cancellationToken)
            ?? throw new EntityNotFoundException($"Vaga {id} não encontrada");

        if (!await _vagaRepository.NivelExistsAsync(dto.NivelId, cancellationToken))
        {
            throw new EntityNotFoundException($"Nível {dto.NivelId} não encontrado");
        }

        if (!await _vagaRepository.TipoVagaExistsAsync(dto.TipoVagaId, cancellationToken))
        {
            throw new EntityNotFoundException($"Tipo de vaga {dto.TipoVagaId} não encontrado");
        }

        var changedCodigo = !string.Equals(vaga.Codigo, dto.Codigo, StringComparison.OrdinalIgnoreCase);
        var changedNivel = vaga.NivelId != dto.NivelId;

        if ((changedCodigo || changedNivel) &&
            await _vagaRepository.ExistsCodigoNoNivelAsync(dto.NivelId, dto.Codigo, id, cancellationToken))
        {
            throw new BusinessException($"Já existe uma vaga com código {dto.Codigo} neste nível");
        }

        vaga.NivelId = dto.NivelId;
        vaga.TipoVagaId = dto.TipoVagaId;
        vaga.Codigo = dto.Codigo;
        vaga.Ativa = dto.Ativa;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("VAGA_UPDATED", vaga.Id, new { vaga.Id, vaga.Codigo, vaga.NivelId, vaga.TipoVagaId }, cancellationToken);

        return MapToDto(vaga);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("VagaService.Delete");

        var vaga = await _vagaRepository.FindByIdAsync(id, asNoTracking: false, cancellationToken: cancellationToken)
            ?? throw new EntityNotFoundException($"Vaga {id} não encontrada");

        _vagaRepository.Remove(vaga);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("VAGA_DELETED", id, new { Id = id }, cancellationToken);
    }

    public async Task<VagaStatusOutDto> GetStatusAsync(long id, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("VagaService.GetStatus");
        var status = await _vagaRepository.GetStatusAsync(id, cancellationToken);
        return status is null
            ? new VagaStatusOutDto("DESCONHECIDO", null, null)
            : new VagaStatusOutDto(status.StatusOcupacao ?? "DESCONHECIDO", status.UltimoOcorrido, status.SensorId);
    }

    public async Task<IEnumerable<VagaOutDto>> FindByEstacionamentoAsync(long estacionamentoId, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("VagaService.FindByEstacionamento");
        var vagas = await _vagaRepository.FindByEstacionamentoAsync(estacionamentoId, cancellationToken);
        return vagas.Select(MapToDto);
    }

    private Task WriteAuditAsync(string eventType, long entityId, object payload, CancellationToken cancellationToken)
        => _auditEventRepository.WriteAsync(new AuditEventWriteDto(
            eventType,
            nameof(Vaga),
            entityId.ToString(),
            null,
            null,
            payload,
            "VagaService"), cancellationToken);

    private static VagaOutDto MapToDto(Vaga vaga) => new(vaga.Id, vaga.Codigo, vaga.Ativa, vaga.NivelId, vaga.TipoVagaId);
}
