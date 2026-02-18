namespace NavetraERP.DTOs;

public class GoodsReceiptItemDto
{
    public int GoodsReceiptId { get; set; }
    public int ProductId { get; set; }
    public string ProductSku { get; set; } = String.Empty;
    public string ProductName { get; set; } = String.Empty;
    public string ProductUnit { get; set; } = String.Empty;
    public decimal QuantityReceived { get; set; }
    public string BatchNumber { get; set; } = String.Empty;
}