using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;

namespace NavetraERP.Services;

public class CustomerService
{

    private readonly IConfiguration _config;

    public CustomerService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateCustomerDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertBillingAddress = @"
                INSERT INTO Addresses(country, region, post_code, city, address_1, address_2)
                VALUES (@BillingAddressCountry, @BillingAddressRegion, @BillingAddressPostCode, @BillingAddressCity, @BillingAddressFirstLine, @BillingAddressSecondLine);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var billingAddressResult = await connection.ExecuteScalarAsync<int>(insertBillingAddress, dto, transaction);

            const string insertShippingAddress = @"
                INSERT INTO Addresses(country, region, post_code, city, address_1, address_2)
                VALUES (@ShippingAddressCountry, @ShippingAddressRegion, @ShippingAddressPostCode, @ShippingAddressCity, @ShippingAddressFirstLine, @ShippingAddressSecondLine);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var shippingAddressResult = await connection.ExecuteScalarAsync<int>(insertShippingAddress, dto, transaction);

            const string insertSupplier = @"
                INSERT INTO Customers (
                    name,
                    tax_number,
                    eu_tax_number,
                    bank_account_number,
                    email,
                    phone_number,
                    billing_address_id,
                    shipping_address_id
                )
                VALUES (
                    @Name,
                    @TaxNumber,
                    @EuTaxNumber,
                    @BankAccountNumber,
                    @Email,
                    @PhoneNumber,
                    @BillingAddressId,
                    @ShippingAddressId
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@BillingAddressId", billingAddressResult);
            parameters.Add("@ShippingAddressId", shippingAddressResult);

            var result = await connection.ExecuteScalarAsync<int>(insertSupplier, parameters, transaction);

            transaction.Commit();

            return result;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

    }

    public async Task<IEnumerable<CustomerListDto>> GetAllAsync()
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                id AS Id,
                name AS Name,
                email AS Email,
                phone_number AS PhoneNumber
            FROM Customers";

        var result = await connection.QueryAsync<CustomerListDto>(query);

        return result;
    }

    public async Task<UpdateCustomerDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                c.id AS Id,
                c.name AS Name,
                c.tax_number AS TaxNumber,
                c.eu_tax_number AS EuTaxNumber,
                c.bank_account_number AS BankAccountNumber,
                c.email AS Email,
                c.phone_number AS PhoneNumber,
                c.billing_address_id AS BillingAddressId,
                a.country AS BillingAddressCountry,
                a.region AS BillingAddressRegion,
                a.post_code AS BillingAddressPostCode,
                a.city AS BillingAddressCity,
                a.address_1 AS BillingAddressFirstLine,
                a.address_2 AS BillingAddressSecondLine,
                c.shipping_address_id AS ShippingAddressId,
                s.country AS ShippingAddressCountry,
                s.region AS ShippingAddressRegion,
                s.post_code AS ShippingAddressPostCode,
                s.city AS ShippingAddressCity,
                s.address_1 AS ShippingAddressFirstLine,
                s.address_2 AS ShippingAddressSecondLine
            FROM Customers c
            JOIN Addresses a ON a.id = c.billing_address_id
            LEFT JOIN Addresses s ON s.id = c.shipping_address_id
            WHERE c.id = @id";

        var result = await connection.QueryFirstOrDefaultAsync<UpdateCustomerDto>(query, new
        {
            id
        });

        return result;
    }

    public async Task<bool> UpdateAsync(int id, UpdateCustomerDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        var rowsAffected = 0;

        try
        {
            const string updateCustomer = @"
                UPDATE Customers
                SET
                    name = @Name,
                    tax_number = @TaxNumber,
                    eu_tax_number = @EuTaxNumber,
                    bank_account_number = @BankAccountNumber,
                    email = @Email,
                    phone_number = @PhoneNumber
                WHERE id = @id";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@id", id);

            rowsAffected = await connection.ExecuteAsync(updateCustomer, parameters, transaction);

            const string updateBillingAddress = @"
                UPDATE Addresses
                SET
                    country = @BillingAddressCountry,
                    region = @BillingAddressRegion,
                    post_code = @BillingAddressPostCode,
                    city = @BillingAddressCity,
                    address_1 = @BillingAddressFirstLine,
                    address_2 = @BillingAddressSecondLine
                WHERE id = @BillingAddressId";

            var billingAddressRowsAffected = await connection.ExecuteAsync(updateBillingAddress, dto, transaction);

            rowsAffected += billingAddressRowsAffected;

            const string updateShippingAddress = @"
                UPDATE Addresses
                SET
                    country = @ShippingAddressCountry,
                    region = @ShippingAddressRegion,
                    post_code = @ShippingAddressPostCode,
                    city = @ShippingAddressCity,
                    address_1 = @ShippingAddressFirstLine,
                    address_2 = @ShippingAddressSecondLine
                WHERE id = @ShippingAddressId";

            var shippingAddressRowsAffected = await connection.ExecuteAsync(updateShippingAddress, dto, transaction);

            rowsAffected += shippingAddressRowsAffected;

            transaction.Commit();

            return rowsAffected > 0;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string delete = @"
            DELETE FROM Customers 
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(delete, new
        {
            id
        });

        return rowsAffected > 0;
    }

}