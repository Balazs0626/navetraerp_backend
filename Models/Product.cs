namespace NavetraERP.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Sku { get; set; } = String.Empty;
    public string? Description { get; set; } = String.Empty;
    public string Unit { get; set; } = String.Empty;
    public decimal? PricePerUnit { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
}