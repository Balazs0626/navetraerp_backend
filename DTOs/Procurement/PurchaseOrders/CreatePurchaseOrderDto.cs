namespace NavetraERP.DTOs;

public class CreatePurchaseOrderDto
{
    public int SupplierId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public string Status { get; set; } = String.Empty;
    public decimal TotalAmount { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new List<PurchaseOrderItemDto>();
}