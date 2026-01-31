using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;
using System.Security.AccessControl;

namespace NavetraERP.Services;

public class ProductionOutputService
{

    private readonly IConfiguration _config;

    public ProductionOutputService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateProductionOutputDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertProductionOutput = @"
                INSERT INTO ProductionOutputs (
                    production_order_id,
                    product_id,
                    quantity_produced,
                    warehouse_id,
                    date_produced
                )
                VALUES (
                    @ProductionOrderId,
                    @ProductId,
                    @QuantityProduced,
                    @WarehouseId,
                    @DateProduced
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var result = await connection.ExecuteScalarAsync<int>(insertProductionOutput, dto, transaction);

            const string insertStockMovement = @"
                INSERT INTO StockMovements (
                    product_id,
                    to_warehouse_id,
                    movement_type,
                    quantity,
                    reference_document,
                    movement_date
                )
                VALUES (
                    @ProductId,
                    @ToWarehouseId,
                    'in',
                    @Quantity,
                    @ReferenceDocument,
                    @MovementDate
                )";

            var stockMovementResult = await connection.ExecuteScalarAsync<int>(insertStockMovement, new
            {
                ProductId = dto.ProductId,
                ToWarehouseId = dto.WarehouseId,
                Quantity = dto.QuantityProduced,
                ReferenceDocument = $"GO-{result.ToString().PadLeft(5, '0')}",
                MovementDate = dto.DateProduced
            }, transaction);

            const string updateInventoryItem = @"
                UPDATE InventoryItems 
                SET 
                    quantity_on_hand = quantity_on_hand + @QuantityOnHand,
                    last_updated = GETDATE()
                WHERE product_id = @ProductId AND warehouse_id = @WarehouseId";

            await connection.ExecuteAsync(updateInventoryItem, new
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                QuantityOnHand = dto.QuantityProduced
            }, transaction);

            transaction.Commit();

