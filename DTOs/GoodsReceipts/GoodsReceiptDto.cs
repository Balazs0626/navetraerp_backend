namespace NavetraERP.DTOs;

public class GoodsReceiptDto
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public string WarehouseName { get; set; } = String.Empty;
    public string WarehouseAddress_1 { get; set; } = String.Empty;
    public string WarehouseAddress_2 { get; set; } = String.Empty;
    public DateTime ReceiptDate { get; set; }
    public string ReceivedByName { get; set; } = String.Empty;
    public List<GoodsReceiptItemDto> Items { get; set; } = new List<GoodsReceiptItemDto>();
}