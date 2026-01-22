namespace NavetraERP.DTOs;

public class CreateProductionOutputDto
{
    public int ProductionOrderId { get; set; }
    public int ProductId { get; set; }
    public decimal QuantityProduced { get; set; }
    public int WarehouseId { get; set; }
    public DateTime DateProduced { get; set; }
}