            return result;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

    }

    public async Task<IEnumerable<ProductionOutputListDto>> GetAllAsync()
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string query = @"
            SELECT
                po.id AS Id,
                po.production_order_id AS ProductionOrderId,
                p.name AS ProductName,
                p.unit AS ProductUnit,
                po.quantity_produced AS QuantityProduced,
                w.name AS WarehouseName,
                po.date_produced AS DateProduced
            FROM ProductionOutputs po
            JOIN Products p ON p.id = po.product_id
            JOIN Warehouses w ON w.id = po.warehouse_id
            WHERE 1 = 1";

        var parameters = new DynamicParameters();

        var result = await connection.QueryAsync<ProductionOutputListDto>(query, parameters);

        foreach (var item in result)
        {
            item.ProductionOrderReceiptNumber = $"PRO-{item.ProductionOrderId.ToString().PadLeft(5, '0')}";
        }

        return result;
    }

    public async Task<ProductionOutputDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        var transaction = connection.BeginTransaction();
        
        try
        {
            const string query = @"
                SELECT
                    po.id AS Id,
                    po.production_order_id AS ProductionOrderId,
                    po.product_id AS ProductId,
                    p.name AS ProductName,
                    po.quantity_produced AS QuantityProduced,
                    po.warehouse_id AS WarehouseId,
                    w.name AS WarehouseName,
                    po.date_produced AS DateProduced
                FROM ProductionOutputs po
                JOIN Products p ON p.id = po.product_id
                JOIN Warehouses w ON w.id = po.warehouse_id
                WHERE po.id = @id";

            var result = await connection.QueryFirstOrDefaultAsync<ProductionOutputDto>(query, new
            {
                id
            }, transaction);

            transaction.Commit();

            return result;
        } 
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, ProductionOutputDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        var rowsAffected = 0;

        try
        {

            var referenceDocument = $"GO-{id.ToString().PadLeft(5, '0')}";

            const string getOldQuery = @"
                SELECT
                    quantity_produced
                FROM ProductionOutputs
                WHERE id = @id";

            var oldQuantity = await connection.QueryFirstOrDefaultAsync<int?>(getOldQuery, new
            {
                id
            }, transaction);

            if (oldQuantity != null)
            {
                const string restoreInventoryItem = @"
                    UPDATE InventoryItems 
                    SET 
                        quantity_on_hand = quantity_on_hand - @QuantityToRestore,
                        last_updated = GETDATE()
                    WHERE product_id = @ProductId AND warehouse_id = @OriginalWarehouseId";

                await connection.ExecuteAsync(restoreInventoryItem, new
                {
                    ProductId = dto.ProductId,
                    OriginalWarehouseId = dto.WarehouseId, 
                    QuantityToRestore = oldQuantity
                }, transaction);
            }

            var deleteStockMovementQuery = "DELETE FROM StockMovements WHERE reference_document = @ReferenceDocument";
            await connection.ExecuteAsync(deleteStockMovementQuery, new { ReferenceDocument = referenceDocument }, transaction);

            const string update = @"
                UPDATE ProductionOutputs
                SET
                    production_order_id = @ProductionOrderId,
                    product_id = @ProductId,
                    quantity_produced = @QuantityProduced,
                    warehouse_id = @WarehouseId,
                    date_produced = @DateProduced
                WHERE id = @id";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@id", id);

            rowsAffected = await connection.ExecuteAsync(update, parameters, transaction);

            const string insertStockMovement = @"
                INSERT INTO StockMovements (
                    product_id, 
                    to_warehouse_id, 
                    movement_type, 
                    quantity, 
                    reference_document, 
                    movement_date
                )
                VALUES (
                    @ProductId, 
                    @ToWarehouseId, 
                    'in', 
                    @Quantity, 
                    @ReferenceDocument, 
                    @MovementDate
                )";

            await connection.ExecuteAsync(insertStockMovement, new
            {
                ProductId = dto.ProductId,
                ToWarehouseId = dto.WarehouseId,
                Quantity = dto.QuantityProduced,
                ReferenceDocument = referenceDocument,
                MovementDate = dto.DateProduced
            }, transaction);


            const string updateInventoryItem = @"
                UPDATE InventoryItems 
                SET 
                    quantity_on_hand = quantity_on_hand + @QuantityOnHand,
                    last_updated = GETDATE()
                WHERE product_id = @ProductId AND warehouse_id = @WarehouseId";

            await connection.ExecuteAsync(updateInventoryItem, new
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                QuantityOnHand = dto.QuantityProduced
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

/*     public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        var transaction = connection.BeginTransaction();

        try
        {
            const string delete = @"
                DELETE FROM ProductionOutputs 
                WHERE id = @id";

            var rowsAffected = await connection.ExecuteAsync(delete, new
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
    } */

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string getDataQuery = @"
                SELECT 
                    product_id AS ProductId, 
                    warehouse_id AS WarehouseId, 
                    quantity_produced AS Quantity 
                FROM ProductionOutputs 
                WHERE id = @id";

            var outputData = await connection.QueryFirstOrDefaultAsync<dynamic>(getDataQuery, new { id }, transaction);

            if (outputData != null)
            {
                const string reduceInventory = @"
                    UPDATE InventoryItems 
                    SET 
                        quantity_on_hand = quantity_on_hand - @Quantity,
                        last_updated = GETDATE()
                    WHERE product_id = @ProductId AND warehouse_id = @WarehouseId";

                await connection.ExecuteAsync(reduceInventory, new
                {
                    ProductId = outputData.ProductId,
                    WarehouseId = outputData.WarehouseId,
                    Quantity = outputData.Quantity
                }, transaction);

                var referenceDocument = $"GO-{id.ToString().PadLeft(5, '0')}"; 

                const string deleteStockMovement = @"
                    DELETE FROM StockMovements 
                    WHERE reference_document = @ReferenceDocument";

                await connection.ExecuteAsync(deleteStockMovement, new 
                { 
                    ReferenceDocument = referenceDocument 
                }, transaction);
            }

            const string deleteProductionOutput = @"
                DELETE FROM ProductionOutputs 
                WHERE id = @id";

            var rowsAffected = await connection.ExecuteAsync(deleteProductionOutput, new
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