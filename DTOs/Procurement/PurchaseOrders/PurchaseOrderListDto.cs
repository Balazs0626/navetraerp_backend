namespace NavetraERP.DTOs;

public class PurchaseOrderListDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = String.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; } 
    public string Status { get; set; } = String.Empty;
}