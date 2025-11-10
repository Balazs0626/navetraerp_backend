namespace NavetraERP.DTOs;

public class LeaveRequestListDto
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = String.Empty;
    public string LeaveType { get; set; } = String.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = String.Empty;
}