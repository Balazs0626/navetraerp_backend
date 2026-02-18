namespace NavetraERP.DTOs;

public class ShiftDto
{
    public string ShiftName { get; set; } = String.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}