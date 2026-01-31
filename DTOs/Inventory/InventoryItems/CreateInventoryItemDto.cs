namespace NavetraERP.DTOs;

public class CreateInventoryItemDto
{
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public string BatchNumber { get; set; } = String.Empty;
}