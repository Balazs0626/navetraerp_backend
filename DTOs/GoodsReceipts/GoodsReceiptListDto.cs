namespace NavetraERP.DTOs;

public class GoodsReceiptListDto
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public string WarehouseName { get; set; } = String.Empty;
    public DateTime ReceiptDate { get; set; }
}