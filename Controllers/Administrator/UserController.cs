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

    [HttpPost]
    public async Task<IActionResult> Create(RegisterRequest req)
    {

        var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);

        var results = await _service.CreateAsync(req.Username, hash, req.RoleId, req.Email);

        if (results == null) NotFound();

        return Ok(results);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var results = await _service.GetAllAsync();

        if (results == null) NotFound();

        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null) 
            return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

        var result = await _service.DeleteAsync(id);

        if (!result) 
            return NotFound();

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UserUpdateDto dto)
    {

        var hash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash);

        if (!String.IsNullOrWhiteSpace(dto.PasswordHash))
            dto.PasswordHash = hash;

        var result = await _service.UpdateAsync(id, dto);


        if (!result) 
            return NotFound();

        return Ok(result);
    }

    [HttpGet("active_users")]
    public async Task<IActionResult> GetActiveUserCount()
    {
        var results = await _service.GetActiveUserCountAsync();

        return Ok(results);
    }
}