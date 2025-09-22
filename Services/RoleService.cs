using Dapper;
using System.Data.SqlClient;
using NavetraERP.Models;

namespace NavetraERP.Services;

public class RoleService
{
    private readonly IConfiguration _config;

    public RoleService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<Role> GetRoleByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        var query = @"
            SELECT
                id,
                role_name AS RoleName
            FROM Roles
            WHERE id = @id";

        var result = await connection.QuerySingleOrDefaultAsync<Role>(query, new
        {
            id
        });

        return result;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        var query = @"
            SELECT
                p.id,
                p.permission_name + ':' + m.module_name AS PermissionName
            FROM RolePermissions rp
            JOIN Permissions p ON rp.permission_id = p.id
            JOIN Modules m ON p.module_id = m.id
            WHERE rp.role_id = @id";

        var results = await connection.QueryAsync<Permission>(query, new
        {
            id
        });

        return results;
    }
}
