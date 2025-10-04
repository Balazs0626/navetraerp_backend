using NavetraERP.Models;

namespace NavetraERP.DTOs;

public class RoleDto
{
    public int Id { get; set; }
    public string RoleName { get; set; } = String.Empty;
    public List<RolePermissionDto> Permissions { get; set; } = new List<RolePermissionDto>();
}