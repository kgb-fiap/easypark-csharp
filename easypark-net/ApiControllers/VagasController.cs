using EasyPark.Api.Dtos;
using EasyPark.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPark.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class VagasController : ControllerBase
{
    private readonly VagaService _service;

    public VagasController(VagaService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto<VagaOutDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResourceDto<VagaOutDto>>> Create(VagaInDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ToResource(result));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VagaOutDto>), StatusCodes.Status200OK)]
    public async Task<IEnumerable<VagaOutDto>> GetAll([FromQuery] string? status)
        => await _service.FindAllAsync(status);

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ResourceDto<VagaOutDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResourceDto<VagaOutDto>>> GetById(long id)
        => Ok(ToResource(await _service.FindByIdAsync(id)));

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ResourceDto<VagaOutDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResourceDto<VagaOutDto>>> Update(long id, VagaInDto dto)
        => Ok(ToResource(await _service.UpdateAsync(id, dto)));

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id:long}/status")]
    [ProducesResponseType(typeof(VagaStatusOutDto), StatusCodes.Status200OK)]
    public Task<VagaStatusOutDto> GetStatus(long id)
        => _service.GetStatusAsync(id);

    [HttpGet("~/api/estacionamentos/{estacionamentoId:long}/vagas")]
    [ProducesResponseType(typeof(IEnumerable<VagaOutDto>), StatusCodes.Status200OK)]
    public async Task<IEnumerable<VagaOutDto>> GetByEstacionamento(long estacionamentoId)
        => await _service.FindByEstacionamentoAsync(estacionamentoId);

    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResourceDto<VagaOutDto>), StatusCodes.Status200OK)]
    public async Task<PagedResourceDto<VagaOutDto>> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = "asc",
        [FromQuery] long? estacionamentoId = null,
        [FromQuery] long? nivelId = null,
        [FromQuery] long? tipoVagaId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? codigo = null)
    {
        var result = await _service.SearchAsync(page, pageSize, sortBy, sortDir, estacionamentoId, nivelId, tipoVagaId, status, codigo);
        return ToPagedResource(result, page, pageSize, sortBy, sortDir, estacionamentoId, nivelId, tipoVagaId, status, codigo);
    }

    private ResourceDto<VagaOutDto> ToResource(VagaOutDto dto)
    {
        var resource = new ResourceDto<VagaOutDto> { Data = dto };
        resource.Links.Add(new LinkDto { Rel = "self", Href = Url.ActionLink(nameof(GetById), values: new { id = dto.Id })!, Method = "GET" });
        resource.Links.Add(new LinkDto { Rel = "update", Href = Url.ActionLink(nameof(Update), values: new { id = dto.Id })!, Method = "PUT" });
        resource.Links.Add(new LinkDto { Rel = "delete", Href = Url.ActionLink(nameof(Delete), values: new { id = dto.Id })!, Method = "DELETE" });
        resource.Links.Add(new LinkDto { Rel = "status", Href = Url.ActionLink(nameof(GetStatus), values: new { id = dto.Id })!, Method = "GET" });
        return resource;
    }

    private PagedResourceDto<VagaOutDto> ToPagedResource(
        PagedResultDto<VagaOutDto> result,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir,
        long? estacionamentoId,
        long? nivelId,
        long? tipoVagaId,
        string? status,
        string? codigo)
    {
        var resource = new PagedResourceDto<VagaOutDto>
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
            estacionamentoId,
            nivelId,
            tipoVagaId,
            status,
            codigo
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
