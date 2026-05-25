using System.Net.Http.Headers;
using System.Net.Http.Json;
using EasyPark.Api.Data;
using EasyPark.Api.Models;
using EasyPark.Application.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyPark.IntegrationTests.Support;

public class EasyParkApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ExternalServices:Eta:HealthUrl"] = "",
                ["Mongo:ConnectionString"] = "",
                ["Jwt:Issuer"] = "EasyPark.Tests",
                ["Jwt:Audience"] = "EasyPark.Tests.Client",
                ["Jwt:SecretKey"] = "easypark-tests-super-secret-key-1234567890",
                ["Jwt:ExpirationMinutes"] = "120"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<EasyParkContext>>();
            services.RemoveAll<EasyParkContext>();
            services.AddSingleton(_connection);
            services.AddDbContext<EasyParkContext>((provider, options) =>
            {
                options.UseSqlite(provider.GetRequiredService<SqliteConnection>());
            });
        });
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(long userId = 1)
    {
        var client = CreateClient();
        var credentials = userId == 1
            ? new { email = "gabriel@email.com", password = "123456" }
            : new { email = "cliente@email.com", password = "123456" };

        var response = await client.PostAsJsonAsync("/api/auth/login", credentials);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<AuthLoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.Token);
        return client;
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EasyParkContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await context.Database.EnsureDeletedAsync();

        if (context.Database.IsSqlite())
        {
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            await context.Database.MigrateAsync();
        }

        await SeedAsync(context, passwordHasher);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }

    private static async Task SeedAsync(EasyParkContext context, IPasswordHasher passwordHasher)
    {
        var uf = new Uf { Sigla = "SP", Nome = "Sao Paulo" };
        var cidade = new Cidade { Id = 1, Nome = "Sao Paulo", UfSigla = "SP", Uf = uf };
        var bairro = new Bairro { Id = 1, Nome = "Bela Vista", CidadeId = 1, Cidade = cidade };
        var endereco = new Endereco
        {
            Id = 1,
            Cep = "01000-000",
            Logradouro = "Av Paulista",
            Numero = "1000",
            BairroId = 1,
            Bairro = bairro
        };
        var estacionamento = new Estacionamento
        {
            Id = 1,
            OperadoraId = 1,
            Nome = "EasyPark Central",
            EnderecoId = 1,
            Endereco = endereco
        };

        context.Ufs.Add(uf);
        context.Cidades.Add(cidade);
        context.Bairros.Add(bairro);
        context.Enderecos.Add(endereco);
        context.Estacionamentos.Add(estacionamento);
        context.Niveis.Add(new Nivel { Id = 1, EstacionamentoId = 1, Nome = "Terreo" });
        context.TiposVaga.Add(new TipoVaga { Id = 1, Nome = "Comum", TarifaPorMinuto = 0.25m });
        context.Vagas.Add(new Vaga { Id = 1, NivelId = 1, TipoVagaId = 1, Codigo = "A-01", Ativa = true });
        context.VagaStatus.Add(new VagaStatus
        {
            VagaId = 1,
            StatusOcupacao = "LIVRE",
            UltimoOcorrido = DateTimeOffset.UtcNow
        });
        context.Usuarios.Add(new Usuario
        {
            Id = 1,
            Nome = "Gabriel",
            Email = "gabriel@email.com",
            PasswordHash = passwordHasher.HashPassword("123456"),
            Role = "Admin",
            Suspenso = false,
            CriadoEm = DateTimeOffset.UtcNow
        });
        context.Usuarios.Add(new Usuario
        {
            Id = 2,
            Nome = "Cliente",
            Email = "cliente@email.com",
            PasswordHash = passwordHasher.HashPassword("123456"),
            Role = "Cliente",
            Suspenso = false,
            CriadoEm = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync();
    }

    private sealed record AuthLoginResponse(string Token, string TokenType, DateTimeOffset ExpiresAt, long UserId, string Email, string Role);
}
