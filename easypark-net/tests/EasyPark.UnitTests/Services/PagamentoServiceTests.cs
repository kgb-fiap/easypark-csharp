using EasyPark.Api.Dtos;
using EasyPark.Api.Models;
using EasyPark.Api.Services;
using EasyPark.UnitTests.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace EasyPark.UnitTests.Services;

public class PagamentoServiceTests
{
    [Fact]
    public async Task CreateAsync_ComPagadorECartao_PersisteDadosAninhados()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();
        context.Usuarios.Add(new Usuario { Id = 1, Nome = "Gabriel", Email = "gabriel@email.com" });
        await context.SaveChangesAsync();
        var service = new PagamentoService(context);
        var endereco = new EnderecoInDto("01000-000", "Av Paulista", "1000", null, "Bela Vista", "Sao Paulo", "SP", "Sao Paulo", null, null);
        var pagador = new PagamentoPagadorInDto("12345678901", "Gabriel", endereco);
        var cartao = new PagamentoCartaoInDto("Gabriel", "VISA", "1234", "tx-1");
        var dto = new PagamentoInDto(null, 1, 42.50m, null, "idem-1", pagador, cartao);

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.Equal("PENDENTE", result.Status);
        Assert.NotNull(result.Pagador);
        Assert.NotNull(result.Cartao);
        Assert.Equal(1, await context.Pagamentos.CountAsync());
        Assert.Equal(1, await context.PagamentoPagadores.CountAsync());
        Assert.Equal(1, await context.PagamentoCartoes.CountAsync());
    }
}
