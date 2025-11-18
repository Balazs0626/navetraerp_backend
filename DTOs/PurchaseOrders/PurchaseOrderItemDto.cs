namespace NavetraERP.DTOs;

public class PurchaseOrderItemDto
{
    public int PurchaseOrderId { get; set; }
    public int ProductId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal? PricePerUnit { get; set; }
    public decimal? Discount { get; set; }
    public decimal? TaxRate { get; set; }
}