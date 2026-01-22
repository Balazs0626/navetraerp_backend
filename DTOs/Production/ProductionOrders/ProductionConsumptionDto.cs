namespace NavetraERP.DTOs;

public class ProductionConsumptionDto
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public int ComponentProductId { get; set; }
    public string ComponentProductName { get; set; } = String.Empty;
    public string ComponentProductUnit { get; set; } = String.Empty;
    public decimal QuantityUsed { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = String.Empty;
    public DateTime DateUsed { get; set; }
}