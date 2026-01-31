namespace NavetraERP.DTOs;

public class InventoryCountListDto
{
    public int Id { get; set; }
    public string CountedByName { get; set; } = String.Empty;
    public string WarehouseName { get; set; } = String.Empty;
    public DateTime CountDate { get; set; }
}