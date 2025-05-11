using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial9.Model;

namespace Tutorial9.Services;

public class DbService(IConfiguration configuration) : IDbService
{
    public async Task<int> AddProductToWarehouse(WarehouseProductDTO product)
    {
        await using SqlConnection connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        await using SqlCommand command = new SqlCommand("", connection);
        
        await command.Connection.OpenAsync();
        var transaction = connection.BeginTransaction();
        command.Transaction = transaction;

        try
        {
            command.CommandText = @"SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            int count = (int)await command.ExecuteScalarAsync();

            if (count <= 0)
                throw new Exception($"Product {product.IdProduct} does not exist");
            
            command.CommandText = @"SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @Id";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
            count = (int)await command.ExecuteScalarAsync();
            
            if (count <= 0)
                throw new Exception($"Warehouse {product.IdWarehouse} does not exist");

            command.CommandText = @"Select IdOrder From [Order]
                     WHERE [Order].IdProduct = @IdProduct
                     AND [Order].Amount = @Amount
                     AND [Order].CreatedAt < @CreatedAt";
            
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            command.Parameters.AddWithValue("@Amount", product.Amount);
            command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
            var idOrder = (int)await command.ExecuteScalarAsync();

            if (idOrder == null)
            {
                throw new Exception($"Order {product.IdProduct} does not exist");
            }
            
            command.CommandText = @"SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @IdOrder";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            count = (int)await command.ExecuteScalarAsync();
            
            if (count > 0)
                throw new Exception($"Order {idOrder} for product {product.IdProduct} already fulfilled");

            command.CommandText = @"UPDATE [Order]
            SET [Order].FulfilledAt = @FulfilledAt
            WHERE [Order].IdOrder = @IdOrder";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@FulfilledAt", product.CreatedAt);
            await command.ExecuteNonQueryAsync();
            
            command.CommandText = @"SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            decimal price = (decimal)await command.ExecuteScalarAsync();
            
            command.CommandText = @"
            INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
            VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)
            SELECT CAST(SCOPE_IDENTITY() as int);
            ";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@Amount", product.Amount);
            command.Parameters.AddWithValue("@Price", price * product.Amount);
            command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
            
            int result = (int)await command.ExecuteScalarAsync();
            
            await transaction.CommitAsync();
            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> AddProductToWarehouseProcedure(WarehouseProductDTO request)
    {
        string command = "AddProductToWarehouse";

        await using SqlConnection connection = new SqlConnection(configuration.GetConnectionString("Default"));
        await using SqlCommand cmd = new SqlCommand(command, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
        cmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

        await connection.OpenAsync();
        int result = (int)await cmd.ExecuteScalarAsync();

        return result;
    }
}