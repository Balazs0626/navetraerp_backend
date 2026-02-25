namespace NavetraERP.DTOs;

public class PurchaseOrderDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = String.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set;} = String.Empty;
    public string SupplierTaxNumber { get; set; } = String.Empty;
    public string SupplierEuTaxNumber { get; set; } = String.Empty;
    public string SupplierBankAccountNumber { get; set; } = String.Empty;
    public string SupplierAddressCountry { get; set; } = String.Empty;
    public string SupplierAddressRegion { get; set; } = String.Empty;
    public string SupplierAddressPostCode { get; set; } = String.Empty;
    public string SupplierAddressCity { get; set; } = String.Empty;
    public string SupplierAddressFirstLine { get; set; } = String.Empty;
    public string? SupplierAddressSecondLine { get; set; } = null;
    public string CompanyName { get; set; } = String.Empty;
    public string CompanyTaxNumber { get; set; } = String.Empty;
    public string CompanyEuTaxNumber { get; set; } = String.Empty;
    public string CompanyBankAccountNumber { get; set; } = String.Empty;
    public string CompanyAddress_1 { get; set; } = String.Empty;
    public string? CompanyAddress_2 { get; set; } = null;
    public DateTime OrderDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public string Status { get; set; } = String.Empty;
    public decimal TotalAmount { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new List<PurchaseOrderItemDto>();
}