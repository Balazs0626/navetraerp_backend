using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;

namespace NavetraERP.Services;

public class WarehouseService
{

    private readonly IConfiguration _config;

    public WarehouseService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateWarehouseDto dto)
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

            const string insertWarehouse = @"
                INSERT INTO Warehouses (
                    name,
                    address_id,
                    manager_employee_id
                )
                VALUES (
                    @Name,
                    @AddressId,
                    @ManagerId
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@AddressId", addressResult);

            var result = await connection.ExecuteScalarAsync<int>(insertWarehouse, parameters, transaction);

            transaction.Commit();

            return result;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

    }

    public async Task<IEnumerable<WarehouseListDto>> GetAllAsync()
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                w.id AS Id,
                w.name AS Name,
                a.city + ', ' + a.address_1 + ' (' + a.country + ', ' + a.region + ')' AS Address,
                e.first_name + ' ' + e.last_name AS ManagerName
            FROM Warehouses w
            JOIN Addresses a ON a.id = w.address_id
            JOIN Employees e ON e.id = w.manager_employee_id";

        var result = await connection.QueryAsync<WarehouseListDto>(query);

        return result;
    }

    public async Task<UpdateWarehouseDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                w.id AS Id,
                w.name AS Name,
                w.manager_employee_id AS ManagerId,
                w.address_id AS AddressId,
                a.country AS AddressCountry,
                a.region AS AddressRegion,
                a.post_code AS AddressPostCode,
                a.city AS AddressCity,
                a.address_1 AS AddressFirstLine,
                a.address_2 AS AddressSecondLine
            FROM Warehouses w
            JOIN Addresses a ON a.id = w.address_id
            WHERE w.id = @id";

        var result = await connection.QueryFirstOrDefaultAsync<UpdateWarehouseDto>(query, new
        {
            id
        });

        return result;
    }

    public async Task<bool> UpdateAsync(int id, UpdateWarehouseDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        var rowsAffected = 0;

        try
        {
            const string updateWarehouse = @"
                UPDATE Warehouses
                SET
                    name = @Name,
                    manager_employee_id = @ManagerId
                WHERE id = @id";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@id", id);

            rowsAffected = await connection.ExecuteAsync(updateWarehouse, parameters, transaction);

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
            DELETE FROM Warehouses 
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(delete, new
        {
            id
        });

        return rowsAffected > 0;
    }

}