using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Models;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/company_data")]
public class CompanyDataController : ControllerBase
{

    private readonly CompanyDataService _service;

    public CompanyDataController(CompanyDataService service)
    {
        _service = service;
    }

    //[Authorize]
    [HttpGet]
    public async Task<IActionResult> GetById()
    {

/*         if (!User.HasClaim("permission", "VIEW:PRODUCTS"))
            return Forbid(); */

        var result = await _service.GetByIdAsync();

        if (result == null)
            NotFound();

        return Ok(result);
    }

    //[Authorize]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] CompanyData model)
    {

/*         if (!User.HasClaim("permission", "EDIT:PRODUCTS"))
            return Forbid(); */

        var result = await _service.UpdateAsync(model);

        if (!result)
            NotFound();

        return Ok(result);
    }

}