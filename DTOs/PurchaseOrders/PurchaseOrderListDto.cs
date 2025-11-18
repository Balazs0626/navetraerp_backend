namespace NavetraERP.DTOs;

public class PurchaseOrderListDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; } 
    public string Status { get; set; } = String.Empty;
}