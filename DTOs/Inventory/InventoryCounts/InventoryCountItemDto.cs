namespace NavetraERP.DTOs;

public class InventoryCountItemDto
{
    public int Id { get; set; }
    public int CountId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = String.Empty;
    public string ProductUnit { get; set; } = String.Empty;
    public decimal CountedQuantity { get; set; }
    public decimal SystemQuantity { get; set; }
}