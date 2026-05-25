using EasyPark.Api.Dtos;
using EasyPark.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPark.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Cliente")]
public class ReservasController : ControllerBase
{
    private readonly ReservaService _service;

    public ReservasController(ReservaService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto<ReservaOutDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResourceDto<ReservaOutDto>>> Create(ReservaInDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ToResource(result));
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ResourceDto<ReservaOutDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResourceDto<ReservaOutDto>>> GetById(long id)
        => Ok(ToResource(await _service.FindByIdAsync(id)));

    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResourceDto<ReservaOutDto>), StatusCodes.Status200OK)]
    public async Task<PagedResourceDto<ReservaOutDto>> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = "asc",
        [FromQuery] long? usuarioId = null,
        [FromQuery] long? vagaId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTimeOffset? dataInicioDe = null,
        [FromQuery] DateTimeOffset? dataInicioAte = null)
    {
        var result = await _service.SearchAsync(page, pageSize, sortBy, sortDir, usuarioId, vagaId, status, dataInicioDe, dataInicioAte);
        return ToPagedResource(result, page, pageSize, sortBy, sortDir, usuarioId, vagaId, status, dataInicioDe, dataInicioAte);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ResourceDto<ReservaOutDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResourceDto<ReservaOutDto>>> Update(long id, ReservaInDto dto)
        => Ok(ToResource(await _service.UpdateAsync(id, dto)));

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    private ResourceDto<ReservaOutDto> ToResource(ReservaOutDto dto)
    {
        var resource = new ResourceDto<ReservaOutDto> { Data = dto };
        resource.Links.Add(new LinkDto { Rel = "self", Href = Url.ActionLink(nameof(GetById), values: new { id = dto.Id })!, Method = "GET" });
        resource.Links.Add(new LinkDto { Rel = "update", Href = Url.ActionLink(nameof(Update), values: new { id = dto.Id })!, Method = "PUT" });
        resource.Links.Add(new LinkDto { Rel = "delete", Href = Url.ActionLink(nameof(Delete), values: new { id = dto.Id })!, Method = "DELETE" });
        resource.Links.Add(new LinkDto { Rel = "pagamentos", Href = Url.ActionLink(nameof(PagamentosController.Search), "Pagamentos", new { reservaId = dto.Id })!, Method = "GET" });
        return resource;
    }

    private PagedResourceDto<ReservaOutDto> ToPagedResource(
        PagedResultDto<ReservaOutDto> result,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir,
        long? usuarioId,
        long? vagaId,
        string? status,
        DateTimeOffset? dataInicioDe,
        DateTimeOffset? dataInicioAte)
    {
        var resource = new PagedResourceDto<ReservaOutDto>
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
            usuarioId,
            vagaId,
            status,
            dataInicioDe,
            dataInicioAte
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
