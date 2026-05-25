using EasyPark.Api.Dtos;
using EasyPark.Api.Services;
using EasyPark.Infrastructure.Auditing;
using EasyPark.Infrastructure.Options;
using EasyPark.Infrastructure.Persistence;
using EasyPark.Infrastructure.Repositories;
using EasyPark.Infrastructure.Security;
using EasyPark.UnitTests.TestSupport;
using Microsoft.Extensions.Options;

namespace EasyPark.UnitTests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_DadosValidos_RetornaTokenBearer()
    {
        await using var context = TestDbContextFactory.Create();
        var service = CreateService(context);

        var result = await service.RegisterAsync(new RegisterRequestDto("Gabriel", "gabriel@email.com", "123456", null));

        Assert.Equal("Bearer", result.TokenType);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("Cliente", result.Role);
    }

    [Fact]
    public async Task LoginAsync_CredenciaisValidas_RetornaToken()
    {
        await using var context = TestDbContextFactory.Create();
        var service = CreateService(context);
        await service.RegisterAsync(new RegisterRequestDto("Gabriel", "gabriel@email.com", "123456", null));

        var result = await service.LoginAsync(new LoginRequestDto("gabriel@email.com", "123456"));

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("gabriel@email.com", result.Email);
    }

    [Fact]
    public async Task LoginAsync_UsuarioComHashBcrypt_RetornaToken()
    {
        await using var context = TestDbContextFactory.Create();
        context.Usuarios.Add(new EasyPark.Api.Models.Usuario
        {
            Nome = "Admin",
            Email = "easypark.admin@fiap.com.br",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = "ADMIN",
            Suspenso = false,
            CriadoEm = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.LoginAsync(new LoginRequestDto("easypark.admin@fiap.com.br", "123456"));

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("Admin", result.Role);
    }

    private static AuthService CreateService(EasyPark.Api.Data.EasyParkContext context)
        => new(
            new UsuarioRepository(context),
            new Pbkdf2PasswordHasher(),
            new JwtTokenService(Options.Create(new JwtOptions())),
            new EfUnitOfWork(context),
            new InMemoryAuditEventRepository());
}
