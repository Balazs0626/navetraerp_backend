namespace NavetraERP.DTOs;

public class CreateProductionOutputDto
{
    public int ProductionOrderId { get; set; }
    public int ProductId { get; set; }
    public decimal QuantityProduced { get; set; }
    public string BatchNumber { get; set; } = String.Empty;
    public int WarehouseId { get; set; }
    public DateTime DateProduced { get; set; }
}