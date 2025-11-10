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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ShiftDto dto)
    {
        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();

        if (result == null) NotFound();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null) NotFound();

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ShiftDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);

        if (!result) NotFound();

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