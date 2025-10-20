using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;

namespace NavetraERP.Services;

public class LeaveRequestService
{

    private readonly IConfiguration _config;

    public LeaveRequestService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateLeaveRequestDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string insert = @"
            INSERT INTO HR_LeaveRequests (employee_id, start_data, end_date, leave_type, status)
            VALUES (@EmployeeId, @StartDate, @EndDate, @LeaveType, @Status);
            SELECT CAST(SCOPE_IDENTITY() AS INT)";

        var result = await connection.ExecuteScalarAsync<int>(insert, dto);

        return result;
    }

    public async Task<IEnumerable<LeaveRequestListDto>> GetAllAsync()
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                lr.id AS Id,
                e.first_name + ' ' + e.last_name AS EmployeeName,
                lr.status AS Status
            FROM HR_LeaveRequests lr
            JOIN HR_Employee e ON e.id = lr.employee_id";

        var result = await connection.QueryAsync<LeaveRequestListDto>(query);

        return result;
    }

}