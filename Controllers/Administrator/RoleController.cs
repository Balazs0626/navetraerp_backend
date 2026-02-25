using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Models;
using NavetraERP.Services;
using System.Security.Claims;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/roles")]
public class RoleController : ControllerBase
{

    RoleService _roleService;
    PermissionService _permissionService;

    public RoleController(RoleService roleService, PermissionService permissionService)
    {
        _roleService = roleService;
        _permissionService = permissionService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoleDto dto)
    {

        if (!User.HasClaim("permission", "EDIT:ROLES")) return Forbid();

        var result = await _roleService.CreateAsync(dto);

        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {

        if (!User.HasClaim("permission", "VIEW:ROLES")) return Forbid();

        var results = await _roleService.GetAllAsync();

        if (results == null) return NotFound();

        return Ok(results);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {

        if (!User.HasClaim("permission", "VIEW:ROLES")) return Forbid();

        var result = await _roleService.GetByIdAsync(id);

        if (result == null) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, RoleDto dto)
    {

        if (!User.HasClaim("permission", "EDIT:ROLES")) return Forbid();

        var result = await _roleService.UpdateAsync(id, dto);

        if (!result) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

        if (!User.HasClaim("permission", "DELETE:ROLES")) return Forbid();

        var result = await _roleService.DeleteAsync(id);

        if (!result) return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpGet("all-permissions")]
    public async Task<IActionResult> GetAllPermissions()
    {

        if (!User.HasClaim("permission", "EDIT:ROLES")) return Forbid();

        var results = await _permissionService.GetAllAsync();

        if (results == null) return NotFound();

        return Ok(results);
    }
}