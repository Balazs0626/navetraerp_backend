using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;

namespace NavetraERP.Services;

public class DepartmentService
{

    private readonly IConfiguration _config;

    public DepartmentService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(DepartmentDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string insert = @"
            INSERT INTO HR_Departments (department_name, description)
            VALUES (@DepartmentName, @Description);
            SELECT CAST(SCOPE_IDENTITY() AS INT)";

        var result = await connection.ExecuteScalarAsync<int>(insert, dto);

        return result;
    }

    public async Task<IEnumerable<DepartmentListDto>> GetAllAsync()
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                id AS Id,
                department_name AS DepartmentName,
                description AS Description
            FROM HR_Departments";

        var result = await connection.QueryAsync<DepartmentListDto>(query);

        return result;
    }

    public async Task<DepartmentDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                department_name AS DepartmentName,
                description AS Description
            FROM HR_Departments
            WHERE id = @id";

        var result = await connection.QueryFirstOrDefaultAsync<DepartmentDto>(query, new
        {
            id
        });

        return result;
    }

    public async Task<bool> UpdateAsync(int id, DepartmentDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string update = @"
            UPDATE HR_Departments
            SET 
                department_name = @DepartmentName,
                description = @Description
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(update, new
        {
            dto.DepartmentName,
            dto.Description,
            id
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string delete = @"
            DELETE FROM HR_Departments
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(delete, new
        {
            id
        });

        return rowsAffected > 0;
    }
}