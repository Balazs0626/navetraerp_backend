using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/warehouses")]
public class WarehouseController : ControllerBase
{

    private readonly WarehouseService _service;

    public WarehouseController(WarehouseService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto)
    {

        if (!User.HasClaim("permission", "CREATE:WAREHOUSES")) return Forbid();

        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {

        if (!User.HasClaim("permission", "VIEW:WAREHOUSES")) return Forbid();

        var result = await _service.GetAllAsync();

        if (result == null) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {

        if (!User.HasClaim("permission", "VIEW:WAREHOUSES")) return Forbid();

        var result = await _service.GetByIdAsync(id);

        if (result == null) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWarehouseDto dto)
    {

        if (!User.HasClaim("permission", "EDIT:WAREHOUSES")) return Forbid();

        var result = await _service.UpdateAsync(id, dto);

        if (!result) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

        if (!User.HasClaim("permission", "DELETE:WAREHOUSES")) return Forbid();

        var result = await _service.DeleteAsync(id);

        if (!result) return NotFound();

        return Ok(result);
    }
}