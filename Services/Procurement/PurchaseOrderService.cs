using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;
using System.Security.AccessControl;

namespace NavetraERP.Services;

public class PurchaseOrderService
{

    private readonly IConfiguration _config;

    public PurchaseOrderService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreatePurchaseOrderDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertPurchaseOrder = @"
                INSERT INTO PurchaseOrders (
                    supplier_id,
                    order_date,
                    expected_delivery_date,
                    status,
                    total_amount,
                    currency
                )
                VALUES (
                    @SupplierId,
                    @OrderDate,
                    @ExpectedDeliveryDate,
                    @Status,
                    @TotalAmount,
                    @Currency
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var result = await connection.ExecuteScalarAsync<int>(insertPurchaseOrder, dto, transaction);
            
            foreach (var item in dto.Items)
            {
                const string insertPurchaseOrderItem = @"
                    INSERT INTO PurchaseOrderItems (
                        purchase_order_id,
                        product_id,
                        quantity_ordered,
                        price_per_unit,
                        discount,
                        tax_rate
                    )
                    VALUES (
                        @PurchaseOrderId,
                        @ProductId,
                        @QuantityOrdered,
                        @PricePerUnit,
                        @Discount,
                        @TaxRate
                    )";

                var itemResult = await connection.ExecuteScalarAsync<int>(insertPurchaseOrderItem, new
                {
                    PurchaseOrderId = result,
                    ProductId = item.ProductId,
                    QuantityOrdered = item.QuantityOrdered,
                    PricePerUnit = item.PricePerUnit,
                    Discount = item.Discount,
                    TaxRate = item.TaxRate
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

    public async Task<IEnumerable<PurchaseOrderListDto>> GetAllAsync(int? id = null, DateTime? orderDate = null, string? status = null)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string query = @"
            SELECT
                id AS Id,
                order_date AS OrderDate,
                expected_delivery_date AS ExpectedDeliveryDate,
                status AS Status
            FROM PurchaseOrders
            WHERE 1 = 1";

        var parameters = new DynamicParameters();

        if (id != null)
        {
            query += " AND id = @Id";
            parameters.Add("@Id", id);
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

        var result = await connection.QueryAsync<PurchaseOrderListDto>(query, parameters);

        return result;
    }

    public async Task<PurchaseOrderDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        var transaction = connection.BeginTransaction();
        
        try
        {
            const string purchaseOrderQuery = @"
                SELECT
                    po.id AS Id,
                    po.supplier_id AS SupplierId,
                    s.name AS SupplierName,
                    s.tax_number AS SupplierTaxNumber,
                    a.country AS SupplierAddressCountry,
                    a.region AS SupplierAddressRegion,
                    a.post_code AS SupplierAddressPostCode,
                    a.city AS SupplierAddressCity,
                    a.address_1 AS SupplierAddressFirstLine,
                    a.address_2 AS SupplierAddressSecondLine,
                    po.order_date AS OrderDate,
                    po.expected_delivery_date AS ExpectedDeliveryDate,
                    po.status AS Status,
                    po.total_amount AS TotalAmount,
                    po.currency AS Currency
                FROM PurchaseOrders po
                JOIN Suppliers s ON s.id = po.supplier_id
                JOIN HR_Addresses a ON a.id = s.address_id
                WHERE po.id = @id";

            var result = await connection.QueryFirstOrDefaultAsync<PurchaseOrderDto>(purchaseOrderQuery, new
            {
                id
            }, transaction);

            const string purchaseOrderItemsQuery = @"
                SELECT
                    poi.purchase_order_id AS PurchaseOrderId,
                    poi.product_id AS ProductId,
                    poi.quantity_ordered AS QuantityOrdered,
                    poi.price_per_unit AS PricePerUnit,
                    poi.discount AS Discount,
                    poi.tax_rate AS TaxRate,
                    ((poi.price_per_unit * (1 - (poi.discount / 100))) * poi.quantity_ordered) * (1 - (poi.tax_rate / 100)) AS NettoPrice,
                    ((poi.price_per_unit * (1 - (poi.discount / 100))) * poi.quantity_ordered) AS BruttoPrice,
                    p.sku AS ProductSku,
                    p.name AS ProductName
                FROM PurchaseOrderItems poi
                JOIN Products p ON p.id = poi.product_id
                WHERE purchase_order_id = @id";

            var items = (await connection.QueryAsync<PurchaseOrderItemDto>(purchaseOrderItemsQuery, new
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

            const string deletePurchaseOrderItems = @"
                DELETE FROM PurchaseOrderItems 
                WHERE purchase_order_id = @id";

            var rowsAffected = await connection.ExecuteAsync(deletePurchaseOrderItems, new
            {
                id
            }, transaction);

            const string deletePurchaseOrder = @"
                DELETE FROM PurchaseOrders 
                WHERE id = @id";

            rowsAffected = await connection.ExecuteAsync(deletePurchaseOrder, new
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