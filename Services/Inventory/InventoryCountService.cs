using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;

namespace NavetraERP.Services;

public class InventoryCountService
{

    private readonly IConfiguration _config;

    public InventoryCountService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateInventoryCountDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertInventoryCount = @"
                INSERT INTO InventoryCounts (
                    warehouse_id,
                    count_date,
                    counted_by_id
                )
                VALUES (
                    @WarehouseId,
                    @CountDate,
                    @CountedById
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var result = await connection.ExecuteScalarAsync<int>(insertInventoryCount, dto, transaction);
            
            foreach (var item in dto.Items)
            {
                const string insertInventoryCountItem = @"
                    INSERT INTO InventoryCountItems (
                        count_id,
                        product_id,
                        counted_quantity,
                        system_quantity
                    )
                    VALUES (
                        @CountId,
                        @ProductId,
                        @CountedQuantity,
                        @SystemQuantity
                    )";

                var itemResult = await connection.ExecuteScalarAsync<int>(insertInventoryCountItem, new
                {
                    CountId = result,
                    ProductId = item.ProductId,
                    CountedQuantity = item.CountedQuantity,
                    SystemQuantity = item.SystemQuantity
                }, transaction);
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

    public async Task<IEnumerable<InventoryCountListDto>> GetAllAsync(int? purchaseOrderId = null, int? warehouseId = null, DateTime? date = null)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string query = @"
            SELECT
                ic.id AS ID,
                e.first_name + ' ' + e.last_name AS CountedByName,
                w.name AS WarehouseName,
                ic.count_date AS CountDate
            FROM InventoryCounts ic
            JOIN Warehouses w ON w.id = ic.warehouse_id
            JOIN Employees e ON e.id = ic.counted_by_id
            WHERE 1 = 1";

        var parameters = new DynamicParameters();

        var result = await connection.QueryAsync<InventoryCountListDto>(query, parameters);

        return result;
    }

    public async Task<InventoryCountDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        var transaction = connection.BeginTransaction();
        
        try
        {
            const string inventoryCountQuery = @"
                SELECT
                    ic.id AS Id,
                    ic.warehouse_id AS WarehouseId,
                    w.name AS WarehouseName,
                    ic.count_date AS CountDate,
                    ic.counted_by_id AS CountedById,
                    e.first_name + ' ' + e.last_name AS CountedByName
                FROM InventoryCounts ic
                JOIN Warehouses w ON w.id = ic.warehouse_id
                JOIN Employees e ON e.id = ic.counted_by_id
                WHERE ic.id = @id";

            var result = await connection.QueryFirstOrDefaultAsync<InventoryCountDto>(inventoryCountQuery, new
            {
                id
            }, transaction);

            const string inventoryCountItemsQuery = @"
                SELECT
                    ici.id AS Id,
                    ici.count_id AS CountId,
                    ici.product_id AS ProductId,
                    p.name AS ProductName,
                    p.unit AS ProductUnit,
                    ici.counted_quantity AS CountedQuantity,
                    ici.system_quantity AS SystemQuantity
                FROM InventoryCountItems ici
                JOIN Products p ON p.id = ici.product_id
                WHERE ici.count_id = @id";

            var items = (await connection.QueryAsync<InventoryCountItemDto>(inventoryCountItemsQuery, new
            {
                id
            }, transaction)).ToList();

            result.Items = items;

            transaction.Commit();

            return result;
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

            const string deleteInventoryCountItems = @"
                DELETE FROM InventoryCountItems 
                WHERE count_id = @id";

            var rowsAffected = await connection.ExecuteAsync(deleteInventoryCountItems, new
            {
                id
            }, transaction);

            const string deleteInventoryCount = @"
                DELETE FROM InventoryCounts 
                WHERE id = @id";

            rowsAffected = await connection.ExecuteAsync(deleteInventoryCount, new
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