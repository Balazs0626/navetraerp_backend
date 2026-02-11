namespace NavetraERP.DTOs;

public class GoodsReceiptListDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = String.Empty;
    public int PurchaseOrderId { get; set; }
    public string PurchaseOrderReceiptNumber { get; set; } = String.Empty;
    public string WarehouseName { get; set; } = String.Empty;
    public DateTime ReceiptDate { get; set; }
}