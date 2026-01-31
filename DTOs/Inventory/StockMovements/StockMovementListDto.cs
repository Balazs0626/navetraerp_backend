namespace NavetraERP.DTOs;

public class StockMovementListDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = String.Empty;
    public string ReferenceDocument { get; set; } = String.Empty;
    public DateTime MovementDate { get; set; }
    public string MovementType { get; set; } = String.Empty;
}