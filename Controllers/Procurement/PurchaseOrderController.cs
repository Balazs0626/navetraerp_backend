using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/purchase_orders")]
public class PurchaseOrderController : ControllerBase
{

    private readonly PurchaseOrderService _service;

    public PurchaseOrderController(PurchaseOrderService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto)
    {

        if (!User.HasClaim("permission", "CREATE:PURCHASE_ORDERS"))
            return Forbid();

        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? id, [FromQuery] DateTime? orderDate, [FromQuery] string? status)
    {

        if (!User.HasClaim("permission", "VIEW:PURCHASE_ORDERS"))
            return Forbid();

        var result = await _service.GetAllAsync(id, orderDate, status);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {

        if (!User.HasClaim("permission", "VIEW:PURCHASE_ORDERS"))
            return Forbid();

        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    //[Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PurchaseOrderDto dto)
    {

        if (!User.HasClaim("permission", "UPDATE:PURCHASE_ORDERS"))
            return Forbid();

        var result = await _service.UpdateAsync(id, dto);

        if (!result)
            NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

        if (!User.HasClaim("permission", "DELETE:PURCHASE_ORDERS"))
            return Forbid();

        var result = await _service.DeleteAsync(id);

        if (!result)
            NotFound();

        return Ok(result);
    }
}