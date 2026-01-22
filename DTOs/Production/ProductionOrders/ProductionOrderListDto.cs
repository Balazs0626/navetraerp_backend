namespace NavetraERP.DTOs;

public class ProductionOrderListDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = String.Empty;
    public string ProductName { get; set; } = String.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = String.Empty;
}