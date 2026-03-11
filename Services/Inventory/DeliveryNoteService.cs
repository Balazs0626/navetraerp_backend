using Dapper;
using System.Data.SqlClient;
using NavetraERP.DTOs;
using NavetraERP.Utils;
using System.Security.AccessControl;

namespace NavetraERP.Services;

public class DeliveryNoteService
{

    private readonly IConfiguration _config;

    public DeliveryNoteService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateAsync(CreateDeliveryNoteDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertDeliveryNote = @"
                INSERT INTO DeliveryNotes (
                    customer_id,
                    license_plate,
                    status,
                    create_date,
                    shipping_date
                )
                VALUES (
                    @CustomerId,
                    @LicensePlate,
                    @Status,
                    @CreateDate,
                    @ShippingDate
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var result = await connection.ExecuteScalarAsync<int>(insertDeliveryNote, new
            {
                dto.CustomerId,
                dto.LicensePlate,
                dto.Status,
                CreateDate = DateTime.UtcNow,
                dto.ShippingDate
            }, transaction);
            
            foreach (var item in dto.Items)
            {
                const string insertDeliveryNoteItem = @"
                    INSERT INTO DeliveryNoteItems (
                        delivery_note_id,
                        product_id,
                        quantity
                    )
                    VALUES (
                        @DeliveryNoteId,
                        @ProductId,
                        @Quantity
                    )";

                var itemResult = await connection.ExecuteScalarAsync<int>(insertDeliveryNoteItem, new
                {
                    DeliveryNoteId = result,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
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

    public async Task<IEnumerable<DeliveryNoteListDto>> GetAllAsync(string? receiptNumber = null, DateTime? createDate = null, DateTime? shippingDate = null, string? status = null)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        string query = @"
            SELECT
                id AS Id,
                status AS Status,
                create_date AS CreateDate,
                shipping_date AS ShippingDate
            FROM DeliveryNotes
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

        if (createDate.HasValue)
        {
            query += " AND create_date = @CreateDate";
            parameters.Add("@CreateDate", createDate.Value.Date);
        }

        if (shippingDate.HasValue)
        {
            query += " AND shipping_date = @ShippingDate";
            parameters.Add("@ShippingDate", shippingDate.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query += " AND status = @Status";
            parameters.Add("@Status", status);
        }

        var result = await connection.QueryAsync<DeliveryNoteListDto>(query, parameters);

        foreach (var item in result)
        {
            item.ReceiptNumber = $"SZL-{item.Id.ToString().PadLeft(5, '0')}";
        }

        return result;
    }

    public async Task<DeliveryNoteDto> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        var transaction = connection.BeginTransaction();
        
        try
        {
            const string deliveryNoteQuery = @"
                SELECT
                    dn.id AS Id,
                    dn.create_date AS CreateDate,
                    dn.shipping_date AS ShippingDate,
                    dn.license_plate AS LicensePlate,
                    dn.status AS Status,
                    cd.name AS ShipperName,
                    cd.tax_number AS ShipperTaxNumber,
                    cd.eu_tax_number AS ShipperEuTaxNumber,
                    cd.billing_country + ', ' + cd.billing_region AS ShipperAddress_1,
                    cd.billing_post_code + ' ' + cd.billing_city + ', ' + cd.billing_address_1 + ' ' + ISNULL(cd.billing_address_2, '') AS ShipperAddress_2,
                    c.id AS CustomerId,
                    c.name AS CustomerName,
                    c.tax_number AS CustomerTaxNumber,
                    c.eu_tax_number AS CustomerEuTaxNumber,
                    a.country + ', ' + a.region AS CustomerAddress_1,
                    a.post_code + ' ' + a.city + ', ' + a.address_1 + ' ' + ISNULL(a.address_2, '') AS CustomerAddress_2
                FROM DeliveryNotes dn
                JOIN Customers c ON c.id = dn.customer_id
                JOIN Addresses a ON a.id = c.shipping_address_id
                CROSS JOIN CompanyData cd
                WHERE dn.id = @id";

            var result = await connection.QueryFirstOrDefaultAsync<DeliveryNoteDto>(deliveryNoteQuery, new
            {
                id
            }, transaction);

            
            if (result != null)
            {
                result.ReceiptNumber = $"SZL-{id.ToString().PadLeft(5, '0')}";
            }

            const string deliveryNoteItemsQuery = @"
                SELECT
                    dni.id AS Id,
                    dni.delivery_note_id AS DeliveryNoteId,
                    dni.product_id AS ProductId,
                    p.sku AS ProductSku,
                    p.name AS ProductName,
                    p.unit AS ProductUnit,
                    dni.quantity AS Quantity
                FROM DeliveryNoteItems dni
                JOIN Products p ON p.id = dni.product_id
                WHERE delivery_note_id = @id";

            var items = (await connection.QueryAsync<DeliveryNoteItemDto>(deliveryNoteItemsQuery, new
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

    public async Task<bool> UpdateAsync(int id, DeliveryNoteDto dto)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("Default"));

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        var rowsAffected = 0;

        try
        {

            const string updateDeliveryNote = @"
                UPDATE DeliveryNotes
                SET
                    customer_id = @CustomerId,
                    shipping_date = @ShippingDate,
                    license_plate = @LicensePlate,
                    status = @Status
                WHERE id = @id";

            var parameters = new DynamicParameters(dto);
            parameters.Add("@id", id);

            rowsAffected = await connection.ExecuteAsync(updateDeliveryNote, parameters, transaction);

            if (dto.Items != null)
            {
                var deleteDeliveryNoteItemQuery = "DELETE FROM DeliveryNoteItems WHERE delivery_note_id = @id";

                await connection.ExecuteAsync(deleteDeliveryNoteItemQuery, new
                {
                    id
                }, transaction);

                foreach (var item in dto.Items)
                {
                    const string insertDeliveryNoteItem = @"
                        INSERT INTO DeliveryNoteItems (
                            delivery_note_id,
                            product_id,
                            quantity
                        )
                        VALUES (
                            @DeliveryNoteId,
                            @ProductId,
                            @Quantity
                        )";

                    var itemResult = await connection.ExecuteScalarAsync<int>(insertDeliveryNoteItem, new
                    {
                        DeliveryNoteId = id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
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

            const string deleteDeliveryNoteItems = @"
                DELETE FROM DeliveryNoteItems
                WHERE delivery_note_id = @id";

            var rowsAffected = await connection.ExecuteAsync(deleteDeliveryNoteItems, new
            {
                id
            }, transaction);

            const string deleteDeliveryNote = @"
                DELETE FROM DeliveryNotes 
                WHERE id = @id";

            rowsAffected = await connection.ExecuteAsync(deleteDeliveryNote, new
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