using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Models;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{

    private readonly ProductService _service;

    public ProductController(ProductService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {

        if (!User.HasClaim("permission", "CREATE:PRODUCTS"))
            return Forbid();

        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? sku, [FromQuery] string? name, [FromQuery] bool? active)
    {

        if (!User.HasClaim("permission", "VIEW:PRODUCTS"))
            return Forbid();

        var result = await _service.GetAllAsync(sku, name, active);

        if (result == null)
            NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {

        if (!User.HasClaim("permission", "VIEW:PRODUCTS"))
            return Forbid();

        var result = await _service.GetByIdAsync(id);

        if (result == null)
            NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductDto dto)
    {

        if (!User.HasClaim("permission", "EDIT:PRODUCTS"))
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

        if (!User.HasClaim("permission", "DELETE:PRODUCTS"))
            return Forbid();

        var result = await _service.DeleteAsync(id);

        if (!result)
            NotFound();

        return Ok(result);
    }

}