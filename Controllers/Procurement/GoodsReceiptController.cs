using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/goods_receipts")]
public class GoodsReceiptController : ControllerBase
{

    private readonly GoodsReceiptService _service;

    public GoodsReceiptController(GoodsReceiptService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGoodsReceiptDto dto)
    {

        if (!User.HasClaim("permission", "CREATE:GOODS_RECEIPTS"))
            return Forbid();

        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? purchaseOrderId, [FromQuery] int? warehouseId, [FromQuery] DateTime? date)
    {

        if (!User.HasClaim("permission", "VIEW:GOODS_RECEIPTS"))
            return Forbid();

        var result = await _service.GetAllAsync(purchaseOrderId, warehouseId, date);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {

        if (!User.HasClaim("permission", "VIEW:GOODS_RECEIPTS"))
            return Forbid();

        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

        if (!User.HasClaim("permission", "DELETE:GOODS_RECEIPTS"))
            return Forbid();

        var result = await _service.DeleteAsync(id);

        if (!result)
            NotFound();

        return Ok(result);
    }
}