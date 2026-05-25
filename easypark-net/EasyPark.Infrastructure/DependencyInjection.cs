using EasyPark.Api.Data;
using EasyPark.Application.Abstractions;
using EasyPark.Application.Security;
using EasyPark.Infrastructure.Auditing;
using EasyPark.Infrastructure.Options;
using EasyPark.Infrastructure.Persistence;
using EasyPark.Infrastructure.Repositories;
using EasyPark.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EasyPark.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") ?? string.Empty;

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<MongoOptions>(configuration.GetSection(MongoOptions.SectionName));

        services.AddDbContext<EasyParkContext>(options => options.UseOracle(connectionString));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IEnderecoRepository, EnderecoRepository>();
        services.AddScoped<IEstacionamentoRepository, EstacionamentoRepository>();
        services.AddScoped<IVagaRepository, VagaRepository>();
        services.AddScoped<IUserRepository, UsuarioRepository>();
        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<IPagamentoRepository, PagamentoRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddSingleton<IAuditEventRepository>(provider =>
        {
            var mongoOptions = provider.GetRequiredService<IOptions<MongoOptions>>();
            return string.IsNullOrWhiteSpace(mongoOptions.Value.ConnectionString)
                ? new InMemoryAuditEventRepository()
                : new MongoAuditEventRepository(mongoOptions);
        });

        return services;
    }
}
