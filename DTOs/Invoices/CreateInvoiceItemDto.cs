namespace NavetraERP.DTOs;

public class CreateInvoiceItemDto
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal TaxRate { get; set; }
}