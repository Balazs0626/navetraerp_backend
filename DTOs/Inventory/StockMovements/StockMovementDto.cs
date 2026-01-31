namespace NavetraERP.DTOs;

public class StockMovementDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = String.Empty;
    public string ProductUnit { get; set; } = String.Empty;
    public int? FromWarehouseId { get; set; } = null;
    public string FromWarehouseName { get; set; } = String.Empty;
    public int? ToWarehouseId { get; set; } = null;
    public string ToWarehouseName { get; set; } = String.Empty;
    public string MovementType { get; set; } = String.Empty;
    public decimal Quantity { get; set; }
    public string? ReferenceDocument { get; set; } = null;
    public DateTime MovementDate { get; set; }
    public int? PerformedById { get; set; } = null;
    public string PerformedByName { get; set; } = String.Empty;
}