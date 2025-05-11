using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model;
using Tutorial9.Services;

namespace Tutorial9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class WarehouseController(IDbService _warehouseService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddProductToWarehouse([FromBody] WarehouseProductDTO productDTO)
        {
            if (productDTO == null)
                return BadRequest("Product data cannot be null");

            try
            {
                var id = await _warehouseService.AddProductToWarehouse(productDTO);
                return Ok(id);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("Procedure")]
        public async Task<IActionResult> AddProductToWarehouseProcedure([FromBody] WarehouseProductDTO productDTO)
        {
            if (productDTO == null)
                return BadRequest("Product data cannot be null");

            try
            {
                var result = await _warehouseService.AddProductToWarehouseProcedure(productDTO);
                return Ok(result);
            } catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}