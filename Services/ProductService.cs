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

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try 
        {
            const string insert = @"
                INSERT INTO Products (name, sku, description, unit, price_per_unit, active, created_at)
                VALUES (@Name, @Sku, @Description, @Unit, @PricePerUnit, @Active, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var result = await connection.ExecuteScalarAsync<int>(insert, dto, transaction);

            if (dto.BomComponents != null)
            {
                foreach (var component in dto.BomComponents)
                {
                    const string insertBomComponent = @"
                        INSERT INTO BillOfMaterials (
                            product_id,
                            component_product_id,
                            quantity_per_unit
                        )
                        VALUES (
                            @ProductId,
                            @ComponentProductId,
                            @QuantityPerUnit
                        )";

                    var componentResult = await connection.ExecuteScalarAsync<int>(insertBomComponent, new
                    {
                        ProductId = result,
                        ComponentProductId = component.ComponentProductId,
                        QuantityPerUnit = component.QuantityPerUnit
                    }, transaction);
                }
            }

            transaction.Commit();

            return result;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<ProductListDto>> GetAllAsync(string? sku = null, string? name = null, bool? active = null)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string query = @"
            SELECT
                p.id AS Id,
                p.sku AS Sku,
                p.name AS Name,
                p.unit AS Unit,
                p.price_per_unit AS PricePerUnit,
                p.active AS Active,
                (
                    SELECT COUNT(*)
                    FROM BillOfMaterials bom
                    WHERE bom.product_id = p.id
                ) AS ComponentCount
            FROM Products p
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

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
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

            var result = await connection.QueryFirstOrDefaultAsync<ProductDto>(query, new
            {
                id
            }, transaction);

            const string bomQuery = @"
                SELECT
                    bom.id AS Id,
                    bom.product_id AS ProductId,
                    bom.component_product_id AS ComponentProductId,
                    p.sku AS ComponentProductSku,
                    p.name AS ComponentProductName,
                    p.unit AS ComponentProductUnit,
                    p.price_per_unit AS ComponentProductPricePerUnit,
                    (p.price_per_unit * bom.quantity_per_unit) AS ComponentProductAllPrice,
                    bom.quantity_per_unit AS QuantityPerUnit
                FROM BillOfMaterials bom
                JOIN Products p ON p.id = bom.component_product_id
                WHERE product_id = @id";

            var components = (await connection.QueryAsync<BomComponentDto>(bomQuery, new
            {
                id
            }, transaction)).ToList();

            result.BomComponents = components;

            transaction.Commit();

            return result;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, ProductDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        var rowsAffected = 0;

        try
        {
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

            rowsAffected += await connection.ExecuteAsync(update, new
            {
                dto.Name,
                dto.Sku,
                dto.Description,
                dto.Unit,
                dto.PricePerUnit,
                dto.Active,
                dto.CreatedAt,
                id
            }, transaction);

            if (dto.BomComponents != null)
            {
                var deleteQuery = "DELETE FROM BillOfMaterials WHERE product_id = @id";

                await connection.ExecuteAsync(deleteQuery, new
                {
                    id
                }, transaction);

                foreach (var component in dto.BomComponents)
                {
                    const string insertBomComponent = @"
                        INSERT INTO BillOfMaterials (
                            product_id,
                            component_product_id,
                            quantity_per_unit
                        )
                        VALUES (
                            @ProductId,
                            @ComponentProductId,
                            @QuantityPerUnit
                        )";

                    var componentResult = await connection.ExecuteScalarAsync<int>(insertBomComponent, new
                    {
                        ProductId = id,
                        ComponentProductId = component.ComponentProductId,
                        QuantityPerUnit = component.QuantityPerUnit
                    }, transaction);
                }
            }

            transaction.Commit();

            return rowsAffected > 0;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        var transaction = connection.BeginTransaction();

        try
        {
            const string deleteBomComponents = @"
                DELETE FROM BillOfMaterials
                WHERE product_id = @id";

            var rowsAffected = await connection.ExecuteAsync(deleteBomComponents, new
            {
                id
            }, transaction);

            const string deleteProduct = @"
                DELETE FROM Products
                WHERE id = @id";

            rowsAffected = await connection.ExecuteAsync(deleteProduct, new
            {
                id
            }, transaction);


            transaction.Commit();

            return rowsAffected > 0;  
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

    }

}