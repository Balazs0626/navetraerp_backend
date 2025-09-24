using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    UserService _service;

    public UserController(UserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var results = await _service.GetAllAsync();

        if (results == null) NotFound();

        return Ok(results);
    }

    [HttpPost]
    public async Task<IActionResult> Create(RegisterRequest req)
    {
        var results = await _service.CreateAsync(req.Username, req.Password, req.RoleId, req.Email);

        if (results == null) NotFound();

        return Ok(results);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result) NotFound();

        return Ok(result);
    }
}