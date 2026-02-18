namespace NavetraERP.DTOs;

public class InvoiceDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set;} = String.Empty;
    public int SalesOrderId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; } 
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string PaidAmountText { get; set; } = String.Empty;
    public decimal TotalTaxRate { get; set; }
    public decimal TotalTax { get; set; }
    public string Status { get; set; } = String.Empty;
    public string SellerName { get; set; } = String.Empty;
    public string SellerTaxNumber { get; set; } = String.Empty;
    public string SellerEuTaxNumber { get; set; } = String.Empty;
    public string SellerBankAccountNumber { get; set; } = String.Empty;
    public string SellerAddress_1 { get; set; } = String.Empty;
    public string SellerAddress_2 { get; set; } = String.Empty;
    public string CustomerName { get; set; } = String.Empty;
    public string CustomerTaxNumber { get; set; } = String.Empty;
    public string CustomerEuTaxNumber { get; set; } = String.Empty;
    public string CustomerBankAccountNumber { get; set; } = String.Empty;
    public string CustomerAddress_1 { get; set; } = String.Empty;
    public string CustomerAddress_2 { get; set; } = String.Empty;
    public List<InvoiceItemDto> Items { get; set; } = new List<InvoiceItemDto>(); 
}