namespace NavetraERP.Models;

public class Shift
{
    public int Id { get; set; }
    public string ShiftName { get; set; } = String.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}