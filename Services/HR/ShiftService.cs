using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;
using NavetraERP.Models;

namespace NavetraERP.Services;

public class ShiftService
{

    private readonly IConfiguration _config;

    public ShiftService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(ShiftDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string insert = @"
            INSERT INTO Shifts (shift_name, start_time, end_time)
            VALUES (@ShiftName, @StartTime, @EndTime);
            SELECT CAST(SCOPE_IDENTITY() AS INT)";

        var result = await connection.ExecuteScalarAsync<int>(insert, dto);

        return result;
    }

    public async Task<IEnumerable<Shift>> GetAllAsync()
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                id AS Id,
                shift_name AS ShiftName,
                start_time AS StartTime,
                end_time AS EndTime
            FROM Shifts";

        var result = await connection.QueryAsync<Shift>(query);

        return result;
    }

    public async Task<ShiftDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                shift_name AS ShiftName,
                start_time AS StartTime,
                end_time AS EndTime
            FROM Shifts
            WHERE id = @id";

        var result = await connection.QuerySingleOrDefaultAsync<ShiftDto>(query, new
        {
            id
        });

        return result;
    }

    public async Task<bool> UpdateAsync(int id, ShiftDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string update = @"
            UPDATE Shifts
            SET
                shift_name = @ShiftName,
                start_time = @StartTime,
                end_time = @EndTime
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(update, new
        {
            dto.ShiftName,
            dto.StartTime,
            dto.EndTime,
            id
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string delete = @"
            DELETE FROM Shifts
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(delete, new
        {
            id
        });

        return rowsAffected > 0;
    }

}