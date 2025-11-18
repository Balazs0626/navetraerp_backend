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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto)
    {
        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

     [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();

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
            NotFound();

        return Ok(result);
    }
}