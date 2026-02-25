using Microsoft.AspNetCore.SignalR;

namespace NavetraERP.DTOs;

public class PurchaseOrderItemDto
{
    public int PurchaseOrderId { get; set; }
    public int ProductId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal NettoPrice { get; set; }
    public decimal BruttoPrice { get; set; }
    public string ProductSku { get; set; } = String.Empty;
    public string ProductName { get; set; } = String.Empty;
    public string ProductUnit { get; set; } = String.Empty;
}