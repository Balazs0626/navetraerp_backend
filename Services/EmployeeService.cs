using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;

namespace NavetraERP.Services;

public class EmployeeService
{

    private readonly IConfiguration _config;

    public EmployeeService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateEmployeeDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertAddress = @"
                INSERT INTO HR_Addresses(country, region, post_code, city, address_1, address_2)
                VALUES (@AddressCountry, @AddressRegion, @AddressPostCode, @AddressCity, @AddressFirstLine, @AddressSecondLine);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var addressResult = await connection.ExecuteScalarAsync<int>(insertAddress, dto, transaction);

            const string insertTempAddress = @"
                INSERT INTO HR_Addresses(country, region, post_code, city, address_1, address_2)
                VALUES (@TempAddressCountry, @TempAddressRegion, @TempAddressPostCode, @TempAddressCity, @TempAddressFirstLine, @TempAddressSecondLine);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            int? tempAddressResult = null;

            if (!String.IsNullOrWhiteSpace(dto.TempAddressCountry) && !String.IsNullOrWhiteSpace(dto.TempAddressRegion) && !String.IsNullOrWhiteSpace(dto.TempAddressPostCode) && !String.IsNullOrWhiteSpace(dto.TempAddressCity) && !String.IsNullOrWhiteSpace(dto.TempAddressFirstLine) && !String.IsNullOrWhiteSpace(dto.TempAddressSecondLine))
            {
                tempAddressResult = await connection.ExecuteScalarAsync<int>(insertTempAddress, dto, transaction);
            }


            const string insertEmployee = @"
                INSERT INTO HR_Employee (
                    first_name,
                    last_name, 
                    birth_date, 
                    id_number, 
                    residence_number, 
                    health_insurance_number, 
                    tax_id_number, 
                    address_id, 
                    temp_address_id, 
                    hire_date, 
                    department_id,
                    position_id,
                    user_id,
                    email,
                    phone_number,
                    salary,
                    status
                )
                VALUES (
                    @FirstName,
                    @LastName,
                    @BirthDate,
                    @IdNumber,
                    @ResidenceNumber,
                    @HealthInsuranceNumber,
                    @TaxIdNumber,
                    @AddressId,
                    @TempAddressId,
                    @HireDate,
                    @DepartmentId,
                    @PositionId,
                    @UserId,
                    @Email,
                    @PhoneNumber,
                    @Salary,
                    @Status
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@AddressId", addressResult);
            parameters.Add("@TempAddressId", tempAddressResult);

            var result = await connection.ExecuteScalarAsync<int>(insertEmployee, parameters, transaction);

            transaction.Commit();

            return result;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

    }

    public async Task<IEnumerable<EmployeeListDto>> GetAllAsync()
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                e.id AS Id,
                e.first_name + ' ' + e.last_name AS FullName,
                d.department_name AS DepartmentName,
                p.position_name AS PositionName,
                e.user_id,
                CASE
                    WHEN e.user_id IS NOT NULL THEN 1
                    ELSE 0
                END AS HasUser
            FROM HR_Employee e
            JOIN HR_Departments d ON e.department_id = d.id
            JOIN HR_Positions p ON e.position_id = p.id";

        var result = await connection.QueryAsync<EmployeeListDto>(query);

