using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EasyPark.Api.Dtos;
using EasyPark.Api.Services;

namespace EasyPark.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly ReservaService _service;

    public ReservasController(ReservaService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReservaOutDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReservaOutDto>> Create(ReservaInDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ReservaOutDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReservaOutDto>> GetById(long id)
    {
        var reserva = await _service.FindByIdAsync(id);
        return Ok(reserva);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResultDto<ReservaOutDto>), StatusCodes.Status200OK)]
    public async Task<PagedResultDto<ReservaOutDto>> Search(
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
        return await _service.SearchAsync(page, pageSize, sortBy, sortDir, usuarioId, vagaId, status, dataInicioDe, dataInicioAte);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ReservaOutDto), StatusCodes.Status200OK)]
    public async Task<ReservaOutDto> Update(long id, ReservaInDto dto)
    {
        return await _service.UpdateAsync(id, dto);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}