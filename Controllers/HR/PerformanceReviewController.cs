using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Models;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/performance_reviews")]
public class PerformanceReviewController : ControllerBase
{

    private readonly PerformanceReviewService _service;

    public PerformanceReviewController(PerformanceReviewService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePerformanceReviewDto dto)
    {
        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? employeeName, [FromQuery] DateTime? date)
    {
        var result = await _service.GetAllAsync(employeeName, date);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result) 
            return NotFound();

        return Ok(result);
    }
}