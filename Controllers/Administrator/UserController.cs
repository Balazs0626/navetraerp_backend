using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    UserService _service;

    public UserController(UserService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(RegisterRequest req)
    {

        if (!User.HasClaim("permission", "CREATE:USERS")) return Forbid();

        var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);

        var results = await _service.CreateAsync(req.Username, hash, req.RoleId, req.Email);

        if (results == null) return NotFound();

        return Ok(results);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {

        if (!User.HasClaim("permission", "VIEW:USERS")) return Forbid();

        var results = await _service.GetAllAsync();

        if (results == null) return NotFound();

        return Ok(results);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {

        if (!User.HasClaim("permission", "VIEW:USERS")) return Forbid();

        var result = await _service.GetByIdAsync(id);

        if (result == null) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UserUpdateDto dto)
    {

        if (!User.HasClaim("permission", "EDIT:USERS")) return Forbid();

        var hash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash);

        if (!String.IsNullOrWhiteSpace(dto.PasswordHash)) dto.PasswordHash = hash;

        var result = await _service.UpdateAsync(id, dto);

        if (!result) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

        if (!User.HasClaim("permission", "DELETE:USERS")) return Forbid();

        var result = await _service.DeleteAsync(id);

        if (!result) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpGet("active_users")]
    public async Task<IActionResult> GetActiveUserCount()
    {
        var results = await _service.GetActiveUserCountAsync();

        return Ok(results);
    }
}