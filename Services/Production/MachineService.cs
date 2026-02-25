using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;

namespace NavetraERP.Services;

public class MachineService
{

    private readonly IConfiguration _config;

    public MachineService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateMachineDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string insert = @"
            INSERT INTO Machines (
                name, 
                code, 
                description,
                active
            )
            VALUES (
                @Name,
                @Code, 
                @Description,
                @Active
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT)";

        var result = await connection.ExecuteScalarAsync<int>(insert, dto);

        return result;
    }

    public async Task<IEnumerable<MachineListDto>> GetAllAsync()
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                id AS Id,
                name AS Name,
                code AS Code,
                active AS Active
            FROM Machines";

        var result = await connection.QueryAsync<MachineListDto>(query);

        return result;
    }

    public async Task<MachineDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                id AS Id,
                name AS Name,
                code AS Code,
                description AS Description,
                active AS Active
            FROM Machines
            WHERE id = @id";

        var result = await connection.QueryFirstOrDefaultAsync<MachineDto>(query, new
        {
            id
        });

        return result;
    }

    public async Task<bool> UpdateAsync(int id, MachineDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string update = @"
            UPDATE Machines
            SET 
                name = @Name,
                code = @Code,
                description = @Description,
                active = @Active
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(update, new
        {
            dto.Name,
            dto.Code,
            dto.Description,
            dto.Active,
            id
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string delete = @"
            DELETE FROM Machines
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(delete, new
        {
            id
        });

        return rowsAffected > 0;
    }
}