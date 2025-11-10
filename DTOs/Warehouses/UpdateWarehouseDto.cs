namespace NavetraERP.DTOs;

public class UpdateWarehouseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public int ManagerId { get; set; }
    public int AddressId { get; set; }
    public string AddressCountry { get; set; } = String.Empty;
    public string AddressRegion { get; set; } = String.Empty;
    public string AddressPostCode { get; set; } = String.Empty;
    public string AddressCity { get; set; } = String.Empty;
    public string AddressFirstLine { get; set; } = String.Empty;
    public string AddressSecondLine { get; set; } = String.Empty;
}