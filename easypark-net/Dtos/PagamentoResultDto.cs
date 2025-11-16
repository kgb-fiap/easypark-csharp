using System.Collections.Generic;
using System.Linq;

namespace EasyPark.Api.Dtos;

// Representa o resultado paginado padrão utilizado pelos endpoints de busca.
public class PagedResultDto<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalItems { get; set; }
    public int TotalPages { get; set; }
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
}

// Link HATEOAS genérico com relação, URL e método HTTP.
public class LinkDto
{
    public string Rel { get; set; } = null!;
    public string Href { get; set; } = null!;
    public string Method { get; set; } = null!;
}

// Wrapper de recursos para transportar dados e links relacionados.
public class ResourceDto<T>
{
    public T Data { get; set; } = default!;
    public List<LinkDto> Links { get; set; } = new();
}

// Versão paginada de um recurso com links adicionais para navegação.
public class PagedResourceDto<T> : PagedResultDto<T>
{
    public List<LinkDto> Links { get; set; } = new();
}