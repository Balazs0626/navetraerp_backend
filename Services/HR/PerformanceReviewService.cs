using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;
using NavetraERP.Models;

namespace NavetraERP.Services;

public class PerformanceReviewService
{

    private readonly IConfiguration _config;

    public PerformanceReviewService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreatePerformanceReviewDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string insert = @"
            INSERT INTO HR_PerformanceReviews (employee_id, review_date, score, comment)
            VALUES (@EmployeeId, @ReviewDate, @Score, @Comment);
            SELECT CAST(SCOPE_IDENTITY() AS INT)";

        var result = await connection.ExecuteScalarAsync<int>(insert, dto);

        return result;
    }

    public async Task<IEnumerable<PerformanceReviewListDto>> GetAllAsync(string? employeeName = null, DateTime? date = null)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string query = @"
            SELECT
                pr.id AS Id,
                e.first_name + ' ' + e.last_name AS EmployeeName,
                pr.review_date AS ReviewDate,
                pr.score AS Score
            FROM HR_PerformanceReviews pr
            JOIN HR_Employee e ON e.id = pr.employee_id
            WHERE 1 = 1";

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(employeeName))
        {
            query += " AND (e.first_name + ' ' + e.last_name) LIKE @Name";
            parameters.Add("@Name", $"%{employeeName}%");
        }

        if (date.HasValue)
        {
            query += " AND pr.review_date = @Date";
            parameters.Add("@Date", date.Value.Date);
        }

        var result = await connection.QueryAsync<PerformanceReviewListDto>(query, parameters);

        return result;
    }

    public async Task<PerformanceReviewDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                pr.id AS Id,
                e.first_name + ' ' + e.last_name AS EmployeeName,
                pr.review_date AS ReviewDate,
                pr.score AS Score,
                pr.comment AS Comment
            FROM HR_PerformanceReviews pr
            JOIN HR_Employee e ON e.id = pr.employee_id
            WHERE pr.id = @id";

        var result = await connection.QueryFirstOrDefaultAsync<PerformanceReviewDto>(query, new
        {
            id
        });

        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string delete = @"
            DELETE FROM HR_PerformanceReviews
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(delete, new
        {
            id
        });

        return rowsAffected > 0;
    }
}