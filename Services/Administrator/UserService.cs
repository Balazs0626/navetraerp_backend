using Dapper;
using System.Data.SqlClient;
using NavetraERP.Models;
using NavetraERP.DTOs;

namespace NavetraERP.Services;

public class UserService
{
    private readonly IConfiguration _config;

    public UserService(IConfiguration config)
    {
        _config = config;
    }

    #region Authentication tasks
    public async Task<User> GetByUsernameAsync(string username)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        var query = @"
            SELECT
                id,
                username,
                password_hash AS PasswordHash,
                role_id AS RoleId
            FROM Users
            WHERE username = @username";

        var result = await connection.QueryFirstOrDefaultAsync<User>(query, new
        {
            username
        });

        return result;
    }

    public async Task<bool> SetUserActiveAsync(int id, bool active)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        var update = @"
            UPDATE Users
            SET active = @active
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(update, new
        {
            id,
            active
        });

        return rowsAffected > 0;
    }

    public async Task<User> CreateAsync(string username, string passwordHash, int roleId, string email)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        var insert = @"
            INSERT INTO Users (username, password_hash, role_id, email, active)
            VALUES (@username, @passwordHash, @roleId, @email, 0)
            SELECT CAST(SCOPE_IDENTITY() AS INT)";

        var result = await connection.ExecuteScalarAsync<int>(insert, new
        {
            username,
            passwordHash,
            roleId,
            email
        });

        return new User { Id = result, Username = username, PasswordHash = passwordHash, RoleId = roleId, Email = email, Active = false };
    }

    public async Task<bool> UpdateUserRoleAsync(int id, int roleId)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        var update = @"
            UPDATE B_Users
            SET role_id = @roleId
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(update, new
        {
            id,
            roleId
        });

        return rowsAffected > 0;
    }
    #endregion

    #region User tasks

    public async Task<IEnumerable<UserListDto>> GetAllAsync()
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                u.id AS Id,
                u.username AS Username,
                u.email AS Email,
                r.role_name AS Role
            FROM Users u
            JOIN Roles r ON u.role_id = r.id";

        var results = await connection.QueryAsync<UserListDto>(query);

        return results;
    }

    public async Task<UserUpdateDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                username AS Username,
                '' AS PasswordHash,
                email AS Email,
                role_id AS RoleId
            FROM Users
            WHERE id = @id";

        var result = await connection.QuerySingleOrDefaultAsync<UserUpdateDto>(query, new
        {
            id
        });

        return result;
    }

    public async Task<bool> UpdateAsync(int id, UserUpdateDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string update = @"
            UPDATE Users
            SET
                username = @Username,
                email = @Email,
                role_id = @RoleId";

        if (!String.IsNullOrWhiteSpace(dto.PasswordHash))
        {
            update += ", password_hash = @PasswordHash";
        }

        update += " WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(update, new
        {
            dto.Username,
            dto.Email,
            dto.RoleId,
            dto.PasswordHash,
            id
        });

        Console.WriteLine(dto.PasswordHash);

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string delete = @"
            DELETE FROM Users
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(delete, new
        {
            id
        });

        return rowsAffected > 0;
    }

    public async Task<int> GetActiveUserCountAsync()
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT COUNT(*) 
            FROM Users
            WHERE active = 1";

        var count = await connection.ExecuteScalarAsync<int>(query);

        return count;
    }

    #endregion

    #region Password Reset

    public async Task<User> GetByEmailAsync(string email)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));
        const string query = "SELECT id, username, email FROM Users WHERE email = @email";
        return await connection.QueryFirstOrDefaultAsync<User>(query, new { email });
    }

    public async Task SaveResetTokenAsync(int userId, string token, DateTime expires)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));
        const string update = @"
            UPDATE Users 
            SET reset_token = @token, 
                reset_token_expires = @expires 
            WHERE id = @userId";
        await connection.ExecuteAsync(update, new { userId, token, expires });
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newHash)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));
        // Egyszerre ellenőrizzük a tokent és a lejárati időt
        const string query = @"
            UPDATE Users 
            SET password_hash = @newHash, 
                reset_token = NULL, 
                reset_token_expires = NULL 
            WHERE email = @email 
            AND reset_token = @token 
            AND reset_token_expires > GETUTCDATE()";

        var rowsAffected = await connection.ExecuteAsync(query, new { email, token, newHash });
        return rowsAffected > 0;
    }

    #endregion
}