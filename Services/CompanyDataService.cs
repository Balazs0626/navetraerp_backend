using Dapper;
using System.Data.SqlClient;
using NavetraERP.Models;

namespace NavetraERP.Services;

public class CompanyDataService
{

    private readonly IConfiguration _config;

    public CompanyDataService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<CompanyData> GetByIdAsync()
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        int id = 1;

        try
        {
            const string query = @"
                SELECT
                    id AS Id, 
                    name AS Name, 
                    tax_number AS TaxNumber, 
                    eu_tax_number AS EuTaxNumber, 
                    bank_account_number AS BankAccountNumber, 
                    registration_number AS RegistrationNumber, 
                    email AS Email, 
                    phone_number AS PhoneNumber, 
                    billing_country AS BillingCountry, 
                    billing_region AS BillingRegion, 
                    billing_post_code AS BillingPostCode, 
                    billing_city AS BillingCity, 
                    billing_address_1 AS BillingAddress1, 
                    billing_address_2 AS BillingAddress2, 
                    shipping_country AS ShippingCountry, 
                    shipping_region AS ShippingRegion, 
                    shipping_post_code AS ShippingPostCode, 
                    shipping_city AS ShippingCity, 
                    shipping_address_1 AS ShippingAddress1, 
                    shipping_address_2 AS ShippingAddress2
                FROM CompanyData 
                WHERE id = @Id";

            var result = await connection.QueryFirstOrDefaultAsync<CompanyData>(query, new
            {
                id
            }, transaction);

            transaction.Commit();

            return result;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(CompanyData model)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        int id = 1;

        var rowsAffected = 0;

        try
        {
            const string update = @"
                UPDATE CompanyData 
                SET 
                    name = @Name, 
                    tax_number = @TaxNumber, 
                    eu_tax_number = @EuTaxNumber, 
                    bank_account_number = @BankAccountNumber, 
                    registration_number = @RegistrationNumber, 
                    email = @Email, 
                    phone_number = @PhoneNumber, 
                    billing_country = @BillingCountry, 
                    billing_region = @BillingRegion, 
                    billing_post_code = @BillingPostCode, 
                    billing_city = @BillingCity, 
                    billing_address_1 = @BillingAddress1, 
                    billing_address_2 = @BillingAddress2, 
                    shipping_country = @ShippingCountry, 
                    shipping_region = @ShippingRegion, 
                    shipping_post_code = @ShippingPostCode, 
                    shipping_city = @ShippingCity, 
                    shipping_address_1 = @ShippingAddress1, 
                    shipping_address_2 = @ShippingAddress2
                WHERE id = @Id";

            var parameters = new DynamicParameters(model);
            parameters.Add("@id", id);

            rowsAffected += await connection.ExecuteAsync(update, parameters, transaction);

            transaction.Commit();

            return rowsAffected > 0;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
}