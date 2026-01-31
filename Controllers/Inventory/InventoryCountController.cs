using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/inventory_counts")]
public class InventoryCountController : ControllerBase
{

    private readonly InventoryCountService _service;

    public InventoryCountController(InventoryCountService service)
    {
        _service = service;
    }

    //[Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryCountDto dto)
    {

/*         if (!User.HasClaim("permission", "CREATE:INVENTORY_COUNTS"))
            return Forbid(); */

        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    //[Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {

/*         if (!User.HasClaim("permission", "VIEW:INVENTORY_COUNTS"))
            return Forbid(); */

        var result = await _service.GetAllAsync();

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    //[Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {

/*         if (!User.HasClaim("permission", "VIEW:GOODS_RECEIPTS"))
            return Forbid(); */

        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    //[Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

/*         if (!User.HasClaim("permission", "DELETE:INVENTORY_COUNTS"))
            return Forbid(); */

        var result = await _service.DeleteAsync(id);

        if (!result)
            NotFound();

        return Ok(result);
    }
}