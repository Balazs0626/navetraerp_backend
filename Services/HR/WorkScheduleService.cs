using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;
using NavetraERP.Models;

namespace NavetraERP.Services;

public class WorkScheduleService
{

    private readonly IConfiguration _config;

    public WorkScheduleService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateWorkScheduleDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        int counter = 0;

        foreach (var employeeId in dto.EmployeeIds)
        {

            const string insert = @"
                INSERT INTO HR_EmployeeShifts (employee_id, shift_id, date)
                VALUES (@EmployeeId, @ShiftId, @Date);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var result = await connection.ExecuteScalarAsync<int>(insert, new
            {
                EmployeeId = employeeId,
                ShiftId = dto.ShiftId,
                Date = dto.Date
            });

            counter++;
        }

        return counter;
    }

    public async Task<IEnumerable<WorkScheduleDto>> GetAllAsync(string? name = null, DateTime? date = null)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string query = @"
            SELECT
                es.id AS Id,
                e.first_name + ' ' + e.last_name As EmployeeName,
                s.shift_name AS ShiftName,
                es.date AS Date,
                s.start_time AS StartTime,
                s.end_time AS EndTime
            FROM HR_EmployeeShifts es
            JOIN HR_Employee e ON e.id = es.employee_id
            JOIN HR_Shifts s ON s.id = es.shift_id
            WHERE 1 = 1";

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query += " AND (e.first_name + ' ' + e.last_name) LIKE @Name";
            parameters.Add("@Name", $"%{name}%");
            System.Console.WriteLine("asdsadas");
        }

        if (date.HasValue)
        {
            query += " AND es.date = @Date";
            parameters.Add("@Date", date.Value.Date);
        }

        query += " ORDER BY es.date DESC";

        var result = await connection.QueryAsync<WorkScheduleDto>(query, parameters);

        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connenction = new SqlConnection(_config.GetConnectionString("Default"));

        const string delete = @"
            DELETE FROM HR_EmployeeShifts
            WHERE id = @id";

        var rowsAffected = await connenction.ExecuteAsync(delete, new
        {
            id
        });

        return rowsAffected > 0;
    }

}