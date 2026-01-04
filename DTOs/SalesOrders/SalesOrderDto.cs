namespace NavetraERP.DTOs;

public class SalesOrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set;} = String.Empty;
    public string CustomerTaxNumber { get; set; } = String.Empty;
    public string CustomerBillingAddress_1 { get; set; } = String.Empty;
    public string CustomerBillingAddress_2 { get; set; } = String.Empty;
    public string CustomerShippingAddress_1 { get; set; } = String.Empty;
    public string CustomerShippingAddress_2 { get; set; } = String.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime RequiredDeliveryDate { get; set; }
    public string Status { get; set; } = String.Empty;
    public decimal TotalAmount { get; set; }
    public List<SalesOrderItemDto> Items { get; set; } = new List<SalesOrderItemDto>();
}