using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;

namespace NavetraERP.Services;

public class GoodsReceiptService
{

    private readonly IConfiguration _config;

    public GoodsReceiptService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateGoodsReceiptDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertGoodsReceipt = @"
                INSERT INTO GoodsReceipts (
                    purchase_order_id,
                    warehouse_id,
                    receipt_date,
                    received_by_employee_id
                )
                VALUES (
                    @PurchaseOrderId,
                    @WarehouseId,
                    @ReceiptDate,
                    @ReceivedBy
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var result = await connection.ExecuteScalarAsync<int>(insertGoodsReceipt, dto, transaction);
            
            foreach (var item in dto.Items)
            {
                const string insertGoodsReceiptItem = @"
                    INSERT INTO GoodsReceiptItems (
                        goods_receipt_id,
                        product_id,
                        quantity_received,
                        batch_number
                    )
                    VALUES (
                        @GoodsReceiptId,
                        @ProductId,
                        @QuantityReceived,
                        @BatchNumber
                    )";

                var itemResult = await connection.ExecuteScalarAsync<int>(insertGoodsReceiptItem, new
                {
                    GoodsReceiptId = result,
                    ProductId = item.ProductId,
                    QuantityReceived = item.QuantityReceived,
                    BatchNumber = item.BatchNumber
                }, transaction);

                const string insertStockMovement = @"
                    INSERT INTO StockMovements (
                        product_id,
                        to_warehouse_id,
                        movement_type,
                        quantity,
                        reference_document,
                        movement_date,
                        performed_by_Id
                    )
                    VALUES (
                        @ProductId,
                        @ToWarehouseId,
                        'in',
                        @Quantity,
                        @ReferenceDocument,
                        @MovementDate,
                        @PerformedBy
                    )";

                var stockMovementResult = await connection.ExecuteScalarAsync<int>(insertStockMovement, new
                {
                    ProductId = item.ProductId,
                    ToWarehouseId = dto.WarehouseId,
                    Quantity = item.QuantityReceived,
                    ReferenceDocument = $"GR-{result.ToString().PadLeft(5, '0')}",
                    MovementDate = dto.ReceiptDate,
                    PerformedBy = dto.ReceivedBy
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
                    ProductId = item.ProductId,
                    WarehouseId = dto.WarehouseId,
                    QuantityOnHand = item.QuantityReceived,
                    BatchNumber = item.BatchNumber
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

    public async Task<IEnumerable<GoodsReceiptListDto>> GetAllAsync(string? receiptNumber = null, string? purchaseOrderReceiptNumber = null, int? warehouseId = null, DateTime? date = null)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string query = @"
            SELECT
                gr.id AS Id,
                gr.purchase_order_id AS PurchaseOrderId,
                w.name AS WarehouseName,
                gr.receipt_date AS ReceiptDate
            FROM GoodsReceipts gr
            JOIN Warehouses w ON w.id = gr.warehouse_id
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
                query += " AND gr.id = @Id";
                parameters.Add("@Id", searchedId.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(purchaseOrderReceiptNumber))
        {
            var parts = purchaseOrderReceiptNumber.Split('-');
            
            int? searchedId = null;

            if (parts.Length > 1)
            {
                if (int.TryParse(parts[1], out int id)) 
                    searchedId = id;
            }
            else if (int.TryParse(purchaseOrderReceiptNumber, out int id))
            {
                searchedId = id;
            }

            if (searchedId.HasValue)
            {
                query += " AND gr.purchase_order_id = @PurchaseOrderId";
                parameters.Add("@PurchaseOrderId", searchedId.Value);
            }
        }

        if (warehouseId != null)
        {
            query += " AND gr.warehouse_id = @WarehouseId";
            parameters.Add("@WarehouseId", warehouseId);
        }

        if (date.HasValue)
        {
            query += " AND gr.receipt_date = @Date";
            parameters.Add("@Date", date.Value.Date);
        }

        var result = await connection.QueryAsync<GoodsReceiptListDto>(query, parameters);

        foreach (var item in result)
        {
            item.ReceiptNumber = $"GR-{item.Id.ToString().PadLeft(5, '0')}";
            item.PurchaseOrderReceiptNumber = $"PO-{item.PurchaseOrderId.ToString().PadLeft(5, '0')}";
        }

        return result;
    }

    public async Task<GoodsReceiptDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        var transaction = connection.BeginTransaction();
        
        try
        {
            const string goodsReceiptQuery = @"
                SELECT
                    gr.id AS Id,
                    gr.purchase_order_id AS PurchaseOrderId,
                    w.name AS WarehouseName,
                    a.country + ', ' + a.region AS WarehouseAddress_1,
                    a.post_code + ' ' + a.city + ', ' + a.address_1 + ' ' + a.address_2 AS WarehouseAddress_2,
                    gr.receipt_date AS ReceiptDate,
                    e.first_name + ' ' + e.last_name AS ReceivedByName
                FROM GoodsReceipts gr
                JOIN Warehouses w ON w.id = gr.warehouse_id
                JOIN Addresses a ON a.id = w.address_id
                JOIN Employees e ON e.id = gr.received_by_employee_id
                WHERE gr.id = @id";

            var result = await connection.QueryFirstOrDefaultAsync<GoodsReceiptDto>(goodsReceiptQuery, new
            {
                id
            }, transaction);

            const string goodsReceiptItemsQuery = @"
                SELECT
                    gri.goods_receipt_id AS GoodsReceiptId,
                    gri.product_id AS ProductId,
                    p.sku AS ProductSku,
                    p.name AS ProductName,
                    p.unit AS ProductUnit,
                    gri.quantity_received AS QuantityReceived,
                    gri.batch_number AS BatchNumber
                FROM GoodsReceiptItems gri
                JOIN Products p ON p.id = gri.product_id
                WHERE gri.goods_receipt_id = @id";

            var items = (await connection.QueryAsync<GoodsReceiptItemDto>(goodsReceiptItemsQuery, new
            {
                id
            }, transaction)).ToList();

            if (result != null)
            {
                result.ReceiptNumber = $"GR-{id.ToString().PadLeft(5, '0')}";
                result.PurchaseOrderReceiptNumber = $"PO-{result.PurchaseOrderId.ToString().PadLeft(5, '0')}";
                result.Items = items;
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

/*     public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        var transaction = connection.BeginTransaction();

        try
        {

            const string deleteGoodsReceiptItems = @"
                DELETE FROM GoodsReceiptItems 
                WHERE goods_receipt_id = @id";

            var rowsAffected = await connection.ExecuteAsync(deleteGoodsReceiptItems, new
            {
                id
            }, transaction);

            const string deleteGoodsReceipt = @"
                DELETE FROM GoodsReceipts 
                WHERE id = @id";

            rowsAffected = await connection.ExecuteAsync(deleteGoodsReceipt, new
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
            // A bizonylatszám generálása a kereséshez (pl. GR-00123)
            // FIGYELEM: Ellenőrizd, hogy nálad mi a prefix (GR, REC, stb.)!
            var referenceDocument = $"GR-{id.ToString().PadLeft(5, '0')}";

            // ---------------------------------------------------------
            // 1. LÉPÉS: Megkeressük, melyik raktárba érkezett az áru
            // ---------------------------------------------------------
            // Feltételezzük, hogy a StockMovements táblában a bevételezésnél 
            // a 'to_warehouse_id' vagy 'warehouse_id' van kitöltve.
            // Ha a GoodsReceipts táblában van warehouse_id, azt is lekérheted inkább.
            const string getWarehouseQuery = @"
                SELECT TOP 1 to_warehouse_id 
                FROM StockMovements 
                WHERE reference_document = @ReferenceDocument";

            var warehouseId = await connection.ExecuteScalarAsync<int?>(
                getWarehouseQuery, 
                new { ReferenceDocument = referenceDocument }, 
                transaction
            );

            // ---------------------------------------------------------
            // 2. LÉPÉS: Lekérjük a tételeket, amiket TÖRÖLNI akarunk
            // ---------------------------------------------------------
            const string getItemsQuery = @"
                SELECT product_id AS ProductId, quantity_received AS QuantityReceived 
                FROM GoodsReceiptItems 
                WHERE goods_receipt_id = @id";

            var items = await connection.QueryAsync<dynamic>(getItemsQuery, new { id }, transaction);

            // ---------------------------------------------------------
            // 3. LÉPÉS: Készlet KORRIGÁLÁSA (Levonás!)
            // ---------------------------------------------------------
            if (warehouseId.HasValue && items.Any())
            {
                const string reduceInventoryItem = @"
                    UPDATE InventoryItems 
                    SET 
                        quantity_on_hand = quantity_on_hand - @QuantityToRemove, -- Itt kivonjuk!
                        last_updated = GETDATE()
                    WHERE product_id = @ProductId AND warehouse_id = @WarehouseId";

                foreach (var item in items)
                {
                    await connection.ExecuteAsync(reduceInventoryItem, new
                    {
                        ProductId = item.ProductId,
                        WarehouseId = warehouseId.Value,
                        QuantityToRemove = item.QuantityReceived
                    }, transaction);
                }
            }

            // ---------------------------------------------------------
            // 4. LÉPÉS: Készletmozgás napló törlése
            // ---------------------------------------------------------
            const string deleteStockMovements = @"
                DELETE FROM StockMovements 
                WHERE reference_document = @ReferenceDocument";

            await connection.ExecuteAsync(deleteStockMovements, new { ReferenceDocument = referenceDocument }, transaction);

            // ---------------------------------------------------------
            // 5. LÉPÉS: Bevételezés tételek törlése
            // ---------------------------------------------------------
            const string deleteGoodsReceiptItems = @"
                DELETE FROM GoodsReceiptItems 
                WHERE goods_receipt_id = @id";

            await connection.ExecuteAsync(deleteGoodsReceiptItems, new { id }, transaction);

            // ---------------------------------------------------------
            // 6. LÉPÉS: Bevételezés fejléc törlése
            // ---------------------------------------------------------
            const string deleteGoodsReceipt = @"
                DELETE FROM GoodsReceipts 
                WHERE id = @id";

            var rowsAffected = await connection.ExecuteAsync(deleteGoodsReceipt, new { id }, transaction);

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