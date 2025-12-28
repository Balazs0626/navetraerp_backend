using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/work_schedules")]
public class WorkScheduleController : ControllerBase
{

    private readonly WorkScheduleService _service;

    public WorkScheduleController(WorkScheduleService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkScheduleDto dto)
    {

        if (!User.HasClaim("permission", "CREATE:WORK_SCHEDULES"))
            return Forbid();

        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? name, [FromQuery] DateTime? date)
    {

        if (!User.HasClaim("permission", "VIEW:WORK_SCHEDULES"))
            return Forbid();

        var result = await _service.GetAllAsync(name, date);

        if (result == null) NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

        if (!User.HasClaim("permission", "DELETE:WORK_SCHEDULES"))
            return Forbid();

        var result = await _service.DeleteAsync(id);

        if (!result) NotFound();

        return Ok(result);
    }
}