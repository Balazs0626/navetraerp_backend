using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;

namespace NavetraERP.Services;

public class SupplierService
{

    private readonly IConfiguration _config;

    public SupplierService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateSupplierDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertAddress = @"
                INSERT INTO Addresses(country, region, post_code, city, address_1, address_2)
                VALUES (@AddressCountry, @AddressRegion, @AddressPostCode, @AddressCity, @AddressFirstLine, @AddressSecondLine);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var addressResult = await connection.ExecuteScalarAsync<int>(insertAddress, dto, transaction);

            const string insertSupplier = @"
                INSERT INTO Suppliers (
                    name,
                    tax_number,
                    contact_person,
                    eu_tax_number,
                    bank_account_number,
                    email,
                    phone_number,
                    address_id
                )
                VALUES (
                    @Name,
                    @TaxNumber,
                    @EuTaxNumber,
                    @BankAccountNumber,
                    @ContactPerson,
                    @Email,
                    @PhoneNumber,
                    @AddressId
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@AddressId", addressResult);

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

    public async Task<IEnumerable<SupplierListDto>> GetAllAsync()
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                id AS Id,
                name AS Name,
                contact_person AS ContactPerson,
                email AS Email,
                phone_number AS PhoneNumber
            FROM Suppliers";

        var result = await connection.QueryAsync<SupplierListDto>(query);

        return result;
    }

    public async Task<UpdateSupplierDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                s.id AS Id,
                s.name AS Name,
                s.tax_number AS TaxNumber,
                s.eu_tax_number AS EuTaxNumber,
                s.bank_account_number AS BankAccountNumber,
                s.contact_person AS ContactPerson,
                s.email AS Email,
                s.phone_number AS PhoneNumber,
                s.address_id AS AddressId,
                a.country AS AddressCountry,
                a.region AS AddressRegion,
                a.post_code AS AddressPostCode,
                a.city AS AddressCity,
                a.address_1 AS AddressFirstLine,
                a.address_2 AS AddressSecondLine
            FROM Suppliers s
            JOIN Addresses a ON a.id = s.address_id
            WHERE s.id = @id";

        var result = await connection.QueryFirstOrDefaultAsync<UpdateSupplierDto>(query, new
        {
            id
        });

        return result;
    }

    public async Task<bool> UpdateAsync(int id, UpdateSupplierDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        var rowsAffected = 0;

        try
        {
            const string updateSupplier = @"
                UPDATE Suppliers
                SET
                    name = @Name,
                    tax_number = @TaxNumber,
                    eu_tax_number = @EuTaxNumber,
                    bank_account_number = @BankAccountNumber,
                    contact_person = @ContactPerson,
                    email = @Email,
                    phone_number = @PhoneNumber
                WHERE id = @id";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@id", id);

            rowsAffected = await connection.ExecuteAsync(updateSupplier, parameters, transaction);

            const string updateAddress = @"
                UPDATE Addresses
                SET
                    country = @AddressCountry,
                    region = @AddressRegion,
                    post_code = @AddressPostCode,
                    city = @AddressCity,
                    address_1 = @AddressFirstLine,
                    address_2 = @AddressSecondLine
                WHERE id = @AddressId";

            var addressRowsAffected = await connection.ExecuteAsync(updateAddress, dto, transaction);

            rowsAffected += addressRowsAffected;

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
            DELETE FROM Suppliers 
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(delete, new
        {
            id
        });

        return rowsAffected > 0;
    }

}