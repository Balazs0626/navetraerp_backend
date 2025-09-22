namespace NavetraERP.DTOs;

public class RegisterRequest
{
    public string Username { get; set; } = String.Empty;
    public string Password { get; set; } = String.Empty;
    public int RoleId { get; set; }
    public string? Email { get; set; } = String.Empty;
}

public class LoginRequest
{
    public string Username { get; set; } = String.Empty;
    public string Password { get; set; } = String.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = String.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Username { get; set; } = String.Empty;
    public string Role { get; set; } = String.Empty;
    public List<string> Permissions { get; set; } = new();
}