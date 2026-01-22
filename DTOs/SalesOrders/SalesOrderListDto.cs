namespace NavetraERP.DTOs;

public class SalesOrderListDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = String.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime RequiredDeliveryDate { get; set; } 
    public string Status { get; set; } = String.Empty;
}