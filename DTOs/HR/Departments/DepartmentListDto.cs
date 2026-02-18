namespace NavetraERP.DTOs;

public class DepartmentListDto
{
    public int Id { get; set; }
    public string DepartmentName { get; set; } = String.Empty;
    public string? Description { get; set; } = String.Empty;
}