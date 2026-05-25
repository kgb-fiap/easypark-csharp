using EasyPark.Api.Dtos;
using EasyPark.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPark.Api.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize(Roles = "Admin")]
public class JobsController : ControllerBase
{
    private readonly JobsService _service;

    public JobsController(JobsService service)
    {
        _service = service;
    }

    [HttpPost("reservas/timeouts")]
    [ProducesResponseType(typeof(JobCountOutDto), StatusCodes.Status200OK)]
    public Task<JobCountOutDto> ReservaTimeouts()
        => _service.ReservaTimeoutsAsync();

    [HttpPost("prereservas/timeouts")]
    [ProducesResponseType(typeof(JobCountOutDto), StatusCodes.Status200OK)]
    public Task<JobCountOutDto> PreReservaTimeouts()
        => _service.PreReservaTimeoutsAsync();

    [HttpPost("reservas/{id:long}/eta")]
    [ProducesResponseType(typeof(EtaUpdateOutDto), StatusCodes.Status200OK)]
    public Task<EtaUpdateOutDto> AtualizarEta(long id, [FromQuery] int minutos)
        => _service.AtualizarEtaAsync(id, minutos);
}
