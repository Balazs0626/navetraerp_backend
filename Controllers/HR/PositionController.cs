using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Models;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionController : ControllerBase
{

    private readonly PositionService _service;

    public PositionController(PositionService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PositionDto dto)
    {

        if (!User.HasClaim("permission", "CREATE:POSITIONS"))
            return Forbid();

        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {

        if (!User.HasClaim("permission", "VIEW:POSITIONS"))
            return Forbid();

        var result = await _service.GetAllAsync();

        if (result == null) NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {

        if (!User.HasClaim("permission", "VIEW:POSITIONS"))
            return Forbid();

        var result = await _service.GetByIdAsync(id);

        if (result == null) NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PositionDto dto)
    {

        if (!User.HasClaim("permission", "EDIT:POSITIONS"))
            return Forbid();

        var result = await _service.UpdateAsync(id, dto);

        if (!result) NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

        if (!User.HasClaim("permission", "DELETE:POSITIONS"))
            return Forbid();

        var result = await _service.DeleteAsync(id);

        if (!result) NotFound();

        return Ok(result);
    }
}