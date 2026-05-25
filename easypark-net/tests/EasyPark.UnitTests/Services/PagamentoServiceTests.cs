using EasyPark.Api.Dtos;
using EasyPark.Api.Models;
using EasyPark.Api.Services;
using EasyPark.Infrastructure.Auditing;
using EasyPark.Infrastructure.Persistence;
using EasyPark.Infrastructure.Repositories;
using EasyPark.UnitTests.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace EasyPark.UnitTests.Services;

public class PagamentoServiceTests
{
    [Fact]
    public async Task CreateAsync_ComPagadorECartao_PersisteDadosAninhados()
    {
        await using var context = TestDbContextFactory.Create();
        context.Usuarios.Add(new Usuario { Id = 1, Nome = "Gabriel", Email = "gabriel@email.com", PasswordHash = "hash", Role = "Cliente" });
        await context.SaveChangesAsync();

        var service = CreateService(context, new TestCurrentUserContext { UserId = 1, Role = "Cliente" });
        var endereco = new EnderecoInDto("01000-000", "Av Paulista", "1000", null, "Bela Vista", "Sao Paulo", "SP", "Sao Paulo", null, null);
        var pagador = new PagamentoPagadorInDto("12345678901", "Gabriel", endereco);
        var cartao = new PagamentoCartaoInDto("Gabriel", "VISA", "1234", "tx-1");
        var dto = new PagamentoInDto(null, 1, 42.50m, null, "idem-1", pagador, cartao);

        var result = await service.CreateAsync(dto);

        Assert.Equal("PENDENTE", result.Status);
        Assert.NotNull(result.Pagador);
        Assert.NotNull(result.Cartao);
        Assert.Equal(1, await context.Pagamentos.CountAsync());
        Assert.Equal(1, await context.PagamentoPagadores.CountAsync());
        Assert.Equal(1, await context.PagamentoCartoes.CountAsync());
    }

    private static PagamentoService CreateService(EasyPark.Api.Data.EasyParkContext context, TestCurrentUserContext currentUser)
        => new(
            new PagamentoRepository(context),
            new ReservaRepository(context),
            new UsuarioRepository(context),
            new EnderecoRepository(context),
            new EfUnitOfWork(context),
            new InMemoryAuditEventRepository(),
            currentUser);
}
