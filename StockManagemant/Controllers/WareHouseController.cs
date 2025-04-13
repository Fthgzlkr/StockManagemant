using Microsoft.AspNetCore.Mvc;
using StockManagemant.Business.Managers;
using StockManagemant.BusinessLogic.Managers.Interfaces;
using StockManagemant.Entities.DTO;
using AutoMapper;



namespace StockManagemant.Web.Controllers
{
    
    public class WarehouseController : Controller
    {
         private readonly IWarehouseManager _warehouseManager;
         private readonly IMapper _mapper;

        public WarehouseController(IWarehouseManager warehouseManager ,IMapper mapper )
        {
            _warehouseManager = warehouseManager;
           
            _mapper = mapper;
        }


      
        [HttpGet]
        public async Task<IActionResult> GetAllWarehouses()
        {
            try
            {
                var warehouses = await _warehouseManager.GetAllWarehousesAsync();
                return Ok(new { success = true, data = warehouses });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Depolar getirilirken hata oluştu: {ex.Message}" });
            }
        }

        //  ID'ye göre depo getir
        [HttpGet]
        public async Task<IActionResult> GetWarehouseById(int id)
        {
            try
            {
                var warehouse = await _warehouseManager.GetWarehouseByIdAsync(id);
                if (warehouse == null)
                    return NotFound(new { success = false, message = "Depo bulunamadı." });

                return Ok(new { success = true, data = warehouse });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Depo getirilirken hata oluştu: {ex.Message}" });
            }
        }

        //  İsme göre depo getir
        [HttpGet]
        public async Task<IActionResult> GetWarehouseByName(string name)
        {
            try
            {
                var warehouse = await _warehouseManager.GetWarehouseByNameAsync(name);
                if (warehouse == null)
                    return NotFound(new { success = false, message = "Depo bulunamadı." });

                return Ok(new { success = true, data = warehouse });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Depo getirilirken hata oluştu: {ex.Message}" });
            }
        }

        //  Yeni depo ekleme
        [HttpPost]
        public async Task<IActionResult> AddWarehouse([FromBody] WareHouseDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                int newWarehouseId = await _warehouseManager.AddWarehouseAsync(dto);
                return Ok(new { success = true, warehouseId = newWarehouseId, message = "Depo başarıyla eklendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Depo eklenirken hata oluştu: {ex.Message}" });
            }
        }

        //  Depo güncelleme
        [HttpPost]
        public async Task<IActionResult> UpdateWarehouse([FromBody] WareHouseDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _warehouseManager.UpdateWarehouseAsync(dto);
                return Ok(new { success = true, message = "Depo başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Depo güncellenirken hata oluştu: {ex.Message}" });
            }
        }

        //  Depo silme (Soft Delete)
        [HttpPost]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            try
            {
                await _warehouseManager.DeleteWarehouseAsync(id);
                return Ok(new { success = true, message = "Depo silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Depo silinirken hata oluştu: {ex.Message}" });
            }
        }

        //  Silinen depoyu geri getirme (Restore)
        [HttpPost]
        public async Task<IActionResult> RestoreWarehouse(int id)
        {
            try
            {
                await _warehouseManager.RestoreWarehouseAsync(id);
                return Ok(new { success = true, message = "Depo geri yüklendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Depo geri yüklenirken hata oluştu: {ex.Message}" });
            }
        }

        //  Depo ve içindeki ürünleri getir
        [HttpGet]
        public async Task<IActionResult> GetWarehouseWithProducts(int id)
        {
            try
            {
                var warehouse = await _warehouseManager.GetWarehouseWithProductsAsync(id);
                if (warehouse == null)
                    return NotFound(new { success = false, message = "Depo bulunamadı veya ürün içermiyor." });

                return Ok(new { success = true, data = warehouse });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Depo ve ürünler getirilirken hata oluştu: {ex.Message}" });
            }
        }

         [HttpGet]
      public async Task<IActionResult> Manage()
    {
        var warehouses = await _warehouseManager.GetAllWarehousesAsync();
        return View(warehouses);
    }


      [HttpGet]
     public IActionResult Locations(int id)
{
    ViewBag.WarehouseId = id;
    return View();
}
  
  


    }
}
