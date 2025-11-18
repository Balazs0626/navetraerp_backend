namespace NavetraERP.DTOs;

public class PurchaseOrderDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set;} = String.Empty;
    public string SupplierTaxNumber { get; set; } = String.Empty;
    public string SupplierAddressCountry { get; set; } = String.Empty;
    public string SupplierAddressRegion { get; set; } = String.Empty;
    public string SupplierAddressPostCode { get; set; } = String.Empty;
    public string SupplierAddressCity { get; set; } = String.Empty;
    public string SupplierAddressFirstLine { get; set; } = String.Empty;
    public string? SupplierAddressSecondLine { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public string Status { get; set; } = String.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = String.Empty;
    public List<PurchaseOrderItemDto> Items { get; set; } = new List<PurchaseOrderItemDto>();
}