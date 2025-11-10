namespace NavetraERP.DTOs;

public class CreateWarehouseDto
{
    public string Name { get; set; } = String.Empty;
    public int ManagerId { get; set; }
    public string AddressCountry { get; set; } = String.Empty;
    public string AddressRegion { get; set; } = String.Empty;
    public string AddressPostCode { get; set; } = String.Empty;
    public string AddressCity { get; set; } = String.Empty;
    public string AddressFirstLine { get; set; } = String.Empty;
    public string? AddressSecondLine { get; set; } = String.Empty;
}