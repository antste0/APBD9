using Tutorial9.Model;

namespace Tutorial9.Services;

public interface IDbService
{
    Task<int> AddProductToWarehouse(WarehouseProductDTO product);
    Task<int> AddProductToWarehouseProcedure(WarehouseProductDTO product);
}