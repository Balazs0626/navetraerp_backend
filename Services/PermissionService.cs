using Dapper;
using System.Data.SqlClient;
using NavetraERP.Models;
using NavetraERP.DTOs;

namespace NavetraERP.Services;

public class PermissionService
{
    private readonly IConfiguration _config;

    public PermissionService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IEnumerable<RolePermissionDto>> GetAllAsync()
    {

        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                p.id AS PermissionId,
                p.module_id AS ModuleId,
                p.permission_name + ':' + m.module_name AS PermissionName
            FROM Permissions p
            LEFT JOIN Modules m ON p.module_id = m.id";

        var results = await connection.QueryAsync<RolePermissionDto>(query);

        return results;
    }
}