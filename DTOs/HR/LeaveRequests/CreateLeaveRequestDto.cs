namespace NavetraERP.DTOs;

public class CreateLeaveRequestDto
{
    public int EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string LeaveType { get; set; } = String.Empty;
    public string Status { get; set; } = String.Empty;
}