        return result;
    }

    public async Task<EmployeeDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                e.id AS Id,
                e.first_name AS FirstName,
                e.last_name AS LastName,
                e.birth_date AS BirthDate,
                e.id_number AS IdNumber,
                e.residence_number AS ResidenceNumber,
                e.health_insurance_number AS HealthInsuranceNumber,
                e.tax_id_number AS TaxIdNumber,
                e.hire_date AS HireDate,
                e.department_id AS DepartmentId,
                e.position_id AS PositionId,
                e.user_id AS UserID,
                e.email AS Email,
                e.phone_number AS PhoneNumber,
                e.salary AS Salary,
                e.status AS Status,
                a.id AS AddressId,
                a.country AS AddressCountry,
                a.region AS AddressRegion,
                a.post_code AS AddressPostCode,
                a.city AS AddressCity,
                a.address_1 AS AddressFirstLine,
                a.address_2 AS AddressSecondLine,
                t.id AS TempAddressId,
                t.country AS TempAddressCountry,
                t.region AS TempAddressRegion,
                t.post_code AS TempAddressPostCode,
                t.city AS TempAddressCity,
                t.address_1 AS TempAddressFirstLine,
                t.address_2 AS TempAddressSecondLine
            FROM HR_Employee e
            JOIN HR_Addresses a ON a.id = e.address_id
            LEFT JOIN HR_Addresses t ON t.id = e.temp_address_id
            WHERE e.id = @id";

        var result = await connection.QueryFirstOrDefaultAsync<EmployeeDto>(query, new
        {
            id
        });

        return result;
    }

    public async Task<bool> UpdateAsync(int id, EmployeeDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        var rowsAffected = 0;

        try
        {
            const string updateEmployee = @"
                UPDATE HR_Employee
                SET
                    first_name = @FirstName,
                    last_name = @LastName,
                    birth_date = @BirthDate,
                    id_number = @IdNumber,
                    residence_number = @ResidenceNumber,
                    health_insurance_number = @HealthInsuranceNumber,
                    tax_id_number = @TaxIdNumber,
                    hire_date = @HireDate,
                    department_id = @DepartmentId,
                    position_id = @PositionId,
                    user_id = @UserId,
                    email = @Email,
                    phone_number = @PhoneNumber,
                    salary = @Salary,
                    status = @Status
                WHERE id = @id";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@id", id);

            rowsAffected = await connection.ExecuteAsync(updateEmployee, parameters, transaction);

            const string updateAddress = @"
                UPDATE HR_Addresses
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

            if (dto.TempAddressId != null)
            {
                const string updateTempAddress = @"
                    UPDATE HR_Addresses
                    SET
                        country = @TempAddressCountry,
                        region = @TempAddressRegion,
                        post_code = @TempAddressPostCode,
                        city = @TempAddressCity,
                        address_1 = @TempAddressFirstLine,
                        address_2 = @TempAddressSecondLine
                    WHERE id = @TempAddressId";

                var tempAddressRowsAffected = await connection.ExecuteAsync(updateTempAddress, dto, transaction);

                rowsAffected += tempAddressRowsAffected;
            }
            else
            {
                const string insertTempAddress = @"
                    INSERT INTO HR_Addresses(country, region, post_code, city, address_1, address_2)
                    VALUES (@TempAddressCountry, @TempAddressRegion, @TempAddressPostCode, @TempAddressCity, @TempAddressFirstLine, @TempAddressSecondLine);
                    SELECT CAST(SCOPE_IDENTITY() AS INT)";

                if (!String.IsNullOrWhiteSpace(dto.TempAddressCountry) || !String.IsNullOrWhiteSpace(dto.TempAddressRegion) || !String.IsNullOrWhiteSpace(dto.TempAddressPostCode) || !String.IsNullOrWhiteSpace(dto.TempAddressCity) || !String.IsNullOrWhiteSpace(dto.TempAddressFirstLine) || !String.IsNullOrWhiteSpace(dto.TempAddressSecondLine))
                {
                    var tempAddressResult = await connection.ExecuteScalarAsync<int>(insertTempAddress, dto, transaction);

                    const string updateEmployeeTempAddress = @"
                        UPDATE HR_Employee
                        SET
                            temp_address_id = @TempAddressId
                        WHERE id = @id";

                    var tempAddressRowsAffected = await connection.ExecuteAsync(updateEmployeeTempAddress, new
                    {
                        TempAddressId = tempAddressResult,
                        id
                    }, transaction);

                    rowsAffected += tempAddressRowsAffected;
                }
            }

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

        const string deleteEmployee = @"
            DELETE FROM HR_Employee 
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(deleteEmployee, new
        {
            id
        });

        return rowsAffected > 0;
    }
}