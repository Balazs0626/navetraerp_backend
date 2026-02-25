using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/sales_orders")]
public class SalesOrderController : ControllerBase
{

    private readonly SalesOrderService _service;

    public SalesOrderController(SalesOrderService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderDto dto)
    {

        if (!User.HasClaim("permission", "CREATE:SALES_ORDERS")) return Forbid();

        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? receiptNumber, [FromQuery] DateTime? orderDate, [FromQuery] string? status)
    {

        if (!User.HasClaim("permission", "VIEW:SALES_ORDERS")) return Forbid();

        var result = await _service.GetAllAsync(receiptNumber, orderDate, status);

        if (result == null) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {

        if (!User.HasClaim("permission", "VIEW:SALES_ORDERS")) return Forbid();

        var result = await _service.GetByIdAsync(id);

        if (result == null) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SalesOrderDto dto)
    {

        if (!User.HasClaim("permission", "EDIT:SALES_ORDERS")) return Forbid();

        var result = await _service.UpdateAsync(id, dto);

        if (!result) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

        if (!User.HasClaim("permission", "DELETE:SALES_ORDERS")) return Forbid();

        var result = await _service.DeleteAsync(id);

        if (!result) return NotFound();

        return Ok(result);
    }
}