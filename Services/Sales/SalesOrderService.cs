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

    public async Task<IEnumerable<SalesOrderListDto>> GetAllAsync(int? id = null, DateTime? orderDate = null, string? status = null)
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

        var result = await connection.QueryAsync<SalesOrderListDto>(query, parameters);

        return result;
    }

    public async Task<SalesOrderDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        var transaction = connection.BeginTransaction();
        
        try
        {
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
                    so.total_amount AS TotalAmount
                FROM SalesOrders so
                JOIN Customers c ON c.id = so.customer_id
                JOIN HR_Addresses ba ON ba.id = c.billing_address_id
                JOIN HR_Addresses sa ON sa.id = c.shipping_address_id
                WHERE so.id = @id";

            var result = await connection.QueryFirstOrDefaultAsync<SalesOrderDto>(salesOrderQuery, new
            {
                id,
            }, transaction);

            if (result != null)
            {
                result.SalesOrderNumber = $"SO-{id.ToString().PadLeft(4, '0')}/{result.OrderDate.ToString("yyyy")}";
            }

            const string salesOrderItemsQuery = @"
                SELECT
                    soi.sales_order_id AS SalesOrderId,
                    soi.product_id AS ProductId,
                    soi.quantity_ordered AS QuantityOrdered,
                    soi.shipped_quantity AS QuantityShipped,
                    soi.unit_price AS PricePerUnit,
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

    public async Task<bool> UpdateAsync(int id, SalesOrderDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        var rowsAffected = 0;

        try
        {
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
                var deleteQuery = "DELETE FROM SalesOrderItems WHERE sales_order_id = @id";

                await connection.ExecuteAsync(deleteQuery, new
                {
                    id
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
    }

}