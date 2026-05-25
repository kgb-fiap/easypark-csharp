using EasyPark.Api.Dtos;
using EasyPark.Api.Exceptions;
using EasyPark.Api.Models;
using EasyPark.Application.Abstractions;
using EasyPark.Application.Security;

namespace EasyPark.Api.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditEventRepository _auditEventRepository;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IAuditEventRepository auditEventRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _auditEventRepository = auditEventRepository;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.FindByEmailAsync(dto.Email, cancellationToken: cancellationToken);
        if (existingUser is not null)
        {
            throw new ConflictException($"Já existe usuário cadastrado com o email {dto.Email}.");
        }

        var role = string.Equals(dto.Role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Cliente";
        var user = new Usuario
        {
            Nome = dto.Nome.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            Role = role,
            Suspenso = false,
            CriadoEm = DateTimeOffset.UtcNow
        };

        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditEventRepository.WriteAsync(new AuditEventWriteDto(
            "USER_REGISTERED",
            nameof(Usuario),
            user.Id.ToString(),
            user.Id,
            null,
            new { user.Id, user.Nome, user.Email, user.Role },
            "AuthService"), cancellationToken);

        return _tokenService.CreateToken(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByEmailAsync(dto.Email, cancellationToken: cancellationToken)
            ?? throw new UnauthorizedException("Credenciais inválidas.");

        if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Credenciais inválidas.");
        }

        if (user.Suspenso)
        {
            throw new ForbiddenException("Usuário suspenso.");
        }

        await _auditEventRepository.WriteAsync(new AuditEventWriteDto(
            "USER_LOGGED_IN",
            nameof(Usuario),
            user.Id.ToString(),
            user.Id,
            null,
            new { user.Id, user.Email },
            "AuthService"), cancellationToken);

        return _tokenService.CreateToken(user);
    }
}
