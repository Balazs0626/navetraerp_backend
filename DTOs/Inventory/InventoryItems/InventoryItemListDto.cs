namespace NavetraERP.DTOs;

public class InventoryItemListDto
{
    public int Id { get; set; }
    public string WarehouseName { get; set; } = String.Empty;
    public string ProductName { get; set; } = String.Empty;
    public string ProductUnit { get; set; } = String.Empty;
    public decimal QuantityOnHand { get; set; }
    public string BatchNumber { get; set; } = String.Empty;
    public DateTime LastUpdated { get; set; }
}