namespace NavetraERP.DTOs;

public class PerformanceReviewDto
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = String.Empty;
    public DateTime ReviewDate { get; set; }
    public int Score { get; set; }
    public string Comment { get; set; } = String.Empty;
}