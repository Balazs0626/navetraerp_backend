using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;
using System.Security.AccessControl;

namespace NavetraERP.Services;

public class ProductionOrderService
{

    private readonly IConfiguration _config;

    public ProductionOrderService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateProductionOrderDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertProductionOrder = @"
                INSERT INTO ProductionOrders (
                    product_id,
                    planned_quantity,
                    start_date,
                    end_date,
                    status,
                    responsible_employee_id
                )
                VALUES (
                    @ProductId,
                    @PlannedQuantity,
                    @StartDate,
                    @EndDate,
                    @Status,
                    @ResponsibleEmployeeId
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var result = await connection.ExecuteScalarAsync<int>(insertProductionOrder, dto, transaction);
            
            foreach (var item in dto.Components)
            {
                const string insertProductionConsumption = @"
                    INSERT INTO ProductionConsumptions (
                        production_order_id,
                        component_product_id,
                        quantity_used,
                        warehouse_id,
                        date_used
                    )
                    VALUES (
                        @ProductionOrderId,
                        @ComponentProductId,
                        @QuantityUsed,
                        @WarehouseId,
                        @DateUsed
                    )";

                var itemResult = await connection.ExecuteScalarAsync<int>(insertProductionConsumption, new
                {
                    ProductionOrderId = result,
                    ComponentProductId = item.ComponentProductId,
                    QuantityUsed = item.QuantityUsed,
                    WarehouseId = item.WarehouseId,
                    DateUsed = item.DateUsed
                }, transaction);

                const string insertStockMovement = @"
                    INSERT INTO StockMovements (
                        product_id,
                        from_warehouse_id,
                        movement_type,
                        quantity,
                        reference_document,
                        movement_date
                    )
                    VALUES (
                        @ProductId,
                        @FromWarehouseId,
                        'out',
                        @Quantity,
                        @ReferenceDocument,
                        @MovementDate
                    )";

                var stockMovementResult = await connection.ExecuteScalarAsync<int>(insertStockMovement, new
                {
                    ProductId = item.ComponentProductId,
                    FromWarehouseId = item.WarehouseId,
                    Quantity = item.QuantityUsed,
                    ReferenceDocument = $"MO-{result.ToString().PadLeft(5, '0')}",
                    MovementDate = dto.StartDate
                }, transaction);

                const string updateInventoryItem = @"
                    UPDATE InventoryItems 
                    SET 
                        quantity_on_hand = quantity_on_hand - @QuantityOnHand,
                        last_updated = GETDATE()
                    WHERE product_id = @ProductId AND warehouse_id = @WarehouseId";

                await connection.ExecuteAsync(updateInventoryItem, new
                {
                    ProductId = item.ComponentProductId,
                    WarehouseId = item.WarehouseId,
                    QuantityOnHand = item.QuantityUsed
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

    public async Task<IEnumerable<ProductionOrderListDto>> GetAllAsync(string? receiptNumber = null, int? product = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string query = @"
            SELECT
                po.id AS Id,
                p.name AS ProductName,
                po.start_date AS StartDate,
                po.end_date AS EndDate,
                po.status AS Status
            FROM ProductionOrders po
            JOIN Products p ON p.id = po.product_id
            WHERE 1 = 1";

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(receiptNumber))
        {
            var parts = receiptNumber.Split('-');
            
            int? searchedId = null;

            if (parts.Length > 1)
            {
                if (int.TryParse(parts[1], out int id)) 
                    searchedId = id;
            }
            else if (int.TryParse(receiptNumber, out int id))
            {
                searchedId = id;
            }

            if (searchedId.HasValue)
            {
                query += " AND po.id = @Id";
                parameters.Add("@Id", searchedId.Value);
            }
        }

        if (product != null)
        {
            query += " AND po.product_id = @Product";
            parameters.Add("@Product", product);
        }

        if (startDate.HasValue)
        {
            query += " AND po.start_date = @StartDate";
            parameters.Add("@StartDate", startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            query += " AND po.end_date = @EndDate";
            parameters.Add("@EndDate", endDate.Value.Date);
        }

        var result = await connection.QueryAsync<ProductionOrderListDto>(query, parameters);

        foreach (var item in result)
        {
            item.ReceiptNumber = $"MO-{item.Id.ToString().PadLeft(5, '0')}";
        }

        return result;
    }

    public async Task<ProductionOrderDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        var transaction = connection.BeginTransaction();
        
        try
        {
            const string productionOrderQuery = @"
                SELECT
                    po.id AS Id,
                    po.product_id AS ProductId,
                    p.name AS ProductName,
                    p.unit AS ProductUnit,
                    po.planned_quantity AS PlannedQuantity,
                    po.start_date AS StartDate,
                    po.end_date AS EndDate,
                    po.status AS Status,
                    po.responsible_employee_id AS ResponsibleEmployeeId,
                    e.first_name + ' ' + e.last_name AS ResponsibleEmployeeName
                FROM ProductionOrders po
                JOIN Products p ON p.id = po.product_id
                JOIN HR_Employee e ON e.id = po.responsible_employee_id
                WHERE po.id = @id";

            var result = await connection.QueryFirstOrDefaultAsync<ProductionOrderDto>(productionOrderQuery, new
            {
                id
            }, transaction);

            if (result != null)
            {
                result.ReceiptNumber = $"MO-{id.ToString().PadLeft(5, '0')}";
            }

            const string productionConsumptionsQuery = @"
                SELECT
                    pc.id AS Id,
                    pc.production_order_id AS ProductionOrderId,
                    pc.component_product_id AS ComponentProductId,
                    p.name AS ComponentProductName,
                    p.unit AS ComponentProductUnit,
                    pc.quantity_used AS QuantityUsed,
                    pc.warehouse_id AS WarehouseId,
                    w.name AS WarehouseName,
                    pc.date_used AS DateUsed
                FROM ProductionConsumptions pc
                JOIN Products p ON p.id = pc.component_product_id
                JOIN Warehouses w On w.id = pc.warehouse_id
                WHERE pc.production_order_id = @id";

            var components = (await connection.QueryAsync<ProductionConsumptionDto>(productionConsumptionsQuery, new
            {
                id
            }, transaction)).ToList();

            result.Components = components;

            transaction.Commit();

            return result;
        } 
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    /* public async Task<bool> UpdateAsync(int id, ProductionOrderDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        var rowsAffected = 0;

        try
        {
            const string updateProductionOrder = @"
                UPDATE ProductionOrders
                SET
                    product_id = @ProductId,
                    planned_quantity = @PlannedQuantity,
                    start_date = @StartDate,
                    end_date = @EndDate,
                    status = @Status,
                    responsible_employee_id = @ResponsibleEmployeeId
                WHERE id = @id";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@id", id);

            rowsAffected = await connection.ExecuteAsync(updateProductionOrder, parameters, transaction);

            if (dto.Components != null)
            {
                var deleteQuery = "DELETE FROM ProductionConsumptions WHERE production_order_id = @id";

                await connection.ExecuteAsync(deleteQuery, new
                {
                    id
                }, transaction);

                foreach (var item in dto.Components)
                {
                    const string insertProductionConsumption = @"
                        INSERT INTO ProductionConsumptions (
                            production_order_id,
                            component_product_id,
                            quantity_used,
                            warehouse_id,
                            date_used
                        )
                        VALUES (
                            @ProductionOrderId,
                            @ComponentProductId,
                            @QuantityUsed,
                            @WarehouseId,
                            @DateUsed
                        )";

                    var itemResult = await connection.ExecuteScalarAsync<int>(insertProductionConsumption, new
                    {
                        ProductionOrderId = id,
                        ComponentProductId = item.ComponentProductId,
                        QuantityUsed = item.QuantityUsed,
                        WarehouseId = item.WarehouseId,
                        DateUsed = item.DateUsed
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
    } */

    public async Task<bool> UpdateAsync(int id, ProductionOrderDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        var rowsAffected = 0;

        try
        {
            var referenceDocument = $"MO-{id.ToString().PadLeft(5, '0')}";

            const string getOldItemsQuery = @"
                SELECT 
                    component_product_id AS ComponentProductId, 
                    quantity_used AS QuantityUsed,
                    warehouse_id AS WarehouseId
                FROM ProductionConsumptions
                WHERE production_order_id = @id";

            var oldItems = await connection.QueryAsync<ProductionConsumptionDto>(getOldItemsQuery, new { id }, transaction);

            if (oldItems.Any())
            {
                const string restoreInventoryItem = @"
                    UPDATE InventoryItems 
                    SET 
                        quantity_on_hand = quantity_on_hand + @QuantityToRestore,
                        last_updated = GETDATE()
                    WHERE product_id = @ProductId AND warehouse_id = @OriginalWarehouseId";

                foreach (var oldItem in oldItems)
                {
                    await connection.ExecuteAsync(restoreInventoryItem, new
                    {
                        ProductId = oldItem.ComponentProductId,
                        OriginalWarehouseId = oldItem.WarehouseId, 
                        QuantityToRestore = oldItem.QuantityUsed
                    }, transaction);
                }
            }

            const string updateProductionOrder = @"
                UPDATE ProductionOrders
                SET
                    product_id = @ProductId,
                    planned_quantity = @PlannedQuantity,
                    start_date = @StartDate,
                    end_date = @EndDate,
                    status = @Status,
                    responsible_employee_id = @ResponsibleEmployeeId
                WHERE id = @id";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@id", id);
            rowsAffected = await connection.ExecuteAsync(updateProductionOrder, parameters, transaction);

            if (dto.Components != null)
            {
                var deleteSalesOrderItemQuery = "DELETE FROM ProductionConsumptions WHERE production_order_id = @id";
                await connection.ExecuteAsync(deleteSalesOrderItemQuery, new { id }, transaction);

                var deleteStockMovementQuery = "DELETE FROM StockMovements WHERE reference_document = @ReferenceDocument";
                await connection.ExecuteAsync(deleteStockMovementQuery, new { ReferenceDocument = referenceDocument }, transaction);

                foreach (var item in dto.Components)
                {
                    const string insertProductionConsumption = @"
                    INSERT INTO ProductionConsumptions (
                        production_order_id,
                        component_product_id,
                        quantity_used,
                        warehouse_id,
                        date_used
                    )
                    VALUES (
                        @ProductionOrderId,
                        @ComponentProductId,
                        @QuantityUsed,
                        @WarehouseId,
                        @DateUsed
                    )";

                    var itemResult = await connection.ExecuteScalarAsync<int>(insertProductionConsumption, new
                    {
                        ProductionOrderId = id,
                        ComponentProductId = item.ComponentProductId,
                        QuantityUsed = item.QuantityUsed,
                        WarehouseId = item.WarehouseId,
                        DateUsed = item.DateUsed
                    }, transaction);

                    const string insertStockMovement = @"
                        INSERT INTO StockMovements (product_id, from_warehouse_id, movement_type, quantity, reference_document, movement_date)
                        VALUES (@ProductId, @FromWarehouseId, 'out', @Quantity, @ReferenceDocument, @MovementDate)";

                    await connection.ExecuteAsync(insertStockMovement, new
                    {
                        ProductId = item.ComponentProductId,
                        FromWarehouseId = item.WarehouseId,
                        Quantity = item.QuantityUsed,
                        ReferenceDocument = referenceDocument,
                        MovementDate = dto.StartDate
                    }, transaction);

                    const string updateInventoryItem = @"
                        UPDATE InventoryItems 
                        SET 
                            quantity_on_hand = quantity_on_hand - @QuantityOnHand,
                            last_updated = GETDATE()
                        WHERE product_id = @ProductId AND warehouse_id = @WarehouseId";

                    await connection.ExecuteAsync(updateInventoryItem, new
                    {
                        ProductId = item.ComponentProductId,
                        WarehouseId = item.WarehouseId,
                        QuantityOnHand = item.QuantityUsed
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

/*     public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        var transaction = connection.BeginTransaction();

        try
        {

            const string deleteProductionConsumption = @"
                DELETE FROM ProductionConsumptions 
                WHERE production_order_id = @id";

            var rowsAffected = await connection.ExecuteAsync(deleteProductionConsumption, new
            {
                id
            }, transaction);

            const string deleteProductionOrder = @"
                DELETE FROM ProductionOrders 
                WHERE id = @id";

            rowsAffected = await connection.ExecuteAsync(deleteProductionOrder, new
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
            var referenceDocument = $"MO-{id.ToString().PadLeft(5, '0')}";

            const string getItemsQuery = @"
                SELECT 
                    component_product_id AS ComponentProductId, 
                    quantity_used AS QuantityUsed,
                    warehouse_id AS WarehouseId
                FROM ProductionConsumptions 
                WHERE production_order_id = @id";

            var items = await connection.QueryAsync<ProductionConsumptionDto>(getItemsQuery, new { id }, transaction);

            // ---------------------------------------------------------
            // 3. LÉPÉS: Készlet visszatöltése (Kompenzálás)
            // ---------------------------------------------------------
            // Csak akkor, ha volt mozgás és van mit visszatölteni
            if (items.Any())
            {
                const string restoreInventoryItem = @"
                    UPDATE InventoryItems 
                    SET 
                        quantity_on_hand = quantity_on_hand + @QuantityToRestore,
                        last_updated = GETDATE()
                    WHERE product_id = @ProductId AND warehouse_id = @WarehouseId";

                foreach (var item in items)
                {
                    await connection.ExecuteAsync(restoreInventoryItem, new
                    {
                        ProductId = item.ComponentProductId,
                        WarehouseId = item.WarehouseId,
                        QuantityToRestore = item.QuantityUsed
                    }, transaction);
                }
            }

            // ---------------------------------------------------------
            // 4. LÉPÉS: Készletmozgás napló törlése
            // ---------------------------------------------------------
            // Ha nem törlöd ki, ott maradnak "árva" rekordok, amik egy nem létező rendelésre hivatkoznak.
            const string deleteStockMovements = @"
                DELETE FROM StockMovements 
                WHERE reference_document = @ReferenceDocument";

            await connection.ExecuteAsync(deleteStockMovements, new { ReferenceDocument = referenceDocument }, transaction);

            // ---------------------------------------------------------
            // 5. LÉPÉS: Rendelés tételek törlése
            // ---------------------------------------------------------
            const string deleteProductionConsumption = @"
                DELETE FROM ProductionConsumptions 
                WHERE production_order_id = @id";

            await connection.ExecuteAsync(deleteProductionConsumption, new { id }, transaction);

            // ---------------------------------------------------------
            // 6. LÉPÉS: Rendelés fejléc törlése
            // ---------------------------------------------------------
            const string deleteProductionOrder = @"
                DELETE FROM ProductionOrders 
                WHERE id = @id";

            var rowsAffected = await connection.ExecuteAsync(deleteProductionOrder, new { id }, transaction);

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