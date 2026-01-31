using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;
using System.Security.AccessControl;

namespace NavetraERP.Services;

public class SalesOrderService
{

    private readonly IConfiguration _config;

    public SalesOrderService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateSalesOrderDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertSalesOrder = @"
                INSERT INTO SalesOrders (
                    customer_id,
                    order_date,
                    required_delivery_date,
                    status,
                    total_amount
                )
                VALUES (
                    @CustomerId,
                    @OrderDate,
                    @RequiredDeliveryDate,
                    @Status,
                    @TotalAmount
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var result = await connection.ExecuteScalarAsync<int>(insertSalesOrder, dto, transaction);
            
            foreach (var item in dto.Items)
            {
                const string insertSalesOrderItem = @"
                    INSERT INTO SalesOrderItems (
                        sales_order_id,
                        product_id,
                        quantity_ordered,
                        shipped_quantity,
                        unit_price,
                        discount,
                        tax_rate
                    )
                    VALUES (
                        @SalesOrderId,
                        @ProductId,
                        @QuantityOrdered,
                        @QuantityShipped,
                        @PricePerUnit,
                        @Discount,
                        @TaxRate
                    )";

                var itemResult = await connection.ExecuteScalarAsync<int>(insertSalesOrderItem, new
                {
                    SalesOrderId = result,
                    ProductId = item.ProductId,
                    QuantityOrdered = item.QuantityOrdered,
                    QuantityShipped = item.QuantityShipped,
                    PricePerUnit = item.PricePerUnit,
                    Discount = item.Discount,
                    TaxRate = item.TaxRate
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
                    ProductId = item.ProductId,
                    FromWarehouseId = dto.WarehouseId,
                    Quantity = item.QuantityShipped,
                    ReferenceDocument = $"SO-{result.ToString().PadLeft(5, '0')}",
                    MovementDate = dto.OrderDate
                }, transaction);

                const string updateInventoryItem = @"
                    UPDATE InventoryItems 
                    SET 
                        quantity_on_hand = quantity_on_hand - @QuantityOnHand,
                        last_updated = GETDATE()
                    WHERE product_id = @ProductId AND warehouse_id = @WarehouseId";

                await connection.ExecuteAsync(updateInventoryItem, new
                {
                    ProductId = item.ProductId,
                    WarehouseId = dto.WarehouseId,
                    QuantityOnHand = item.QuantityShipped
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

    public async Task<IEnumerable<SalesOrderListDto>> GetAllAsync(string? receiptNumber = null, DateTime? orderDate = null, string? status = null)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string query = @"
            SELECT
                id AS Id,
                order_date AS OrderDate,
                required_delivery_date AS RequiredDeliveryDate,
                status AS Status
            FROM SalesOrders
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
                query += " AND id = @Id";
                parameters.Add("@Id", searchedId.Value);
            }
        }

        if (orderDate.HasValue)
        {
            query += " AND order_date = @OrderDate";
            parameters.Add("@OrderDate", orderDate.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query += " AND status = @Status";
            parameters.Add("@Status", status);
        }

        var result = await connection.QueryAsync<SalesOrderListDto>(query, parameters);

        foreach (var item in result)
        {
            item.ReceiptNumber = $"SO-{item.Id.ToString().PadLeft(5, '0')}";
        }
            

        return result;
    }

    public async Task<SalesOrderDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        var transaction = connection.BeginTransaction();
        
        try
        {
            var referenceDocument = $"SO-{id.ToString().PadLeft(5, '0')}";

            const string salesOrderQuery = @"
                SELECT
                    so.id AS Id,
                    c.id AS CustomerId,
                    c.name AS CustomerName,
                    c.tax_number AS CustomerTaxNumber,
                    ba.country + ', ' + ba.region AS CustomerBillingAddress_1,
                    ba.post_code + ' ' + ba.city + ', ' + ba.address_1 + ' ' + ISNULL(ba.address_2, '') AS CustomerBillingAddress_2,
                    sa.country + ', ' + sa.region AS CustomerShippingAddress_1,
                    sa.post_code + ' ' + sa.city + ', ' + sa.address_1 + ' ' + ISNULL(sa.address_2, '') AS CustomerShippingAddress_2,
                    so.order_date AS OrderDate,
                    so.required_delivery_date AS RequiredDeliveryDate,
                    so.status AS Status,
                    so.total_amount AS TotalAmount,
                    sm.from_warehouse_id AS WarehouseId
                FROM SalesOrders so
                JOIN Customers c ON c.id = so.customer_id
                JOIN HR_Addresses ba ON ba.id = c.billing_address_id
                JOIN HR_Addresses sa ON sa.id = c.shipping_address_id
                JOIN StockMovements sm ON sm.reference_document = @ReferenceDocument
                WHERE so.id = @id";

            var result = await connection.QueryFirstOrDefaultAsync<SalesOrderDto>(salesOrderQuery, new
            {
                ReferenceDocument = referenceDocument,
                id,
            }, transaction);

            if (result != null)
            {
                result.ReceiptNumber = $"SO-{id.ToString().PadLeft(5, '0')}";
            }

            const string salesOrderItemsQuery = @"
                SELECT
                    soi.sales_order_id AS SalesOrderId,
                    soi.product_id AS ProductId,
                    soi.quantity_ordered AS QuantityOrdered,
                    soi.shipped_quantity AS QuantityShipped,
                    soi.unit_price AS PricePerUnit,
                    (soi.unit_price - (soi.unit_price * soi.discount/100)) AS PricePerUnitWithDiscount,
                    (soi.unit_price * soi.shipped_quantity) * (100 - soi.discount)/100 AS TotalPrice,
                    soi.discount AS Discount,
                    soi.tax_rate AS TaxRate,
                    p.sku AS ProductSku,
                    p.name AS ProductName,
                    p.unit AS ProductUnit
                FROM SalesOrderItems soi
                JOIN Products p ON p.id = soi.product_id
                WHERE sales_order_id = @id";

            var items = (await connection.QueryAsync<SalesOrderItemDto>(salesOrderItemsQuery, new
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

/*     public async Task<bool> UpdateAsync(int id, SalesOrderDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        var rowsAffected = 0;

        try
        {

            //Törzsadatok módosítása

            const string updateSalesOrder = @"
                UPDATE SalesOrders
                SET
                    customer_id = @CustomerId,
                    order_date = @OrderDate,
                    required_delivery_date = @RequiredDeliveryDate,
                    status = @Status,
                    total_amount = @TotalAmount
                WHERE id = @id";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@id", id);

            rowsAffected = await connection.ExecuteAsync(updateSalesOrder, parameters, transaction);

            if (dto.Items != null)
            {
                var deleteSalesOrderItemQuery = "DELETE FROM SalesOrderItems WHERE sales_order_id = @id";

                await connection.ExecuteAsync(deleteSalesOrderItemQuery, new
                {
                    id
                }, transaction);

                var referenceDocument = $"SO-{id.ToString().PadLeft(5, '0')}";

                var deleteStockMovementQuery = "DELETE FROM StockMovements WHERE reference_document = @ReferenceDocument";

                await connection.ExecuteAsync(deleteStockMovementQuery, new
                {
                    ReferenceDocument = referenceDocument
                }, transaction);

                foreach (var item in dto.Items)
                {
                    const string insertSalesOrderItem = @"
                        INSERT INTO SalesOrderItems (
                            sales_order_id,
                            product_id,
                            quantity_ordered,
                            shipped_quantity,
                            unit_price,
                            discount,
                            tax_rate
                        )
                        VALUES (
                            @SalesOrderId,
                            @ProductId,
                            @QuantityOrdered,
                            @QuantityShipped,
                            @PricePerUnit,
                            @Discount,
                            @TaxRate
                        )";

                    var itemResult = await connection.ExecuteScalarAsync<int>(insertSalesOrderItem, new
                    {
                        SalesOrderId = id,
                        ProductId = item.ProductId,
                        QuantityOrdered = item.QuantityOrdered,
                        QuantityShipped = item.QuantityShipped,
                        PricePerUnit = item.PricePerUnit,
                        Discount = item.Discount,
                        TaxRate = item.TaxRate
                    }, transaction);

                    const string insertStockMovement = @"
                        INSERT INTO StockMovements (
                            product_id,
                            movement_type,
                            quantity,
                            reference_document,
                            movement_date
                        )
                        VALUES (
                            @ProductId,
                            'out',
                            @Quantity,
                            @ReferenceDocument,
                            @MovementDate
                        )";

                    var stockMovementResult = await connection.ExecuteScalarAsync<int>(insertStockMovement, new
                    {
                        ProductId = item.ProductId,
                        Quantity = item.QuantityShipped,
                        ReferenceDocument = $"SO-{id.ToString().PadLeft(5, '0')}",
                        MovementDate = dto.OrderDate
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

    public async Task<bool> UpdateAsync(int id, SalesOrderDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        var rowsAffected = 0;

        try
        {
            var referenceDocument = $"SO-{id.ToString().PadLeft(5, '0')}";

            // ---------------------------------------------------------
            // 0. LÉPÉS: Megkeressük, melyik raktárból ment ki az áru EREDETILEG
            // ---------------------------------------------------------
            // Mivel a SalesOrder nem tárolja a raktárat, a készletmozgásokból bányásszuk elő.
            const string getOriginalWarehouseQuery = @"
                SELECT TOP 1 from_warehouse_id 
                FROM StockMovements 
                WHERE reference_document = @ReferenceDocument";

            var originalWarehouseId = await connection.ExecuteScalarAsync<int?>(
                getOriginalWarehouseQuery, 
                new { ReferenceDocument = referenceDocument }, 
                transaction
            );

            // ---------------------------------------------------------
            // 1. LÉPÉS: A régi tételek lekérdezése
            // ---------------------------------------------------------
            const string getOldItemsQuery = @"
                SELECT product_id AS ProductId, shipped_quantity AS QuantityShipped 
                FROM SalesOrderItems 
                WHERE sales_order_id = @id";

            var oldItems = await connection.QueryAsync<SalesOrderItemDto>(getOldItemsQuery, new { id }, transaction);

            // ---------------------------------------------------------
            // 2. LÉPÉS: Kompenzálás (CSAK akkor, ha megvan az eredeti raktár)
            // ---------------------------------------------------------
            if (originalWarehouseId.HasValue && oldItems.Any())
            {
                const string restoreInventoryItem = @"
                    UPDATE InventoryItems 
                    SET 
                        quantity_on_hand = quantity_on_hand + @QuantityToRestore,
                        last_updated = GETDATE()
                    WHERE product_id = @ProductId AND warehouse_id = @OriginalWarehouseId"; // Itt a trükk!

                foreach (var oldItem in oldItems)
                {
                    // Itt az EREDETI raktárba töltjük vissza!
                    await connection.ExecuteAsync(restoreInventoryItem, new
                    {
                        ProductId = oldItem.ProductId,
                        OriginalWarehouseId = originalWarehouseId.Value, 
                        QuantityToRestore = oldItem.QuantityShipped
                    }, transaction);
                }
            }

            // ---------------------------------------------------------
            // 3. LÉPÉS: SalesOrder fejléc update (ez maradt)
            // ---------------------------------------------------------
            const string updateSalesOrder = @"
                UPDATE SalesOrders
                SET
                    customer_id = @CustomerId,
                    order_date = @OrderDate,
                    required_delivery_date = @RequiredDeliveryDate,
                    status = @Status,
                    total_amount = @TotalAmount
                WHERE id = @id";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@id", id);
            rowsAffected = await connection.ExecuteAsync(updateSalesOrder, parameters, transaction);

            if (dto.Items != null)
            {
                // ---------------------------------------------------------
                // 4. LÉPÉS: Régi adatok törlése
                // ---------------------------------------------------------
                var deleteSalesOrderItemQuery = "DELETE FROM SalesOrderItems WHERE sales_order_id = @id";
                await connection.ExecuteAsync(deleteSalesOrderItemQuery, new { id }, transaction);

                var deleteStockMovementQuery = "DELETE FROM StockMovements WHERE reference_document = @ReferenceDocument";
                await connection.ExecuteAsync(deleteStockMovementQuery, new { ReferenceDocument = referenceDocument }, transaction);

                // ---------------------------------------------------------
                // 5. LÉPÉS: Új tételek beszúrása és ÚJ raktárból levonás
                // ---------------------------------------------------------
                foreach (var item in dto.Items)
                {
                    // ... (SalesOrderItem INSERT ugyanaz, mint előbb) ...
                    const string insertSalesOrderItem = @"
                        INSERT INTO SalesOrderItems (sales_order_id, product_id, quantity_ordered, shipped_quantity, unit_price, discount, tax_rate)
                        VALUES (@SalesOrderId, @ProductId, @QuantityOrdered, @QuantityShipped, @PricePerUnit, @Discount, @TaxRate)";
                    
                    await connection.ExecuteAsync(insertSalesOrderItem, new { SalesOrderId = id, ProductId = item.ProductId, QuantityOrdered = item.QuantityOrdered, QuantityShipped = item.QuantityShipped, PricePerUnit = item.PricePerUnit, Discount = item.Discount, TaxRate = item.TaxRate }, transaction);


                    // ... (StockMovement INSERT - Itt már a dto.WarehouseId-t használjuk!) ...
                    const string insertStockMovement = @"
                        INSERT INTO StockMovements (product_id, from_warehouse_id, movement_type, quantity, reference_document, movement_date)
                        VALUES (@ProductId, @FromWarehouseId, 'out', @Quantity, @ReferenceDocument, @MovementDate)";

                    await connection.ExecuteAsync(insertStockMovement, new
                    {
                        ProductId = item.ProductId,
                        FromWarehouseId = dto.WarehouseId, // Ez az ÚJ raktár ID
                        Quantity = item.QuantityShipped,
                        ReferenceDocument = referenceDocument,
                        MovementDate = dto.OrderDate
                    }, transaction);


                    // ... (Inventory levonás - Ez is az ÚJ raktárból megy) ...
                    const string updateInventoryItem = @"
                        UPDATE InventoryItems 
                        SET 
                            quantity_on_hand = quantity_on_hand - @QuantityOnHand,
                            last_updated = GETDATE()
                        WHERE product_id = @ProductId AND warehouse_id = @WarehouseId";

                    await connection.ExecuteAsync(updateInventoryItem, new
                    {
                        ProductId = item.ProductId,
                        WarehouseId = dto.WarehouseId, // Ez az ÚJ raktár ID
                        QuantityOnHand = item.QuantityShipped
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

            const string deleteSalesOrderItems = @"
                DELETE FROM SalesOrderItems 
                WHERE sales_order_id = @id";

            var rowsAffected = await connection.ExecuteAsync(deleteSalesOrderItems, new
            {
                id
            }, transaction);

            const string deleteSalesOrder = @"
                DELETE FROM SalesOrders 
                WHERE id = @id";

            rowsAffected = await connection.ExecuteAsync(deleteSalesOrder, new
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
            var referenceDocument = $"SO-{id.ToString().PadLeft(5, '0')}";

            // ---------------------------------------------------------
            // 1. LÉPÉS: Megkeressük a raktárat a mozgások alapján
            // ---------------------------------------------------------
            const string getWarehouseQuery = @"
                SELECT TOP 1 
                    from_warehouse_id 
                FROM StockMovements 
                WHERE reference_document = @ReferenceDocument";

            var warehouseId = await connection.ExecuteScalarAsync<int?>(
                getWarehouseQuery, 
                new { ReferenceDocument = referenceDocument }, 
                transaction
            );

            // ---------------------------------------------------------
            // 2. LÉPÉS: Lekérjük a törlendő tételeket (hogy tudjuk, mit kell visszatölteni)
            // ---------------------------------------------------------
            const string getItemsQuery = @"
                SELECT 
                    product_id AS ProductId, 
                    shipped_quantity AS QuantityShipped 
                FROM SalesOrderItems 
                WHERE sales_order_id = @id";

            var items = await connection.QueryAsync<SalesOrderItemDto>(getItemsQuery, new { id }, transaction);

            // ---------------------------------------------------------
            // 3. LÉPÉS: Készlet visszatöltése (Kompenzálás)
            // ---------------------------------------------------------
            // Csak akkor, ha volt mozgás és van mit visszatölteni
            if (warehouseId.HasValue && items.Any())
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
                        ProductId = item.ProductId,
                        WarehouseId = warehouseId.Value,
                        QuantityToRestore = item.QuantityShipped
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
            const string deleteSalesOrderItems = @"
                DELETE FROM SalesOrderItems 
                WHERE sales_order_id = @id";

            await connection.ExecuteAsync(deleteSalesOrderItems, new { id }, transaction);

            // ---------------------------------------------------------
            // 6. LÉPÉS: Rendelés fejléc törlése
            // ---------------------------------------------------------
            const string deleteSalesOrder = @"
                DELETE FROM SalesOrders 
                WHERE id = @id";

            var rowsAffected = await connection.ExecuteAsync(deleteSalesOrder, new { id }, transaction);

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