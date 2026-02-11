using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/stock_movements")]
public class StockMovementController : ControllerBase
{

    private readonly StockMovementService _service;

    public StockMovementController(StockMovementService service)
    {
        _service = service;
    }

    //[Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockMovementDto dto)
    {

/*         if (!User.HasClaim("permission", "CREATE:STOCK_MOVEMENTS"))
            return Forbid(); */

        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    //[Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? productId, [FromQuery] string? referenceDocument, [FromQuery] string? movementType, [FromQuery] DateTime? movementDate, [FromQuery] DateTime? movementDateGte)
    {

/*         if (!User.HasClaim("permission", "VIEW:STOCK_MOVEMENTS"))
            return Forbid(); */

        var result = await _service.GetAllAsync(productId, referenceDocument, movementType, movementDate, movementDateGte);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    //[Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {

/*         if (!User.HasClaim("permission", "VIEW:STOCK_MOVEMENTS"))
            return Forbid(); */

        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    //[Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] StockMovementDto dto)
    {

/*         if (!User.HasClaim("permission", "VIEW:STOCK_MOVEMENTS"))
            return Forbid(); */

        var result = await _service.UpdateAsync(id, dto);

        if (!result)
            NotFound();

        return Ok(result);
    }

    //[Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

/*         if (!User.HasClaim("permission", "DELETE:STOCK_MOVEMENTS"))
            return Forbid(); */

        var result = await _service.DeleteAsync(id);

        if (!result)
            NotFound();

        return Ok(result);
    }
}