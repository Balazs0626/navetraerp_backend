namespace NavetraERP.DTOs;

public class InvoiceListDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set;} = String.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; } 
    public string Status { get; set; } = String.Empty;
}