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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkScheduleDto dto)
    {
        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? name, [FromQuery] DateTime? date)
    {
        var result = await _service.GetAllAsync(name, date);

        if (result == null) NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result) NotFound();

        return Ok(result);
    }
}