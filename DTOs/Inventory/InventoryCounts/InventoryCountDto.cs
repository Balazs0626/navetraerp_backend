namespace NavetraERP.DTOs;

public class InventoryCountDto
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = String.Empty;
    public DateTime CountDate { get; set; }
    public int CountedById { get; set; }
    public string CountedByName { get; set; } = String.Empty;
    public List<InventoryCountItemDto> Items { get; set; } = new List<InventoryCountItemDto>();
}