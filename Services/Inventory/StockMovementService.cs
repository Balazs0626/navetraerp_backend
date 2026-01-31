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

            transaction.Commit();

            return result;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

    }

    public async Task<IEnumerable<StockMovementListDto>> GetAllAsync(int? productId = null, string? referenceDocument = null, string? movementType = null, DateTime? movementDate = null)
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

        const string delete = @"
            DELETE FROM StockMovements 
            WHERE id = @id";

        var rowsAffected = await connection.ExecuteAsync(delete, new
        {
            id
        });

        return rowsAffected > 0;
    }

}