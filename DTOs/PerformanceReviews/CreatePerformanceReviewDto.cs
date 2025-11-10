namespace NavetraERP.DTOs;

public class CreatePerformanceReviewDto
{
    public int EmployeeId { get; set; }
    public DateTime ReviewDate { get; set; }
    public int Score { get; set; }
    public string Comment { get; set; } = String.Empty;
}