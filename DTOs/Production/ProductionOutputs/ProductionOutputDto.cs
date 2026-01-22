namespace NavetraERP.DTOs;

public class ProductionOutputDto
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = String.Empty;
    public decimal QuantityProduced { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = String.Empty;
    public DateTime DateProduced { get; set; }
}