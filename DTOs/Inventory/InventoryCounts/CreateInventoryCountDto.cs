namespace NavetraERP.DTOs;

public class CreateInventoryCountDto
{
    public int WarehouseId { get; set; }
    public DateTime CountDate { get; set; }
    public int CountedById { get; set; }
    public List<InventoryCountItemDto> Items { get; set; } = new List<InventoryCountItemDto>();
}