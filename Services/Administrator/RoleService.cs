using Dapper;
using System.Data.SqlClient;
using NavetraERP.Models;
using NavetraERP.DTOs;

namespace NavetraERP.Services;

public class RoleService
{
    private readonly IConfiguration _config;

    public RoleService(IConfiguration config)
    {
        _config = config;
    }

    #region Auth tasks

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

    #endregion

    #region Role tasks

    public async Task<int> CreateAsync(RoleDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        var result = 0;

        try
        {
            const string insert = @"
                INSERT INTO Roles (role_name)
                VALUES (@RoleName);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            result = await connection.ExecuteScalarAsync<int>(insert, new
            {
                dto.RoleName
            }, transaction);

            if (dto.Permissions != null)
            {

                const string insertPermissions = @"
                    INSERT INTO RolePermissions
                        (role_id, permission_id)
                    VALUES
                        (@RoleId, @PermissionId)";

                foreach (var permission in dto.Permissions)
                {
                    await connection.ExecuteAsync(insertPermissions, new
                    {
                        RoleId = result,
                        PermissionId = permission.PermissionId
                    }, transaction);
                }
            }

            transaction.Commit();

            return result;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                id,
                role_name AS RoleName
            FROM Roles";

        var results = await connection.QueryAsync<Role>(query);

        return results;
    }

    public async Task<RoleDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string roleQuery = @"
            SELECT
                id AS Id,
                role_name AS RoleName
            FROM Roles
            WHERE id = @id";

        const string permissionQuery = @"
            SELECT
                rp.permission_id AS PermissionId,
                p.module_id AS ModuleId,
                p.permission_name + ':' + m.module_name AS PermissionName
            FROM RolePermissions rp
            JOIN Permissions p ON rp.permission_id = p.id
            JOIN Modules m ON p.module_id = m.id
            JOIN Roles r ON rp.role_id = r.id
            WHERE rp.role_id = @id";

        var roleResult = await connection.QueryFirstOrDefaultAsync<RoleDto>(roleQuery, new
        {
            id
        });

        var permissionResults = await connection.QueryAsync<RolePermissionDto>(permissionQuery, new
        {
            id
        });

        if (roleResult != null) roleResult.Permissions = permissionResults.ToList();

        return roleResult;
    }

    public async Task<bool> UpdateAsync(int id, RoleDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        var transaction = connection.BeginTransaction();

        var rowsAffected = 0;

        try
        {
            const string updateRole = @"
                UPDATE Roles
                SET
                    role_name = @RoleName
                WHERE id = @id";

            rowsAffected = await connection.ExecuteAsync(updateRole, new
            {
                dto.RoleName,
                id
            }, transaction);

            if (dto.Permissions != null)
            {
                const string deleteRolePermissions = @"
                    DELETE FROM RolePermissions 
                    WHERE role_id = @id";

                await connection.ExecuteAsync(deleteRolePermissions, new
                {
                    id
                }, transaction);

                foreach (var permission in dto.Permissions)
                {
                    const string insert = @"
                        INSERT INTO RolePermissions
                            (role_id, permission_id)
                        VALUES
                            (@RoleId, @PermissionId)";

                    await connection.ExecuteAsync(insert, new
                    {
                        RoleId = id,
                        PermissionId = permission.PermissionId
                    }, transaction);
                }
            }

            transaction.Commit();

            return rowsAffected > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string delete = @"
            DELETE FROM Roles
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(delete, new
        {
            id
        });

        return rowsAffected > 0;
    }

    #endregion
}
