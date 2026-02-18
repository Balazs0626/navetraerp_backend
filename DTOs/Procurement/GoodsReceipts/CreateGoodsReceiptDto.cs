namespace NavetraERP.DTOs;

public class CreateGoodsReceiptDto
{
    public int PurchaseOrderId { get; set; }
    public int WarehouseId { get; set; }
    public DateTime ReceiptDate { get; set; }
    public int ReceivedBy { get; set; }
    public List<GoodsReceiptItemDto> Items { get; set; } = new List<GoodsReceiptItemDto>();
}