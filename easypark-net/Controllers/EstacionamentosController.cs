using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EasyPark.Api.Dtos;
using EasyPark.Api.Services;

namespace EasyPark.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstacionamentosController : ControllerBase
{
    private readonly EstacionamentoService _service;
    public EstacionamentosController(EstacionamentoService service)
    {
        _service = service;
    }

    /// Cria um novo estacionamento.
    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto<EstacionamentoOutDto>), 201)]
    public async Task<ActionResult<ResourceDto<EstacionamentoOutDto>>> Create(EstacionamentoInDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ToResource(result));
    }

    /// Lista todos os estacionamentos.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EstacionamentoOutDto>), 200)]
    public async Task<IEnumerable<EstacionamentoOutDto>> GetAll()
    {
        return await _service.FindAllAsync();
    }

    /// Retorna os dados de um estacionamento pelo ID.
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ResourceDto<EstacionamentoOutDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ResourceDto<EstacionamentoOutDto>>> GetById(long id)
    {
        var result = await _service.FindByIdAsync(id);
        return Ok(ToResource(result));
    }

    /// Atualiza um estacionamento existente.
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(EstacionamentoOutDto), 200)]
    [ProducesResponseType(404)]
    public async Task<EstacionamentoOutDto> Update(long id, EstacionamentoInDto dto)
    {
        return await _service.UpdateAsync(id, dto);
    }

    /// Exclui um estacionamento. Retorna 204 em caso de sucesso.
    [HttpDelete("{id:long}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// Busca estacionamentos com filtros, ordenação e paginação.
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResourceDto<EstacionamentoOutDto>), 200)]
    public async Task<PagedResourceDto<EstacionamentoOutDto>> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = "asc",
        [FromQuery] string? nome = null,
        [FromQuery] string? ufSigla = null,
        [FromQuery] string? cidadeNome = null,
        [FromQuery] string? bairroNome = null)
    {
        var result = await _service.SearchAsync(page, pageSize, sortBy, sortDir, nome, ufSigla, cidadeNome, bairroNome);
        return ToPagedResource(result, page, pageSize, sortBy, sortDir, nome, ufSigla, cidadeNome, bairroNome);
    }

    private ResourceDto<EstacionamentoOutDto> ToResource(EstacionamentoOutDto dto)
    {
        var resource = new ResourceDto<EstacionamentoOutDto> { Data = dto };
        resource.Links.Add(new LinkDto
        {
            Rel = "self",
            Href = Url.ActionLink(nameof(GetById), values: new { id = dto.Id })!,
            Method = "GET"
        });
        resource.Links.Add(new LinkDto
        {
            Rel = "update",
            Href = Url.ActionLink(nameof(Update), values: new { id = dto.Id })!,
            Method = "PUT"
        });
        resource.Links.Add(new LinkDto
        {
            Rel = "delete",
            Href = Url.ActionLink(nameof(Delete), values: new { id = dto.Id })!,
            Method = "DELETE"
        });
        resource.Links.Add(new LinkDto
        {
            Rel = "vagas",
            Href = Url.ActionLink(nameof(VagasController.GetByEstacionamento), "Vagas", new { estacionamentoId = dto.Id })
                ?? $"/api/estacionamentos/{dto.Id}/vagas",
            Method = "GET"
        });
        return resource;
    }

    private PagedResourceDto<EstacionamentoOutDto> ToPagedResource(
        PagedResultDto<EstacionamentoOutDto> result,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir,
        string? nome,
        string? ufSigla,
        string? cidadeNome,
        string? bairroNome)
    {
        var resource = new PagedResourceDto<EstacionamentoOutDto>
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
            nome,
            ufSigla,
            cidadeNome,
            bairroNome
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