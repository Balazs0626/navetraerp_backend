using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoiceController : ControllerBase
{

    private readonly InvoiceService _service;

    public InvoiceController(InvoiceService service)
    {
        _service = service;
    }

    //[Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceDto dto)
    {

        /* if (!User.HasClaim("permission", "CREATE:PURCHASE_ORDERS"))
            return Forbid(); */

        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    //[Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? receiptNumber, [FromQuery] DateTime? invoiceDate, [FromQuery] string? status)
    {

        /* if (!User.HasClaim("permission", "VIEW:PURCHASE_ORDERS"))
            return Forbid(); */

        var result = await _service.GetAllAsync(receiptNumber, invoiceDate, status);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    //[Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {

        /* if (!User.HasClaim("permission", "VIEW:PURCHASE_ORDERS"))
            return Forbid(); */

        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    //[Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] InvoiceDto dto)
    {

        /* if (!User.HasClaim("permission", "DELETE:PURCHASE_ORDERS"))
            return Forbid(); */

        var result = await _service.UpdateAsync(id, dto);

        if (!result)
            return NotFound();

        return Ok(result);
    }

    //[Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

        /* if (!User.HasClaim("permission", "DELETE:PURCHASE_ORDERS"))
            return Forbid(); */

        var result = await _service.DeleteAsync(id);

        if (!result)
            return NotFound();

        return Ok(result);
    }
}