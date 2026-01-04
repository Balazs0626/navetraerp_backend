namespace NavetraERP.DTOs;

public class CreateInvoiceDto
{
    public int Id { get; set; }
    public int SalesOrderId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set;} = String.Empty;
    public List<CreateInvoiceItemDto> Items { get; set; } = new List<CreateInvoiceItemDto>(); 
}