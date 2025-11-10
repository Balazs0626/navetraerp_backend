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

    public async Task<IEnumerable<LeaveRequestListDto>> GetAllAsync(string? employeeName = null, string? leaveType = null, string? status = null)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string query = @"
            SELECT
                lr.id AS Id,
                e.first_name + ' ' + e.last_name AS EmployeeName,
                lr.leave_type AS LeaveType,
                lr.start_data AS StartDate,
                lr.end_date AS EndDate,
                lr.status AS Status
            FROM HR_LeaveRequests lr
            JOIN HR_Employee e ON e.id = lr.employee_id
            WHERE 1 = 1";

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(employeeName))
        {
            query += " AND (e.first_name + ' ' + e.last_name) LIKE @EmployeeName";
            parameters.Add("@EmployeeName", $"%{employeeName}%");
        }

        if (!string.IsNullOrWhiteSpace(leaveType))
        {
            query += " AND lr.leave_type = @LeaveType";
            parameters.Add("@LeaveType", leaveType);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query += " AND lr.status = @Status";
            parameters.Add("@Status", status);
        }

        var result = await connection.QueryAsync<LeaveRequestListDto>(query, parameters);

        return result;
    }

    public async Task<bool> ApproveAsync(LeaveRequestDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        var rowsAffected = 0;

        foreach (var id in dto.Ids)
        {
            const string update = @"
                UPDATE HR_LeaveRequests
                SET
                    status = 'approved'
                WHERE id = @id";

            rowsAffected = await connection.ExecuteAsync(update, new
            {
                id
            });
        }

        return rowsAffected > 0;
    }
    
    public async Task<bool> RejectAsync(LeaveRequestDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        var rowsAffected = 0;
        
        foreach (var id in dto.Ids)
        {
            const string update = @"
                UPDATE HR_LeaveRequests
                SET
                    status = 'rejected'
                WHERE id = @id";

            rowsAffected = await connection.ExecuteAsync(update, new
            {
                id
            });
        }

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string delete = @"
            DELETE FROM HR_LeaveRequests
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(delete, new
        {
            id
        });

        return rowsAffected > 0;
    }

}