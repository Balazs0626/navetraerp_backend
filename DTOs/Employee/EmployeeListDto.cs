namespace NavetraERP.DTOs;

public class EmployeeListDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = String.Empty;
    public string PositionName { get; set; } = String.Empty;
    public string DepartmentName { get; set; } = String.Empty;
    public bool HasUser { get; set; }
}