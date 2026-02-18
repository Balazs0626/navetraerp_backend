namespace NavetraERP.DTOs;

public class RolePermissionDto
{
    public int PermissionId { get; set; }
    public int ModuleId { get; set; }
    public string PermissionName { get; set; } = String.Empty;
}