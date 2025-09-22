namespace NavetraERP.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = String.Empty;
    public string PasswordHash { get; set; } = String.Empty;
    public string? Email { get; set; }
    public int RoleId { get; set; }
    public bool Active { get; set; }
}