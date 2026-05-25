using System.ComponentModel.DataAnnotations;
using EasyPark.Api.Dtos;
using EasyPark.Api.Exceptions;
using EasyPark.Api.Models;
using EasyPark.Api.Observability;
using EasyPark.Application.Abstractions;

namespace EasyPark.Api.Services;

public class EstacionamentoService
{
    private readonly IEstacionamentoRepository _estacionamentoRepository;
    private readonly IEnderecoRepository _enderecoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditEventRepository _auditEventRepository;

    public EstacionamentoService(
        IEstacionamentoRepository estacionamentoRepository,
        IEnderecoRepository enderecoRepository,
        IUnitOfWork unitOfWork,
        IAuditEventRepository auditEventRepository)
    {
        _estacionamentoRepository = estacionamentoRepository;
        _enderecoRepository = enderecoRepository;
        _unitOfWork = unitOfWork;
        _auditEventRepository = auditEventRepository;
    }

    public async Task<EstacionamentoOutDto> CreateAsync(EstacionamentoInDto dto, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("EstacionamentoService.Create");
        activity?.SetTag("estacionamento.nome", dto.Nome);
        activity?.SetTag("operadora.id", dto.OperadoraId);

        var enderecoDto = dto.Endereco ?? throw new ValidationException("Endereço é obrigatório");
        var endereco = await _enderecoRepository.UpsertAsync(enderecoDto, cancellationToken: cancellationToken);

        var estacionamento = new Estacionamento
        {
            OperadoraId = dto.OperadoraId,
            Nome = dto.Nome,
            Endereco = endereco,
            CriadoEm = DateTimeOffset.UtcNow
        };

        _estacionamentoRepository.Add(estacionamento);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var persisted = await _estacionamentoRepository.FindByIdWithEnderecoAsync(estacionamento.Id, cancellationToken: cancellationToken)
            ?? throw new EntityNotFoundException($"Estacionamento {estacionamento.Id} não encontrado");

        await WriteAuditAsync("ESTACIONAMENTO_CREATED", persisted.Id, new { persisted.Id, persisted.Nome, persisted.OperadoraId }, cancellationToken);
        return MapToDto(persisted);
    }

    public async Task<IEnumerable<EstacionamentoOutDto>> FindAllAsync(CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("EstacionamentoService.FindAll");
        var items = await _estacionamentoRepository.FindAllWithEnderecoAsync(cancellationToken);
        return items.Select(MapToDto);
    }

    public async Task<PagedResultDto<EstacionamentoOutDto>> SearchAsync(
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir,
        string? nome,
        string? ufSigla,
        string? cidadeNome,
        string? bairroNome,
        CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("EstacionamentoService.Search");
        activity?.SetTag("page", page);
        activity?.SetTag("page.size", pageSize);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize <= 0 ? 10 : pageSize, 1, 100);
        sortDir = string.IsNullOrWhiteSpace(sortDir) ? "asc" : sortDir.Trim().ToLowerInvariant();

        var result = await _estacionamentoRepository.SearchAsync(page, pageSize, sortBy, sortDir, nome, ufSigla, cidadeNome, bairroNome, cancellationToken);
        var totalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);

        return new PagedResultDto<EstacionamentoOutDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = result.TotalItems,
            TotalPages = totalPages,
            Items = result.Items.Select(MapToDto).ToList()
        };
    }

    public async Task<EstacionamentoOutDto> FindByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("EstacionamentoService.FindById");
        activity?.SetTag("estacionamento.id", id);

        var estacionamento = await _estacionamentoRepository.FindByIdWithEnderecoAsync(id, cancellationToken: cancellationToken)
            ?? throw new EntityNotFoundException($"Estacionamento {id} não encontrado");
        return MapToDto(estacionamento);
    }

    public async Task<EstacionamentoOutDto> UpdateAsync(long id, EstacionamentoInDto dto, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("EstacionamentoService.Update");
        activity?.SetTag("estacionamento.id", id);

        var estacionamento = await _estacionamentoRepository.FindTrackedByIdWithEnderecoAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException($"Estacionamento {id} não encontrado");

        var enderecoDto = dto.Endereco ?? throw new ValidationException("Endereço é obrigatório");
        await _enderecoRepository.UpsertAsync(enderecoDto, estacionamento.Endereco, cancellationToken);

        estacionamento.OperadoraId = dto.OperadoraId;
        estacionamento.Nome = dto.Nome;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("ESTACIONAMENTO_UPDATED", estacionamento.Id, new { estacionamento.Id, estacionamento.Nome, estacionamento.OperadoraId }, cancellationToken);

        return MapToDto(estacionamento);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("EstacionamentoService.Delete");
        activity?.SetTag("estacionamento.id", id);

        var estacionamento = await _estacionamentoRepository.FindTrackedByIdWithEnderecoAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException($"Estacionamento {id} não encontrado");

        _estacionamentoRepository.Remove(estacionamento);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("ESTACIONAMENTO_DELETED", id, new { Id = id }, cancellationToken);
    }

    private Task WriteAuditAsync(string eventType, long entityId, object payload, CancellationToken cancellationToken)
        => _auditEventRepository.WriteAsync(new AuditEventWriteDto(
            eventType,
            nameof(Estacionamento),
            entityId.ToString(),
            null,
            null,
            payload,
            "EstacionamentoService"), cancellationToken);

    private static EstacionamentoOutDto MapToDto(Estacionamento estacionamento)
        => new(estacionamento.Id, estacionamento.Nome, MapEndereco(estacionamento.Endereco));

    private static EnderecoOutDto? MapEndereco(Endereco? endereco)
    {
        if (endereco is null)
        {
            return null;
        }

        return new EnderecoOutDto(
            endereco.Id,
            endereco.Cep,
            endereco.Logradouro,
            endereco.Numero,
            endereco.Complemento,
            endereco.Bairro?.Nome,
            endereco.Bairro?.Cidade?.Nome,
            endereco.Bairro?.Cidade?.Uf?.Sigla,
            endereco.Bairro?.Cidade?.Uf?.Nome,
            endereco.Latitude,
            endereco.Longitude);
    }
}
