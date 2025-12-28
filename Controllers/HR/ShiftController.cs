using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Models;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/shifts")]
public class ShiftController : ControllerBase
{

    private readonly ShiftService _service;

    public ShiftController(ShiftService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ShiftDto dto)
    {

        if (!User.HasClaim("permission", "CREATE:SHIFTS"))
            return Forbid();

        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {

        if (!User.HasClaim("permission", "VIEW:SHIFTS"))
            return Forbid();

        var result = await _service.GetAllAsync();

        if (result == null) NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {

        if (!User.HasClaim("permission", "VIEW:SHIFTS"))
            return Forbid();

        var result = await _service.GetByIdAsync(id);

        if (result == null) NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ShiftDto dto)
    {

        if (!User.HasClaim("permission", "EDIT:SHIFTS"))
            return Forbid();

        var result = await _service.UpdateAsync(id, dto);

        if (!result) NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

        if (!User.HasClaim("permission", "DELETE:SHIFTS"))
            return Forbid();

        var result = await _service.DeleteAsync(id);

        if (!result) NotFound();

        return Ok(result);
    }

}