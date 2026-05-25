using EasyPark.Api.Dtos;
using EasyPark.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPark.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Cliente")]
public class PagamentosController : ControllerBase
{
    private readonly PagamentoService _service;

    public PagamentosController(PagamentoService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto<PagamentoOutDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResourceDto<PagamentoOutDto>>> Create(PagamentoInDto dto)
    {
        var pagamento = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = pagamento.Id }, ToResource(pagamento));
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ResourceDto<PagamentoOutDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResourceDto<PagamentoOutDto>>> GetById(long id)
        => Ok(ToResource(await _service.FindByIdAsync(id)));

    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResourceDto<PagamentoOutDto>), StatusCodes.Status200OK)]
    public async Task<PagedResourceDto<PagamentoOutDto>> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = "asc",
        [FromQuery] long? reservaId = null,
        [FromQuery] long? usuarioId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? metodo = null)
    {
        var result = await _service.SearchAsync(page, pageSize, sortBy, sortDir, reservaId, usuarioId, status, metodo);
        return ToPagedResource(result, page, pageSize, sortBy, sortDir, reservaId, usuarioId, status, metodo);
    }

    private ResourceDto<PagamentoOutDto> ToResource(PagamentoOutDto dto)
    {
        var resource = new ResourceDto<PagamentoOutDto> { Data = dto };
        resource.Links.Add(new LinkDto { Rel = "self", Href = Url.ActionLink(nameof(GetById), values: new { id = dto.Id })!, Method = "GET" });
        resource.Links.Add(new LinkDto { Rel = "search-by-user", Href = Url.ActionLink(nameof(Search), values: new { usuarioId = dto.UsuarioId })!, Method = "GET" });

        if (dto.ReservaId.HasValue)
        {
            resource.Links.Add(new LinkDto { Rel = "reserva", Href = Url.ActionLink(nameof(ReservasController.GetById), "Reservas", new { id = dto.ReservaId })!, Method = "GET" });
        }

        return resource;
    }

    private PagedResourceDto<PagamentoOutDto> ToPagedResource(
        PagedResultDto<PagamentoOutDto> result,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir,
        long? reservaId,
        long? usuarioId,
        string? status,
        string? metodo)
    {
        var resource = new PagedResourceDto<PagamentoOutDto>
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages,
            Items = result.Items
        };

        string CreateLink(int targetPage) => Url.ActionLink(nameof(Search), values: new
        {
            page = targetPage,
            pageSize,
            sortBy,
            sortDir,
            reservaId,
            usuarioId,
            status,
            metodo
        })!;

        resource.Links.Add(new LinkDto { Rel = "self", Href = CreateLink(page), Method = "GET" });
        if (page > 1)
        {
            resource.Links.Add(new LinkDto { Rel = "prev", Href = CreateLink(page - 1), Method = "GET" });
            resource.Links.Add(new LinkDto { Rel = "first", Href = CreateLink(1), Method = "GET" });
        }

        if (page < result.TotalPages)
        {
            resource.Links.Add(new LinkDto { Rel = "next", Href = CreateLink(page + 1), Method = "GET" });
            resource.Links.Add(new LinkDto { Rel = "last", Href = CreateLink(result.TotalPages), Method = "GET" });
        }

        return resource;
    }
}
