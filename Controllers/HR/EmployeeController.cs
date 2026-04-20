using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Models;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/employee")]
public class EmployeeController : ControllerBase
{

    private readonly EmployeeService _service;

    public EmployeeController(EmployeeService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {

        if (!User.HasClaim("permission", "CREATE:EMPLOYEES")) return Forbid();

        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? fullName, [FromQuery] int? departmentId, [FromQuery] int? positionId)
    {

        if (!User.HasClaim("permission", "VIEW:EMPLOYEES") &&
            !User.HasClaim("permission", "CREATE:WORK_SCHEDULES") &&
            !User.HasClaim("permission", "CREATE:LEAVE_REQUESTS") &&
            !User.HasClaim("permission", "CREATE:PERFORMANCE_REVIEWS") &&
            !User.HasClaim("permission", "CREATE:INVENTORY_COUNTS") &&
            !User.HasClaim("permission", "CREATE:WAREHOUSES") &&
            !User.HasClaim("permission", "EDIT:WAREHOUSES") &&
            !User.HasClaim("permission", "CREATE:GOODS_RECEIPTS") &&
            !User.HasClaim("permission", "CREATE:PRODUCTION_ORDERS") &&
            !User.HasClaim("permission", "EDIT:PRODUCTION_ORDERS") &&
            !User.HasClaim("permission", "CREATE:INVENTORY_COUNTS") &&
            !User.HasClaim("permission", "CREATE:STOCK_MOVEMENTS") &&
            !User.HasClaim("permission", "EDIT:STOCK_MOVEMENTS")
        ) return Forbid();

        var result = await _service.GetAllAsync(fullName, departmentId, positionId);

        if (result == null) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {

        if (!User.HasClaim("permission", "VIEW:EMPLOYEES")) return Forbid();

        var result = await _service.GetByIdAsync(id);

        if (result == null) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeDto dto)
    {

        if (!User.HasClaim("permission", "EDIT:EMPLOYEES")) return Forbid();

        var result = await _service.UpdateAsync(id, dto);

        if (!result) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

        if (!User.HasClaim("permission", "DELETE:EMPLOYEES"))
            return Forbid();

        var result = await _service.DeleteAsync(id);

        if (!result) 
            return NotFound();

        return Ok(result);
    }

}