namespace NavetraERP.DTOs;

public class CreateStockMovementDto
{
    public int ProductId { get; set; }
    public int? FromWarehouseId { get; set; } = null;
    public int? ToWarehouseID { get; set; } = null;
    public string MovementType { get; set; } = String.Empty;
    public decimal Quantity { get; set; }
    public string? ReferenceDocument { get; set; } = null;
    public DateTime MovementDate { get; set; }
    public int? PerformedById { get; set; } = null;
}