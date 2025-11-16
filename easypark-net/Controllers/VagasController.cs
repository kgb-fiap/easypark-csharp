using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EasyPark.Api.Dtos;
using EasyPark.Api.Services;

namespace EasyPark.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VagasController : ControllerBase
{
    private readonly VagaService _service;
    public VagasController(VagaService service)
    {
        _service = service;
    }

    /// Cria uma nova vaga. Retorna 201 Created com o DTO de saída.
    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto<VagaOutDto>), 201)]
    public async Task<ActionResult<ResourceDto<VagaOutDto>>> Create(VagaInDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ToResource(result));
    }

    /// Lista todas as vagas. Pode filtrar por status de ocupação.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VagaOutDto>), 200)]
    public async Task<IEnumerable<VagaOutDto>> GetAll([FromQuery] string? status)
    {
        return await _service.FindAllAsync(status);
    }

    /// Retorna os detalhes de uma vaga pelo ID.
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ResourceDto<VagaOutDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ResourceDto<VagaOutDto>>> GetById(long id)
    {
        var result = await _service.FindByIdAsync(id);
        return Ok(ToResource(result));
    }

    /// Atualiza uma vaga. Retorna o DTO atualizado.
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(VagaOutDto), 200)]
    [ProducesResponseType(404)]
    public async Task<VagaOutDto> Update(long id, VagaInDto dto)
    {
        return await _service.UpdateAsync(id, dto);
    }

    /// Remove uma vaga. Retorna 204 em caso de sucesso.
    [HttpDelete("{id:long}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// Recupera o status atual de uma vaga (LIVRE, OCUPADA ou DESCONHECIDO).
    [HttpGet("{id:long}/status")]
    [ProducesResponseType(typeof(VagaStatusOutDto), 200)]
    public async Task<VagaStatusOutDto> GetStatus(long id)
    {
        return await _service.GetStatusAsync(id);
    }

    /// Lista todas as vagas pertencentes a um estacionamento.
    [HttpGet("~/api/estacionamentos/{estacionamentoId:long}/vagas")]
    [ProducesResponseType(typeof(IEnumerable<VagaOutDto>), 200)]
    public async Task<IEnumerable<VagaOutDto>> GetByEstacionamento(long estacionamentoId)
    {
        return await _service.FindByEstacionamentoAsync(estacionamentoId);
    }

    /// Pesquisa vagas com filtros, ordenação e paginação.
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResourceDto<VagaOutDto>), 200)]
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
            Rel = "status",
            Href = Url.ActionLink(nameof(GetStatus), values: new { id = dto.Id })!,
            Method = "GET"
        });
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