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
    [ProducesResponseType(typeof(EstacionamentoOutDto), 201)]
    public async Task<ActionResult<EstacionamentoOutDto>> Create(EstacionamentoInDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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
    [ProducesResponseType(typeof(EstacionamentoOutDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<EstacionamentoOutDto>> GetById(long id)
    {
        var result = await _service.FindByIdAsync(id);
        return Ok(result);
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
}