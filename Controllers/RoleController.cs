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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoleDto dto)
    {
        var result = await _roleService.CreateAsync(dto);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var results = await _roleService.GetAllAsync();

        if (results == null) NotFound();

        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _roleService.GetByIdAsync(id);

        if (result == null) NotFound();

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, RoleDto dto)
    {
        var result = await _roleService.UpdateAsync(id, dto);

        if (!result) NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _roleService.DeleteAsync(id);

        if (!result) NotFound();

        return Ok(result);
    }

    [HttpGet("all-permissions")]
    public async Task<IActionResult> GetAllPermissions()
    {
        var results = await _permissionService.GetAllAsync();

        if (results == null) NotFound();

        return Ok(results);
    }
}