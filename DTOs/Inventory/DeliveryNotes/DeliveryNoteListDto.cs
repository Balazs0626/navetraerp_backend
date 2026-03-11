namespace NavetraERP.DTOs;

public class DeliveryNoteListDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = String.Empty;
    public string Status { get; set; } = String.Empty;
    public DateTime CreateDate { get; set; }
    public DateTime ShippingDate { get; set; }
}