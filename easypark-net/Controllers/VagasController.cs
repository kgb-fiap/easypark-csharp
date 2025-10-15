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
    [ProducesResponseType(typeof(VagaOutDto), 201)]
    public async Task<ActionResult<VagaOutDto>> Create(VagaInDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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
    [ProducesResponseType(typeof(VagaOutDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<VagaOutDto>> GetById(long id)
    {
        var result = await _service.FindByIdAsync(id);
        return Ok(result);
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
    [HttpGet("/estacionamentos/{estacionamentoId:long}/vagas")]
    [ProducesResponseType(typeof(IEnumerable<VagaOutDto>), 200)]
    public async Task<IEnumerable<VagaOutDto>> GetByEstacionamento(long estacionamentoId)
    {
        return await _service.FindByEstacionamentoAsync(estacionamentoId);
    }
}