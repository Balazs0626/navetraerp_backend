using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/inventory_items")]
public class InventoryItemController : ControllerBase
{

    private readonly InventoryItemService _service;

    public InventoryItemController(InventoryItemService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemDto dto)
    {

        if (!User.HasClaim("permission", "CREATE:INVENTORY_ITEMS")) return Forbid();

        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? warehouseId, [FromQuery] int? productId, [FromQuery] string? batchNumber)
    {

        if (!User.HasClaim("permission", "VIEW:INVENTORY_ITEMS")) return Forbid();

        var result = await _service.GetAllAsync(warehouseId, productId, batchNumber);

        if (result == null) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

        if (!User.HasClaim("permission", "DELETE:INVENTORY_ITEMS")) return Forbid();

        var result = await _service.DeleteAsync(id);

        if (!result) return NotFound();

        return Ok(result);
    }
}