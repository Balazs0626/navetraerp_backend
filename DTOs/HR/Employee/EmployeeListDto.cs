namespace NavetraERP.DTOs;

public class EmployeeListDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = String.Empty;
    public int PositionId { get; set; }
    public string PositionName { get; set; } = String.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = String.Empty;
    public bool HasUser { get; set; }
}