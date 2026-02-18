using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;
using NavetraERP.Models;

namespace NavetraERP.Services;

public class PositionService
{

    private readonly IConfiguration _config;

    public PositionService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(PositionDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string insert = @"
            INSERT INTO Positions (position_name, description)
            VALUES (@PositionName, @Description);
            SELECT CAST(SCOPE_IDENTITY() AS INT)";

        var result = await connection.ExecuteScalarAsync<int>(insert, dto);

        return result;
    }

    public async Task<IEnumerable<Position>> GetAllAsync()
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                id AS Id,
                position_name AS PositionName,
                description AS Description
            FROM Positions";

        var result = await connection.QueryAsync<Position>(query);

        return result;
    }

    public async Task<PositionDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                position_name AS PositionName,
                description AS Description
            FROM Positions
            WHERE id = @id";

        var result = await connection.QueryFirstOrDefaultAsync<PositionDto>(query, new
        {
            id
        });

        return result;
    }

    public async Task<bool> UpdateAsync(int id, PositionDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string update = @"
            UPDATE Positions
            SET
                position_name = @PositionName,
                description = @Description
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(update, new
        {
            dto.PositionName,
            dto.Description,
            id
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string delete = @"
            DELETE FROM Positions
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(delete, new
        {
            id
        });

        return rowsAffected > 0;
    }

}