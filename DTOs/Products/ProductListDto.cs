namespace NavetraERP.DTOs;

public class ProductListDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
    public string Unit { get; set; } = String.Empty;
    public bool Active { get; set; }
    public int ComponentCount { get; set; }
}