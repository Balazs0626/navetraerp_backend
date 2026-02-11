using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;

namespace NavetraERP.Services;

/*
    GoodsReceiptService, SalesOrderService is hozzáfér a táblához
*/

public class StockMovementService
{

    private readonly IConfiguration _config;

    public StockMovementService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateStockMovementDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string insert = @"
                INSERT INTO StockMovements (
                    product_id,
                    from_warehouse_id,
                    to_warehouse_id,
                    movement_type,
                    quantity,
                    reference_document,
                    movement_date,
                    performed_by_id
                )
                VALUES (
                    @ProductId,
                    @FromWarehouseId,
                    @ToWarehouseId,
                    @MovementType,
                    @Quantity,
                    @ReferenceDocument,
                    @MovementDate,
                    @PerformedById
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var parameters = new DynamicParameters(dto);

            var result = await connection.ExecuteScalarAsync<int>(insert, parameters, transaction);

            if (dto.MovementType == "transfer")
            {
                const string updateFromInventoryItem = @"
                    UPDATE InventoryItems 
                    SET 
                        quantity_on_hand = quantity_on_hand - @QuantityOnHand,
                        last_updated = GETDATE()
                    WHERE product_id = @ProductId AND warehouse_id = @WarehouseId";

                await connection.ExecuteAsync(updateFromInventoryItem, new
                {
                    ProductId = dto.ProductId,
                    WarehouseId = dto.FromWarehouseId,
                    QuantityOnHand = dto.Quantity
                }, transaction);

                const string getBatchNumberQuery = @"
                    SELECT TOP 1
                        batch_number
                    FROM InventoryItems
                    WHERE warehouse_id = @WarehouseId AND product_id = @ProductId";

                var batchNumber = await connection.QueryFirstOrDefaultAsync<string?>(getBatchNumberQuery, new
                {
                    WarehouseId = dto.FromWarehouseId,
                    ProductId = dto.ProductId
                }, transaction);

                const string upsertInventoryItem = @"
                    IF EXISTS (SELECT 1 FROM InventoryItems WHERE product_id = @ProductId AND warehouse_id = @WarehouseId)
                    BEGIN
                        UPDATE InventoryItems 
                        SET 
                            quantity_on_hand = quantity_on_hand + @QuantityOnHand,
                            last_updated = GETDATE()
                        WHERE product_id = @ProductId AND warehouse_id = @WarehouseId
                    END
                    ELSE
                    BEGIN
                        INSERT INTO InventoryItems (
                            product_id, 
                            warehouse_id, 
                            quantity_on_hand,
                            reorder_level,
                            batch_number,
                            last_updated
                        )
                        VALUES (
                            @ProductId, 
                            @WarehouseId, 
                            @QuantityOnHand,
                            0,
                            @BatchNumber,
                            GETDATE()
                        )
                    END";

                await connection.ExecuteAsync(upsertInventoryItem, new
                {
                    ProductId = dto.ProductId,
                    WarehouseId = dto.ToWarehouseId,
                    QuantityOnHand = dto.Quantity,
                    BatchNumber = batchNumber
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

    public async Task<IEnumerable<StockMovementListDto>> GetAllAsync(int? productId = null, string? referenceDocument = null, string? movementType = null, DateTime? movementDate = null, DateTime? movementDateGte = null)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string query = @"
            SELECT
                sm.id AS Id,
                sm.product_id AS ProductId,
                p.name AS ProductName,
                sm.reference_document AS ReferenceDocument,
                sm.movement_date AS MovementDate,
                sm.movement_type AS MovementType
            FROM StockMovements sm
            JOIN Products p ON p.id = sm.product_id
            WHERE 1 = 1";

        var parameters = new DynamicParameters();

        if (productId != null)
        {
            query += " AND sm.product_id = @ProductId";
            parameters.Add("@ProductId", productId);
        }

        if (movementDate.HasValue)
        {
            query += " AND sm.movement_date = @MovementDate";
            parameters.Add("@MovementDate", movementDate.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(referenceDocument))
        {
            query += " AND sm.reference_document LIKE @ReferenceDocument";
            parameters.Add("@ReferenceDocument", $"%{referenceDocument}%");
        }


        if (!string.IsNullOrWhiteSpace(movementType))
        {
            query += " AND sm.movement_type = @MovementType";
            parameters.Add("@MovementType", movementType);
        }

        if (movementDateGte.HasValue)
        {
            query += " AND sm.movement_date >= @StartDate";
            parameters.Add("StartDate", movementDateGte.Value.Date);
        }

        var result = await connection.QueryAsync<StockMovementListDto>(query, parameters);

        return result;
    }

    public async Task<StockMovementDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                sm.id AS Id,
                sm.product_id AS ProductId,
                p.name AS ProductName,
                p.unit AS ProductUnit,
                sm.from_warehouse_id AS FromWarehouseId,
                fw.name AS FromWarehouseName,
                sm.to_warehouse_id AS ToWarehouseId,
                tw.name AS ToWarehouseName,
                sm.movement_type AS MovementType,
                sm.quantity AS Quantity,
                sm.reference_document AS ReferenceDocument,
                sm.movement_date AS MovementDate,
                sm.performed_by_id AS PerformedById,
                e.first_name + ' ' + e.last_name AS PerformedByName
            FROM StockMovements sm
            JOIN Products p ON p.id = sm.product_id
            LEFT JOIN Warehouses fw ON fw.id = sm.from_warehouse_id
            LEFT JOIN Warehouses tw ON tw.id = sm.to_warehouse_id
            LEFT JOIN HR_Employee e ON e.id = sm.performed_by_id
            WHERE sm.id = @id";

        var result = await connection.QueryFirstOrDefaultAsync<StockMovementDto>(query, new
        {
            id
        });

        return result;
    }

    public async Task<bool> UpdateAsync(int id, StockMovementDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {

            const string getOldDataQuery = @"
            SELECT 
                movement_type, 
                quantity, 
                product_id, 
                from_warehouse_id, 
                to_warehouse_id 
            FROM StockMovements 
            WHERE id = @id";

            var oldMove = await connection.QueryFirstOrDefaultAsync<dynamic>(getOldDataQuery, new 
            { 
                id 
            }, transaction);

            if (oldMove != null && oldMove.movement_type == "transfer")
            {
                // Ha transfer volt, akkor "visszacsináljuk":
                // 1. Ahol levontunk (From), oda visszaadjuk (+)
                if (oldMove.from_warehouse_id != null)
                {
                    await connection.ExecuteAsync(@"
                        UPDATE InventoryItems 
                        SET quantity_on_hand = quantity_on_hand + @Qty, last_updated = GETDATE()
                        WHERE product_id = @Pid AND warehouse_id = @Wid", 
                        new { Qty = oldMove.quantity, Pid = oldMove.product_id, Wid = oldMove.from_warehouse_id }, transaction);
                }

                // 2. Ahol hozzáadtunk (To), onnan levonjuk (-)
                if (oldMove.to_warehouse_id != null)
                {
                    await connection.ExecuteAsync(@"
                        UPDATE InventoryItems 
                        SET quantity_on_hand = quantity_on_hand - @Qty, last_updated = GETDATE()
                        WHERE product_id = @Pid AND warehouse_id = @Wid", 
                        new { Qty = oldMove.quantity, Pid = oldMove.product_id, Wid = oldMove.to_warehouse_id }, transaction);
                }
            }

            const string update = @"
                UPDATE StockMovements
                SET
                    product_id = @ProductId,
                    from_warehouse_id = @FromWarehouseId,
                    to_warehouse_id = @ToWarehouseId,
                    movement_type = @MovementType,
                    quantity = @Quantity,
                    reference_document = @ReferenceDocument,
                    movement_date = @MovementDate,
                    performed_by_id = @PerformedById
                WHERE id = @id";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@id", id);

            var rowsAffected = await connection.ExecuteAsync(update, parameters, transaction);

            if (dto.MovementType == "transfer")
            {
                const string updateFromInventoryItem = @"
                    UPDATE InventoryItems 
                    SET 
                        quantity_on_hand = quantity_on_hand - @QuantityOnHand,
                        last_updated = GETDATE()
                    WHERE product_id = @ProductId AND warehouse_id = @WarehouseId";

                await connection.ExecuteAsync(updateFromInventoryItem, new
                {
                    ProductId = dto.ProductId,
                    WarehouseId = dto.FromWarehouseId,
                    QuantityOnHand = dto.Quantity
                }, transaction);

                const string updateToInventoryItem = @"
                    UPDATE InventoryItems 
                    SET 
                        quantity_on_hand = quantity_on_hand + @QuantityOnHand,
                        last_updated = GETDATE()
                    WHERE product_id = @ProductId AND warehouse_id = @WarehouseId";

                await connection.ExecuteAsync(updateToInventoryItem, new
                {
                    ProductId = dto.ProductId,
                    WarehouseId = dto.ToWarehouseId,
                    QuantityOnHand = dto.Quantity,
                }, transaction);
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

        using var transaction = connection.BeginTransaction();

        try
        {
            const string getOldDataQuery = @"
                SELECT 
                    movement_type, 
                    quantity, 
                    product_id, 
                    from_warehouse_id, 
                    to_warehouse_id 
                FROM StockMovements 
                WHERE id = @id";

            var oldMove = await connection.QueryFirstOrDefaultAsync<dynamic>(getOldDataQuery, new 
            { 
                id 
            }, transaction);

            if (oldMove != null && oldMove.movement_type == "transfer")
            {
                if (oldMove.from_warehouse_id != null)
                {
                    await connection.ExecuteAsync(@"
                        UPDATE InventoryItems 
                        SET quantity_on_hand = quantity_on_hand + @Qty, last_updated = GETDATE()
                        WHERE product_id = @Pid AND warehouse_id = @Wid", 
                        new { Qty = oldMove.quantity, Pid = oldMove.product_id, Wid = oldMove.from_warehouse_id }, transaction);
                }

                if (oldMove.to_warehouse_id != null)
                {
                    await connection.ExecuteAsync(@"
                        UPDATE InventoryItems 
                        SET quantity_on_hand = quantity_on_hand - @Qty, last_updated = GETDATE()
                        WHERE product_id = @Pid AND warehouse_id = @Wid", 
                        new { Qty = oldMove.quantity, Pid = oldMove.product_id, Wid = oldMove.to_warehouse_id }, transaction);
                }
            }

            const string delete = @"
                DELETE FROM StockMovements 
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
    }

}