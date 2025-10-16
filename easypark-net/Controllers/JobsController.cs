using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EasyPark.Api.Dtos;
using EasyPark.Api.Services;

namespace EasyPark.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly JobsService _service;
    public JobsController(JobsService service)
    {
        _service = service;
    }

    /// Dispara o job que cancela reservas expiradas.
    [HttpPost("reservas/timeouts")]
    [ProducesResponseType(typeof(JobCountOutDto), 200)]
    public async Task<JobCountOutDto> ReservaTimeouts()
    {
        return await _service.ReservaTimeoutsAsync();
    }

    /// Dispara o job que cancela pré-reservas expiradas.
    [HttpPost("prereservas/timeouts")]
    [ProducesResponseType(typeof(JobCountOutDto), 200)]
    public async Task<JobCountOutDto> PreReservaTimeouts()
    {
        return await _service.PreReservaTimeoutsAsync();
    }

    /// Atualiza o ETA (tempo estimado de chegada) de uma reserva.
    /// O valor em minutos é passado como query parameter.
    [HttpPost("reservas/{id:long}/eta")]
    [ProducesResponseType(typeof(EtaUpdateOutDto), 200)]
    public async Task<EtaUpdateOutDto> AtualizarEta(long id, [FromQuery] int minutos)
    {
        return await _service.AtualizarEtaAsync(id, minutos);
    }
}