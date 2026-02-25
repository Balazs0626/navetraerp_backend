using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;

namespace NavetraERP.Services;

/*
    GoodsReceiptService, SalesOrderService is hozzáfér a táblához
*/

public class InventoryItemService
{

    private readonly IConfiguration _config;

    public InventoryItemService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateInventoryItemDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string insert = @"
                INSERT INTO InventoryItems (
                    warehouse_id,
                    product_id,
                    quantity_on_hand,
                    batch_number,
                    last_updated
                )
                VALUES (
                    @WarehouseId,
                    @ProductId,
                    @QuantityOnHand,
                    @BatchNumber,
                    GETDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var parameters = new DynamicParameters(dto);

            var result = await connection.ExecuteScalarAsync<int>(insert, parameters, transaction);

            transaction.Commit();

            return result;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

    }

    public async Task<IEnumerable<InventoryItemListDto>> GetAllAsync(int? warehouseId = null, int? productId = null, string? batchNumber = null)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string query = @"
            SELECT
                ii.id AS Id,
                w.name AS WarehouseName,
                p.name AS ProductName,
                p.unit AS ProductUnit,
                ii.quantity_on_hand AS QuantityOnHand,
                ii.batch_number AS BatchNumber,
                ii.last_updated AS LastUpdated
            FROM InventoryItems ii
            JOIN Products p ON p.id = ii.product_id
            JOIN Warehouses w ON w.id = ii.warehouse_id
            WHERE 1 = 1";

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(batchNumber))
        {
            query += " AND ii.batch_number LIKE @BatchNumber";
            parameters.Add("@BatchNumber", $"%{batchNumber}%");
        }

        if (warehouseId != null)
        {
            query += " AND ii.warehouse_id = @WarehouseId";
            parameters.Add("@WarehouseId", warehouseId);
        }

        if (productId != null)
        {
            query += " AND ii.product_id = @ProductId";
            parameters.Add("@ProductId", productId);
        }

        var result = await connection.QueryAsync<InventoryItemListDto>(query, parameters);

        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string delete = @"
            DELETE FROM InventoryItems 
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(delete, new
        {
            id
        });

        return rowsAffected > 0;
    }

}