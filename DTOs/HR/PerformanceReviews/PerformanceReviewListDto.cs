namespace NavetraERP.DTOs;

public class PerformanceReviewListDto
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = String.Empty;
    public DateTime ReviewDate { get; set; }
    public int Score { get; set; }
}