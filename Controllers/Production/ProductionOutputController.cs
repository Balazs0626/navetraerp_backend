using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/production_outputs")]
public class ProductionOutputController : ControllerBase
{

    private readonly ProductionOutputService _service;

    public ProductionOutputController(ProductionOutputService service)
    {
        _service = service;
    }

    //[Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductionOutputDto dto)
    {

/*         if (!User.HasClaim("permission", "CREATE:PRODUCTION_OUTPUT"))
            return Forbid(); */

        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    //[Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {

/*         if (!User.HasClaim("permission", "VIEW:PRODUCTION_OUTPUT"))
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

/*         if (!User.HasClaim("permission", "VIEW:PRODUCTION_OUTPUT"))
            return Forbid(); */

        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    //[Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductionOutputDto dto)
    {

/*         if (!User.HasClaim("permission", "UPDATE:PRODUCTION_OUTPUT"))
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

/*         if (!User.HasClaim("permission", "DELETE:PRODUCTION_OUTPUT"))
            return Forbid(); */

        var result = await _service.DeleteAsync(id);

        if (!result)
            NotFound();

        return Ok(result);
    }
}