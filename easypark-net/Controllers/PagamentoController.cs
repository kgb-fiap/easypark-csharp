using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EasyPark.Api.Dtos;
using EasyPark.Api.Services;

namespace EasyPark.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagamentosController : ControllerBase
{
    private readonly PagamentoService _service;

    public PagamentosController(PagamentoService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PagamentoOutDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PagamentoOutDto>> Create(PagamentoInDto dto)
    {
        var pagamento = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = pagamento.Id }, pagamento);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(PagamentoOutDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagamentoOutDto>> GetById(long id)
    {
        var pagamento = await _service.FindByIdAsync(id);
        return Ok(pagamento);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResultDto<PagamentoOutDto>), StatusCodes.Status200OK)]
    public async Task<PagedResultDto<PagamentoOutDto>> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = "asc",
        [FromQuery] long? reservaId = null,
        [FromQuery] long? usuarioId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? metodo = null)
    {
        return await _service.SearchAsync(page, pageSize, sortBy, sortDir, reservaId, usuarioId, status, metodo);
    }
}