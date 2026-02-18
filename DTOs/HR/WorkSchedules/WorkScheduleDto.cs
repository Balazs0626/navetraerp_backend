namespace NavetraERP.DTOs;

public class WorkScheduleDto
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = String.Empty;
    public string ShiftName { get; set; } = String.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}