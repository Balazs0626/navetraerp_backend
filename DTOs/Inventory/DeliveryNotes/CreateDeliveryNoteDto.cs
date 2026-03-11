namespace NavetraERP.DTOs;

public class CreateDeliveryNoteDto
{
    public int CustomerId { get; set; }
    public string LicensePlate { get; set; } = String.Empty;
    public string Status { get; set; } = String.Empty;
    public DateTime CreateDate { get; set; }
    public DateTime ShippingDate { get; set; }
    public List<DeliveryNoteItemDto> Items { get; set; } = new List<DeliveryNoteItemDto>();
}