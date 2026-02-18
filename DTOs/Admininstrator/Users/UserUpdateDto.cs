namespace NavetraERP.DTOs;

public class UserUpdateDto
{
    public string Username { get; set; } = String.Empty;
    public string PasswordHash { get; set; } = String.Empty;
    public string Email { get; set; } = String.Empty;
    public int RoleId { get; set; }
}