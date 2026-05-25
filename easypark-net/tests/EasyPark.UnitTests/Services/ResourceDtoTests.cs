using EasyPark.Api.Dtos;
using EasyPark.Application.Abstractions;

namespace EasyPark.UnitTests.Services;

public class ResourceDtoTests
{
    [Fact]
    public void PagedResourceDto_PropriedadesComLinks_ArmazenamValores()
    {
        // Arrange
        var item = new ReservaOutDto(1, 2, 3, "PRE_RESERVA", DateTimeOffset.UtcNow, null, null, false, 10m, null);

        // Act
        var resource = new PagedResourceDto<ReservaOutDto>
        {
            Page = 1,
            PageSize = 10,
            TotalItems = 1,
            TotalPages = 1,
            Items = new[] { item },
            Links = new List<LinkDto>
            {
                new() { Rel = "self", Href = "/api/reservas", Method = "GET" }
            }
        };

        // Assert
        Assert.Single(resource.Items);
        Assert.Single(resource.Links);
        Assert.Equal("self", resource.Links[0].Rel);
    }

    [Fact]
    public void ResourceDto_ComDataELinks_ArmazenamValores()
    {
        // Arrange
        var data = new PagamentoOutDto(1, 2, 3, "PAGO", 42m, "idem", DateTimeOffset.UtcNow.ToString("O"), null, null);

        // Act
        var resource = new ResourceDto<PagamentoOutDto>
        {
            Data = data,
            Links = new List<LinkDto>
            {
                new() { Rel = "self", Href = "/api/pagamentos/1", Method = "GET" },
                new() { Rel = "search-by-user", Href = "/api/pagamentos?usuarioId=3", Method = "GET" }
            }
        };

        // Assert
        Assert.Equal(1, resource.Data.Id);
        Assert.Equal(2, resource.Links.Count);
    }

    [Fact]
    public void PagedQueryResult_ComItens_TotalizaResultado()
    {
        // Arrange
        var items = new[] { "A", "B" };

        // Act
        var result = new PagedQueryResult<string>(items, items.Length);

        // Assert
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(items, result.Items);
    }
}
