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
                    total_amount
                )
                VALUES (
                    @SupplierId,
                    @OrderDate,
                    @ExpectedDeliveryDate,
                    @Status,
                    @TotalAmount
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

    public async Task<IEnumerable<PurchaseOrderListDto>> GetAllAsync(string? receiptNumber = null, DateTime? orderDate = null, string? status = null)
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
            query += " AND order_date = @InvoiceDate";
            parameters.Add("@InvoiceDate", orderDate.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query += " AND status = @Status";
            parameters.Add("@Status", status);
        }

        var result = await connection.QueryAsync<PurchaseOrderListDto>(query, parameters);

        foreach (var item in result)
        {
            item.ReceiptNumber = $"PO-{item.Id.ToString().PadLeft(5, '0')}";
        }

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
                    s.eu_tax_number AS SupplierEuTaxNumber,
                    s.bank_account_number AS SupplierBankAccountNumber,
                    a.country AS SupplierAddressCountry,
                    a.region AS SupplierAddressRegion,
                    a.post_code AS SupplierAddressPostCode,
                    a.city AS SupplierAddressCity,
                    a.address_1 AS SupplierAddressFirstLine,
                    a.address_2 AS SupplierAddressSecondLine,
                    cd.name AS CompanyName,
                    cd.tax_number AS CompanyTaxNumber,
                    cd.eu_tax_number AS CompanyEuTaxNumber,
                    cd.bank_account_number AS CompanyBankAccountNumber,
                    cd.billing_country + ', ' + cd.billing_region AS CompanyAddress_1,
                    cd.billing_post_code + ' ' + cd.billing_city + ', ' + cd.billing_address_1 + ' ' + ISNULL(cd.billing_address_2, '') AS CompanyAddress_2,
                    po.order_date AS OrderDate,
                    po.expected_delivery_date AS ExpectedDeliveryDate,
                    po.status AS Status,
                    po.total_amount AS TotalAmount
                FROM PurchaseOrders po
                JOIN Suppliers s ON s.id = po.supplier_id
                JOIN Addresses a ON a.id = s.address_id
                CROSS JOIN CompanyData cd
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
                    p.name AS ProductName,
                    p.unit AS ProductUnit
                FROM PurchaseOrderItems poi
                JOIN Products p ON p.id = poi.product_id
                WHERE purchase_order_id = @id";

            var items = (await connection.QueryAsync<PurchaseOrderItemDto>(purchaseOrderItemsQuery, new
            {
                id
            }, transaction)).ToList();

            if (result != null)
            {
                result.ReceiptNumber = $"PO-{id.ToString().PadLeft(5, '0')}";
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

    public async Task<bool> UpdateAsync(int id, PurchaseOrderDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        var rowsAffected = 0;

        try
        {
            const string updatePurchaseOrder = @"
                UPDATE PurchaseOrders
                SET
                    supplier_id = @SupplierId,
                    order_date = @OrderDate,
                    expected_delivery_date = @ExpectedDeliveryDate,
                    status = @Status,
                    total_amount = @TotalAmount
                WHERE id = @id";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@id", id);

            rowsAffected = await connection.ExecuteAsync(updatePurchaseOrder, parameters, transaction);

            if (dto.Items != null)
            {
                var deleteQuery = "DELETE FROM PurchaseOrderItems WHERE purchase_order_id = @id";

                await connection.ExecuteAsync(deleteQuery, new
                {
                    id
                }, transaction);

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
                        PurchaseOrderId = id,
                        ProductId = item.ProductId,
                        QuantityOrdered = item.QuantityOrdered,
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