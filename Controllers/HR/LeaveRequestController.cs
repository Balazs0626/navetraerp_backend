using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Models;
using NavetraERP.Services;

namespace NavetraERP.Controllers;

[ApiController]
[Route("api/leave_requests")]
public class LeaveRequestController : ControllerBase
{

    private readonly LeaveRequestService _service;

    public LeaveRequestController(LeaveRequestService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto dto)
    {

        if (!User.HasClaim("permission", "CREATE:LEAVE_REQUESTS"))
            return Forbid();

        var result = await _service.CreateAsync(dto);

        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? employeeName, [FromQuery] string? leaveType, [FromQuery] string? status)
    {

        if (!User.HasClaim("permission", "VIEW:LEAVE_REQUESTS"))
            return Forbid();

        var result = await _service.GetAllAsync(employeeName, leaveType, status);

        if (result == null) 
            return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpPut("approve")]
    public async Task<IActionResult> Approve([FromBody] LeaveRequestDto dto)
    {

        if (!User.HasClaim("permission", "EDIT:LEAVE_REQUESTS"))
            return Forbid();

        var result = await _service.ApproveAsync(dto);

        if (!result) 
            return NotFound();

        return Ok(result);
    }

    [HttpPut("reject")]
    public async Task<IActionResult> Reject([FromBody] LeaveRequestDto dto)
    {

        if (!User.HasClaim("permission", "EDIT:LEAVE_REQUESTS"))
            return Forbid();

        var result = await _service.RejectAsync(dto);

        if (!result) 
            return NotFound();

        return Ok(result);
    }
}