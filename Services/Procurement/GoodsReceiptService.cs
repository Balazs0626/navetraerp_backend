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

    public async Task<IEnumerable<GoodsReceiptListDto>> GetAllAsync()
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        const string query = @"
            SELECT
                gr.id AS Id,
                gr.purchase_order_id AS PurchaseOrderId,
                w.name AS WarehouseName,
                gr.receipt_date AS ReceiptDate
            FROM GoodsReceipts gr
            JOIN Warehouses w ON w.id = gr.warehouse_id";

        var result = await connection.QueryAsync<GoodsReceiptListDto>(query);

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
                JOIN HR_Addresses a ON a.id = w.address_id
                JOIN HR_Employee e ON e.id = gr.received_by_employee_id
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
                    gri.quantity_received AS QuantityReceived,
                    gri.batch_number AS BatchNumber
                FROM GoodsReceiptItems gri
                JOIN Products p ON p.id = gri.product_id
                WHERE gri.goods_receipt_id = @id";

            var items = (await connection.QueryAsync<GoodsReceiptItemDto>(goodsReceiptItemsQuery, new
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
    }

}