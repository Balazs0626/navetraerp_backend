namespace NavetraERP.DTOs;

public class ProductionOutputListDto
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public string ProductionOrderReceiptNumber { get; set; } = String.Empty;
    public string ProductName { get; set; } = String.Empty;
    public string ProductUnit { get; set; } = String.Empty;
    public decimal QuantityProduced { get; set; }
    public string WarehouseName { get; set; } = String.Empty;
    public DateTime DateProduced { get; set; }
}