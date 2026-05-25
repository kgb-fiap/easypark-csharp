using EasyPark.Api.Dtos;
using EasyPark.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPark.Api.Controllers;

[ApiController]
[Route("api/auditoria/eventos-sensor")]
[Authorize(Roles = "Admin")]
public class AuditoriaController : ControllerBase
{
    private readonly AuditService _auditService;

    public AuditoriaController(AuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<AuditEventOutDto>), StatusCodes.Status200OK)]
    public async Task<PagedResultDto<AuditEventOutDto>> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? eventType = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        [FromQuery] long? userId = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize <= 0 ? 10 : pageSize, 1, 100);

        var result = await _auditService.SearchAsync(page, pageSize, eventType, entityType, entityId, userId);
        return new PagedResultDto<AuditEventOutDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = result.TotalItems,
            TotalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize),
            Items = result.Items
        };
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuditEventOutDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuditEventOutDto>> GetById(string id)
    {
        var result = await _auditService.FindByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}
