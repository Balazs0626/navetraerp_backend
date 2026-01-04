namespace NavetraERP.DTOs;

public class InvoiceListDto
{
    public int Id { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; } 
    public string Status { get; set; } = String.Empty;
}