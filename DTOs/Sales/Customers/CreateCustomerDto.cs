namespace NavetraERP.DTOs;

public class CreateCustomerDto
{
    public string Name { get; set; } = String.Empty;
    public string TaxNumber { get; set; } = String.Empty;
    public string EuTaxNumber { get; set; } = String.Empty;
    public string BankAccountNumber { get; set; } = String.Empty;
    public string Email { get; set; } = String.Empty;
    public string PhoneNumber { get; set; } = String.Empty;
    public string BillingAddressCountry { get; set; } = String.Empty;
    public string BillingAddressRegion { get; set; } = String.Empty;
    public string BillingAddressPostCode { get; set; } = String.Empty;
    public string BillingAddressCity { get; set; } = String.Empty;
    public string BillingAddressFirstLine { get; set; } = String.Empty;
    public string? BillingAddressSecondLine { get; set; } = String.Empty;
    public string? ShippingAddressCountry { get; set; } = String.Empty;
    public string? ShippingAddressRegion { get; set; } = String.Empty;
    public string? ShippingAddressPostCode { get; set; } = String.Empty;
    public string? ShippingAddressCity { get; set; } = String.Empty;
    public string? ShippingAddressFirstLine { get; set; } = String.Empty;
    public string? ShippingAddressSecondLine { get; set; } = String.Empty;
}