namespace NavetraERP.DTOs;

public class BomComponentDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ComponentProductId { get; set; }
    public string ComponentProductSku { get; set; } = String.Empty;
    public string ComponentProductName { get; set; } = String.Empty;
    public string ComponentProductUnit { get; set; } = String.Empty;
    public decimal ComponentProductPricePerUnit { get; set; }
    public decimal ComponentProductAllPrice { get; set; }
    public decimal QuantityPerUnit { get; set; }
}