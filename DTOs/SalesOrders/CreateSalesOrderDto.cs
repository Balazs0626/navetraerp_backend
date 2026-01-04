namespace NavetraERP.DTOs;

public class CreateSalesOrderDto
{
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime RequiredDeliveryDate { get; set; }
    public string Status { get; set; } = String.Empty;
    public decimal TotalAmount { get; set; }
    public List<SalesOrderItemDto> Items { get; set; } = new List<SalesOrderItemDto>();
}