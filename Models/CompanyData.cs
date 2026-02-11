namespace NavetraERP.Models;

public class CompanyData
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string TaxNumber { get; set; } = String.Empty;
    public string EuTaxNumber { get; set; } = String.Empty;
    public string BankAccountNumber { get; set; } = String.Empty;
    public string RegistrationNumber { get; set; } = String.Empty;
    public string? Email { get; set; } = null;
    public string? PhoneNumber { get; set; } = null;
    public string BillingCountry { get; set; } = String.Empty;
    public string BillingRegion { get; set; } = String.Empty;
    public string BillingPostCode { get; set; } = String.Empty;
    public string BillingCity { get; set; } = String.Empty;
    public string BillingAddress1 { get; set; } = String.Empty;
    public string? BillingAddress2 { get; set; } = null;
    public string? ShippingCountry { get; set; } = null;
    public string? ShippingRegion { get; set; } = null;
    public string? ShippingPostCode { get; set; } = null;
    public string? ShippingCity { get; set; } = null;
    public string? ShippingAddress1 { get; set; } = null;
    public string? ShippingAddress2 { get; set; } = null;
}