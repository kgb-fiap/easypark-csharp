using EasyPark.Api.Data;
using EasyPark.Api.Models;
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
    public const string ApiKey = "integration-test-key";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:ApiKey"] = ApiKey,
                ["ExternalServices:Eta:HealthUrl"] = ""
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

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", ApiKey);
        return client;
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EasyParkContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await SeedAsync(context);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }

    private static async Task SeedAsync(EasyParkContext context)
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
        context.Usuarios.Add(new Usuario { Id = 1, Nome = "Gabriel", Email = "gabriel@email.com" });

        await context.SaveChangesAsync();
    }
}
