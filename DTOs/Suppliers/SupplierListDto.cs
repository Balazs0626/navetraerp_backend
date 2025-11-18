namespace NavetraERP.DTOs;

public class SupplierListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string ContactPerson { get; set; } = String.Empty;
    public string Email { get; set; } = String.Empty;
    public string PhoneNumber { get; set;} = String.Empty;
}