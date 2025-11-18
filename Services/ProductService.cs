using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;
using NavetraERP.Models;

namespace NavetraERP.Services;

public class ProductService
{

    private readonly IConfiguration _config;

    public ProductService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateProductDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string insert = @"
            INSERT INTO Products (name, sku, description, unit, price_per_unit, active, created_at)
            VALUES (@Name, @Sku, @Description, @Unit, @PricePerUnit, @Active, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT)";

        var result = await connection.ExecuteScalarAsync<int>(insert, dto);

        return result;
    }

    public async Task<IEnumerable<ProductListDto>> GetAllAsync(string? sku = null, string? name = null, bool? active = null)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string query = @"
            SELECT
                id AS Id,
                sku AS Sku,
                name AS Name,
                active AS Active
            FROM Products
            WHERE 1 = 1";

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(sku))
        {
            query += " AND sku LIKE @Sku";
            parameters.Add("@Sku", $"%{sku}%");
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            query += " AND name LIKE @name";
            parameters.Add("@Name", $"%{name}%");
        }

        if (active != null)
        {
            query += " AND active = @Active";
            parameters.Add("@Active", active);
        }

        var result = await connection.QueryAsync<ProductListDto>(query, parameters);

        return result;
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                id AS Id,
                sku AS Sku,
                name AS Name,
                description AS Description,
                unit AS Unit,
                price_per_unit AS PricePerUnit,
                active AS Active,
                created_at AS CreatedAt
            FROM Products
            WHERE id = @id";

        var result = await connection.QueryFirstOrDefaultAsync<Product>(query, new
        {
            id
        });

        return result;
    }

    public async Task<bool> UpdateAsync(int id, Product model)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string update = @"
            UPDATE Products
            SET 
                name = @Name,
                sku = @Sku,
                description = @Description,
                unit = @Unit,
                price_per_unit = @PricePerUnit,
                active = @Active
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(update, new
        {
            model.Name,
            model.Sku,
            model.Description,
            model.Unit,
            model.PricePerUnit,
            model.Active,
            model.CreatedAt,
            id
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string delete = @"
            DELETE FROM Products
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(delete, new
        {
            id
        });

        return rowsAffected > 0;
    }

}