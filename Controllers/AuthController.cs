using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavetraERP.DTOs;
using NavetraERP.Models;
using NavetraERP.Services;
using System.Security.Claims;

namespace NavetraERP.Controllers;

public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _users;
    private readonly RoleService _roles;
    private readonly JwtService _jwt;
    private readonly IEmailService _email;

    public AuthController(UserService users, RoleService roles, JwtService jwt, IEmailService email)
    {
        _users = users;
        _roles = roles;
        _jwt = jwt;
        _email = email;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);
        var user = await _users.CreateAsync(req.Username, hash, req.RoleId, req.Email);
        if (user == null) return BadRequest("Nem sikerült létrehozni.");

        var role = await _roles.GetRoleByIdAsync(req.RoleId) ?? new Role { Id = 0, RoleName = "Unknown" };
        var perms = await _roles.GetPermissionsForRoleAsync(req.RoleId);

        return Ok(_jwt.CreateToken(user, role, perms));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var user = await _users.GetByUsernameAsync(req.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Hibás felhasználónév vagy jelszó.");

        var role = await _roles.GetRoleByIdAsync(user.RoleId) ?? new Role { Id = 0, RoleName = "Unknown" };
        var perms = await _roles.GetPermissionsForRoleAsync(user.RoleId);

        var active = await _users.SetUserActiveAsync(user.Id, true);

        return Ok(_jwt.CreateToken(user, role, perms));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserRole(int id, int roleId)
    {
        var result = await _users.UpdateUserRoleAsync(id, roleId);

        if (!result)
            NotFound("Hiba");

        return Ok(result);
    }

    [HttpPut("active")]
    [Authorize]
    public async Task<IActionResult> SetUserActive(int id, bool active)
    {
        var result = await _users.SetUserActiveAsync(id, active);

        if (!result)
            NotFound("Hiba");

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> Me()
    {
        var username = User.Identity?.Name ?? "";
        var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "";
        var perms = User.Claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
        var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
        return Ok(new { id, username, role, permissions = perms });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        var user = await _users.GetByEmailAsync(req.Email);
        
        // Biztonság: Ha nincs user, akkor is OK-t adunk, hogy ne lehessen e-mail címeket halászni
        if (user == null) return Ok(new { message = "Ha a cím létezik, a kiküldtük a tájékoztatót." });

        var token = Guid.NewGuid().ToString();
        // 15 perces lejárati idő
        await _users.SaveResetTokenAsync(user.Id, token, DateTime.UtcNow.AddMinutes(15));

        var subject = "Jelszó visszaállítás - NavetraERP";
        var message = $@"
            <h2>Jelszó visszaállítás</h2>
            <p>Kérted a jelszavad visszaállítását. A lenti kód 15 percig érvényes:</p>
            <p><strong>Token:</strong> {token}</p>
            <p>Vagy kattints ide: <a href='http://localhost:5173/reset_password?token={token}&email={user.Email}'>Visszaállítás</a></p>";

        await _email.SendEmailAsync(user.Email, subject, message);

        return Ok(new { message = "E-mail elküldve." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        var newHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        var success = await _users.ResetPasswordAsync(req.Email, req.Token, newHash);

        if (!success)
            return BadRequest("Érvénytelen token vagy lejárt időkeret.");

        return Ok(new { message = "A jelszó sikeresen frissítve!" });
    }
}
