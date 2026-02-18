namespace NavetraERP.DTOs;

public class CreateEmployeeDto
{
    public string FirstName { get; set; } = String.Empty;
    public string LastName { get; set; } = String.Empty;
    public DateTime BirthDate { get; set; }
    public string IdNumber { get; set; } = String.Empty;
    public string ResidenceNumber { get; set; } = String.Empty;
    public string HealthInsuranceNumber { get; set; } = String.Empty;
    public string TaxIdNumber { get; set; } = String.Empty;
    public string BankAccountNumber { get; set; } = String.Empty;
    public DateTime HireDate { get; set; }
    public int DepartmentId { get; set; }
    public int PositionId { get; set; }
    public int? UserId { get; set; }
    public string Email { get; set; } = String.Empty;
    public string PhoneNumber { get; set; } = String.Empty;
    public decimal Salary { get; set; }
    public string Status { get; set; } = String.Empty;
    public string AddressCountry { get; set; } = String.Empty;
    public string AddressRegion { get; set; } = String.Empty;
    public string AddressPostCode { get; set; } = String.Empty;
    public string AddressCity { get; set; } = String.Empty;
    public string AddressFirstLine { get; set; } = String.Empty;
    public string AddressSecondLine { get; set; } = String.Empty;
    public string? TempAddressCountry { get; set; } = String.Empty;
    public string? TempAddressRegion { get; set; } = String.Empty;
    public string? TempAddressPostCode { get; set; } = String.Empty;
    public string? TempAddressCity { get; set; } = String.Empty;
    public string? TempAddressFirstLine { get; set; } = String.Empty;
    public string? TempAddressSecondLine { get; set; } = String.Empty;
}