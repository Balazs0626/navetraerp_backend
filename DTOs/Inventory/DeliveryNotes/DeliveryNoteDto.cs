namespace NavetraERP.DTOs;

public class DeliveryNoteDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = String.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = String.Empty;
    public string CustomerTaxNumber { get; set; } = String.Empty;
    public string CustomerEuTaxNumber { get; set; } = String.Empty;
    public string CustomerAddress_1 { get; set; } = String.Empty;
    public string CustomerAddress_2 { get; set; } = String.Empty;
    public string ShipperName { get; set; } = String.Empty;
    public string ShipperTaxNumber { get; set; } = String.Empty;
    public string ShipperEuTaxNumber { get; set; } = String.Empty;
    public string ShipperAddress_1 { get; set; } = String.Empty;
    public string ShipperAddress_2 { get; set; } = String.Empty;
    public string LicensePlate { get; set; } = String.Empty;
    public string Status { get; set; } = String.Empty;
    public DateTime CreateDate { get; set; }
    public DateTime ShippingDate { get; set; }
    public List<DeliveryNoteItemDto> Items { get; set; } = new List<DeliveryNoteItemDto>();
}