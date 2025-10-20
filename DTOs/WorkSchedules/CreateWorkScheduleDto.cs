namespace NavetraERP.DTOs;

public class CreateWorkScheduleDto
{
    public List<int> EmployeeIds { get; set; } = new List<int>();
    public int ShiftId { get; set; }
    public DateTime Date { get; set; }
}