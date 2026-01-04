using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;
using System.Security.AccessControl;

namespace NavetraERP.Services;

public class InvoiceService
{

    private readonly IConfiguration _config;

    public InvoiceService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateInvoiceDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertInvoice = @"
                INSERT INTO Invoices (
                    sales_order_id,
                    invoice_date,
                    due_date,
                    total_amount,
                    paid_amount,
                    status
                )
                VALUES (
                    @SalesOrderId,
                    @InvoiceDate,
                    @DueDate,
                    @TotalAmount,
                    @PaidAmount,
                    @Status
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var result = await connection.ExecuteScalarAsync<int>(insertInvoice, dto, transaction);
            
            foreach (var item in dto.Items)
            {
                const string insertInvoiceItem = @"
                    INSERT INTO InvoiceItems (
                        invoice_id,
                        product_id,
                        quantity,
                        unit_price,
                        tax_rate
                    )
                    VALUES (
                        @InvoiceId,
                        @ProductId,
                        @Quantity,
                        @PricePerUnit,
                        @TaxRate
                    )";

                var itemResult = await connection.ExecuteScalarAsync<int>(insertInvoiceItem, new
                {
                    InvoiceId = result,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    PricePerUnit = item.PricePerUnit,
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

    public async Task<IEnumerable<InvoiceListDto>> GetAllAsync(int? id = null, DateTime? invoiceDate = null, string? status = null)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string query = @"
            SELECT
                id AS Id,
                invoice_date AS InvoiceDate,
                due_date AS DueDate,
                status AS Status
            FROM Invoices
            WHERE 1 = 1";

        var parameters = new DynamicParameters();

        if (id != null)
        {
            query += " AND id = @Id";
            parameters.Add("@Id", id);
        }

        if (invoiceDate.HasValue)
        {
            query += " AND invoice_date = @InvoiceDate";
            parameters.Add("@InvoiceDate", invoiceDate.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query += " AND status = @Status";
            parameters.Add("@Status", status);
        }

        var result = await connection.QueryAsync<InvoiceListDto>(query, parameters);

        return result;
    }

    public async Task<InvoiceDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        var transaction = connection.BeginTransaction();
        
        try
        {
            const string invoiceQuery = @"
                SELECT
                    i.id AS Id,
                    i.sales_order_id AS SalesOrderId,
                    i.invoice_date AS InvoiceDate,
                    i.due_date AS DueDate,
                    i.total_amount AS TotalAmount,
                    i.paid_amount AS PaidAmount,
                    27 AS TotalTaxRate,
                    (i.paid_amount - i.total_amount) AS TotalTax,
                    i.status AS Status,
                    'Saját Kft.' AS SellerName,
                    '11111111-11' AS SellerTaxNumber,
                    'Magyarország, Vas' AS SellerAddress_1,
                    '9600 Sárvár, Teszt utca 6.' AS SellerAddress_2,
                    c.name AS CustomerName,
                    c.tax_number AS CustomerTaxNumber,
                    a.country + ', ' + a.region AS CustomerAddress_1,
                    a.post_code + ' ' + a.city + ', ' + a.address_1 + ' ' + ISNULL(a.address_2, '') AS CustomerAddress_2
                FROM Invoices i
                JOIN SalesOrders so ON so.id = i.sales_order_id
                JOIN Customers c ON c.id = so.customer_id
                JOIN HR_Addresses a ON a.id = c.billing_address_id
                WHERE i.id = @id";

            var result = await connection.QueryFirstOrDefaultAsync<InvoiceDto>(invoiceQuery, new
            {
                id
            }, transaction);

            const string invoiceItemsQuery = @"
                SELECT
                    ii.id AS Id,
                    ii.invoice_id AS InvoiceId,
                    ii.product_id AS ProductId,
                    p.name AS ProductName,
                    p.unit AS ProductUnit,
                    ii.quantity AS Quantity,
                    ii.unit_price AS PricePerUnit,
                    ii.tax_rate AS TaxRate
                FROM InvoiceItems ii
                JOIN Products p ON p.id = ii.product_id
                WHERE invoice_id = @id";

            var items = (await connection.QueryAsync<InvoiceItemDto>(invoiceItemsQuery, new
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

    public async Task<bool> UpdateAsync(int id, InvoiceDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        var rowsAffected = 0;

        try
        {
            const string updateInvoice = @"
                UPDATE Invoices
                SET
                    status = @Status
                WHERE id = @id";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@id", id);

            rowsAffected = await connection.ExecuteAsync(updateInvoice, parameters, transaction);

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

            const string deleteInvoiceItems = @"
                DELETE FROM InvoiceItems 
                WHERE invoice_id = @id";

            var rowsAffected = await connection.ExecuteAsync(deleteInvoiceItems, new
            {
                id
            }, transaction);

            const string deleteInvoice = @"
                DELETE FROM Invoices 
                WHERE id = @id";

            rowsAffected = await connection.ExecuteAsync(deleteInvoice, new
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