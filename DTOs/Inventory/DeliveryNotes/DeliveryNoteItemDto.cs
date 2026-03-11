namespace NavetraERP.DTOs;

public class DeliveryNoteItemDto
{
    public int DeliveryNoteId { get; set; }
    public int ProductId { get; set; }
    public string ProductSku { get; set; } = String.Empty;
    public string ProductName { get; set; } = String.Empty;
    public string ProductUnit { get; set; } = String.Empty;
    public decimal Quantity { get; set; }
}