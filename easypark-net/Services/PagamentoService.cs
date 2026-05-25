using EasyPark.Api.Dtos;
using EasyPark.Api.Exceptions;
using EasyPark.Api.Models;
using EasyPark.Api.Observability;
using EasyPark.Application.Abstractions;

namespace EasyPark.Api.Services;

public class PagamentoService
{
    private readonly IPagamentoRepository _pagamentoRepository;
    private readonly IReservaRepository _reservaRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEnderecoRepository _enderecoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditEventRepository _auditEventRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public PagamentoService(
        IPagamentoRepository pagamentoRepository,
        IReservaRepository reservaRepository,
        IUserRepository userRepository,
        IEnderecoRepository enderecoRepository,
        IUnitOfWork unitOfWork,
        IAuditEventRepository auditEventRepository,
        ICurrentUserContext currentUserContext)
    {
        _pagamentoRepository = pagamentoRepository;
        _reservaRepository = reservaRepository;
        _userRepository = userRepository;
        _enderecoRepository = enderecoRepository;
        _unitOfWork = unitOfWork;
        _auditEventRepository = auditEventRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<PagamentoOutDto> CreateAsync(PagamentoInDto dto, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("PagamentoService.Create");

        var actorId = RequireAuthenticatedUserId();
        var usuarioId = await ResolveUsuarioIdAsync(dto, cancellationToken);

        if (dto.ReservaId.HasValue)
        {
            var reserva = await _reservaRepository.FindByIdAsync(dto.ReservaId.Value, cancellationToken: cancellationToken)
                ?? throw new EntityNotFoundException($"Reserva {dto.ReservaId.Value} não encontrada");

            EnsureCanAccess(reserva.UsuarioId);
        }

        if (usuarioId.HasValue)
        {
            _ = await _userRepository.FindByIdAsync(usuarioId.Value, cancellationToken: cancellationToken)
                ?? throw new EntityNotFoundException($"Usuário {usuarioId.Value} não encontrado");
        }

        var pagamento = new Pagamento
        {
            ReservaId = dto.ReservaId,
            UsuarioId = usuarioId,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "PENDENTE" : dto.Status.Trim().ToUpperInvariant(),
            Valor = dto.Valor,
            IdempotenciaChave = dto.IdempotenciaChave,
            CriadoEm = DateTimeOffset.UtcNow
        };

        _pagamentoRepository.Add(pagamento);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        PagamentoPagador? pagador = null;
        if (dto.Pagador is not null)
        {
            pagador = new PagamentoPagador
            {
                PagamentoId = pagamento.Id,
                CpfCnpj = dto.Pagador.CpfCnpj,
                Nome = dto.Pagador.Nome
            };

            if (dto.Pagador.Endereco is not null)
            {
                pagador.Endereco = await _enderecoRepository.UpsertAsync(dto.Pagador.Endereco, cancellationToken: cancellationToken);
            }

            _pagamentoRepository.AddPagador(pagador);
        }

        PagamentoCartao? cartao = null;
        if (dto.Cartao is not null)
        {
            cartao = new PagamentoCartao
            {
                PagamentoId = pagamento.Id,
                Titular = dto.Cartao.Titular,
                Bandeira = dto.Cartao.Bandeira,
                UltimosDigitos = dto.Cartao.UltimosDigitos,
                TransacaoId = dto.Cartao.TransacaoId
            };

            _pagamentoRepository.AddCartao(cartao);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("PAGAMENTO_CREATED", pagamento.Id, actorId, new { pagamento.Id, pagamento.UsuarioId, pagamento.ReservaId, pagamento.Status, pagamento.Valor }, cancellationToken);

        return MapPagamento(pagamento, pagador, cartao);
    }

    public async Task<PagamentoOutDto> FindByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("PagamentoService.FindById");

        var pagamento = await _pagamentoRepository.FindByIdAsync(id, cancellationToken: cancellationToken)
            ?? throw new EntityNotFoundException($"Pagamento {id} não encontrado");

        EnsureCanAccess(pagamento.UsuarioId);

        var pagador = await _pagamentoRepository.FindPagadorByPagamentoIdAsync(id, cancellationToken);
        var cartao = await _pagamentoRepository.FindCartaoByPagamentoIdAsync(id, cancellationToken);

        return MapPagamento(pagamento, pagador, cartao);
    }

    public async Task<PagedResultDto<PagamentoOutDto>> SearchAsync(
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir,
        long? reservaId,
        long? usuarioId,
        string? status,
        string? metodo,
        CancellationToken cancellationToken = default)
    {
        using var activity = EasyParkTelemetry.ActivitySource.StartActivity("PagamentoService.Search");

        var actorId = RequireAuthenticatedUserId();
        if (!_currentUserContext.IsAdmin)
        {
            usuarioId = actorId;
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize <= 0 ? 10 : pageSize, 1, 100);
        sortDir = string.IsNullOrWhiteSpace(sortDir) ? "asc" : sortDir.Trim().ToLowerInvariant();

        var result = await _pagamentoRepository.SearchAsync(page, pageSize, sortBy, sortDir, reservaId, usuarioId, status, metodo, cancellationToken);
        var totalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);
        var paymentIds = result.Items.Select(x => x.Id).ToList();

        var pagadores = await _pagamentoRepository.FindPagadoresByPagamentoIdsAsync(paymentIds, cancellationToken);
        var cartoes = await _pagamentoRepository.FindCartoesByPagamentoIdsAsync(paymentIds, cancellationToken);

        return new PagedResultDto<PagamentoOutDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = result.TotalItems,
            TotalPages = totalPages,
            Items = result.Items.Select(pg => MapPagamento(
                pg,
                pagadores.TryGetValue(pg.Id, out var pagador) ? pagador : null,
                cartoes.TryGetValue(pg.Id, out var cartao) ? cartao : null)).ToList()
        };
    }

    private async Task<long?> ResolveUsuarioIdAsync(PagamentoInDto dto, CancellationToken cancellationToken)
    {
        if (_currentUserContext.IsAdmin)
        {
            return dto.UsuarioId;
        }

        if (!_currentUserContext.UserId.HasValue)
        {
            throw new UnauthorizedException("Usuário autenticado é obrigatório.");
        }

        if (dto.UsuarioId.HasValue && dto.UsuarioId.Value != _currentUserContext.UserId.Value)
        {
            throw new ForbiddenException("Você não pode criar pagamentos para outro usuário.");
        }

        if (dto.ReservaId.HasValue)
        {
            var reserva = await _reservaRepository.FindByIdAsync(dto.ReservaId.Value, cancellationToken: cancellationToken)
                ?? throw new EntityNotFoundException($"Reserva {dto.ReservaId.Value} não encontrada");

            if (reserva.UsuarioId != _currentUserContext.UserId.Value)
            {
                throw new ForbiddenException("Você não pode pagar uma reserva de outro usuário.");
            }

            return reserva.UsuarioId;
        }

        return _currentUserContext.UserId.Value;
    }

    private void EnsureCanAccess(long? usuarioId)
    {
        if (_currentUserContext.IsAdmin)
        {
            return;
        }

        if (!_currentUserContext.UserId.HasValue || usuarioId != _currentUserContext.UserId.Value)
        {
            throw new ForbiddenException("Você não tem acesso a este pagamento.");
        }
    }

    private long RequireAuthenticatedUserId()
        => _currentUserContext.UserId ?? throw new UnauthorizedException("Usuário autenticado é obrigatório.");

    private Task WriteAuditAsync(string eventType, long entityId, long actorId, object payload, CancellationToken cancellationToken)
        => _auditEventRepository.WriteAsync(new AuditEventWriteDto(
            eventType,
            nameof(Pagamento),
            entityId.ToString(),
            actorId,
            _currentUserContext.CorrelationId,
            payload,
            "PagamentoService"), cancellationToken);

    private static PagamentoOutDto MapPagamento(Pagamento pagamento, PagamentoPagador? pagador, PagamentoCartao? cartao)
        => new(
            pagamento.Id,
            pagamento.ReservaId,
            pagamento.UsuarioId,
            pagamento.Status,
            pagamento.Valor,
            pagamento.IdempotenciaChave,
            pagamento.CriadoEm.ToString("O"),
            pagador == null ? null : new PagamentoPagadorOutDto(
                pagador.CpfCnpj,
                pagador.Nome,
                MapEndereco(pagador.Endereco)),
            cartao == null ? null : new PagamentoCartaoOutDto(
                cartao.Titular,
                cartao.Bandeira,
                cartao.UltimosDigitos,
                cartao.TransacaoId));

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
