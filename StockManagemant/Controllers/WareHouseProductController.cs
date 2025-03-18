using Microsoft.AspNetCore.Mvc;
using StockManagemant.Business.Managers;
using StockManagemant.Entities.DTO;
using StockManagemant.DataAccess.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;
using StockManagemant.DataAccess.Repositories.Filters;

namespace StockManagemant.Controllers
{
    
    public class WarehouseProductController : Controller
    {
        private readonly IWarehouseProductManager _warehouseProductManager;

        public WarehouseProductController(IWarehouseProductManager warehouseProductManager)
        {
            _warehouseProductManager = warehouseProductManager;
        }


        public IActionResult WarehouseProducts(int warehouseId)
        {
            if (warehouseId <= 0)
            {
                return BadRequest("Geçersiz depo ID!");
            }

            return View("WarehouseProducts", warehouseId); 
        }


        public IActionResult AddProduct()
        {
            return View();
        }

        // Sayfalı depo ürünleri
        [HttpGet]
        public async Task<IActionResult> GetWarehouseProducts([FromQuery] WarehouseProductFilter filter, int warehouseId, int page = 1, int rows = 5)
        {
            if (warehouseId <= 0)
            {
                return BadRequest(new { success = false, message = "Geçersiz depo ID!" });
            }

            try
            {
                var totalProducts = await _warehouseProductManager.GetTotalWarehouseProductCountAsync(filter, warehouseId);
                var totalPages = (int)Math.Ceiling((double)totalProducts / rows);
                var products = await _warehouseProductManager.GetPagedWarehouseProductsAsync(filter, warehouseId, page, rows);

                Console.WriteLine($"Total Records: {totalProducts}, Total Pages: {totalPages}");

                var jsonData = new
                {
                    total = totalPages,
                    page = page,
                    records = totalProducts,
                    rows = products.Select(p => new
                    {
                        id = p.ProductId,
                        name = p.ProductName,
                        price = p.Price,
                        category = p.CategoryName,
                        currencyType = p.Currency.ToString(),
                        stock = p.StockQuantity
                    })
                };

                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Depodaki ürünler getirilirken hata oluştu: {ex.Message}" });
            }
        }





        // Yeni Ürün ekleme depoya 
        [HttpPost]
        public async Task<IActionResult> AddProductToWarehouse([FromBody] WarehouseProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _warehouseProductManager.AddProductToWarehouseAsync(dto);
                return Ok(new { success = true, message = "Ürün başarıyla depoya eklendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Depoya ürün eklenirken hata oluştu: {ex.Message}" });
            }
        }

        //Depo ürünü stok yönetimi 
        [HttpPost]
        public async Task<IActionResult> UpdateStock([FromBody] WarehouseProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _warehouseProductManager.IncreaseStockAsync(dto);
                return Ok(new { success = true, message = "Ürün stoğu başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Ürün stoğu güncellenirken hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DecreaseStock([FromBody] WarehouseProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _warehouseProductManager.DecreaseStockAsync(dto);
                return Ok(new { success = true, message = "Ürün stoğu başarıyla azaltıldı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Ürün stoğu azaltılırken hata oluştu: {ex.Message}" });
            }
        }


        // Depo ürün kaldıerma
        [HttpPost]
        public async Task<IActionResult> RemoveProductFromWarehouse(int warehouseProductId)
        {
            try
            {
                await _warehouseProductManager.RemoveProductFromWarehouseAsync(warehouseProductId);
                return Ok(new { success = true, message = "Ürün depodan kaldırıldı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Depodaki ürün kaldırılırken hata oluştu: {ex.Message}" });
            }
        }

        // Depo ürünü geri getirme 
        [HttpPost]
        public async Task<IActionResult> RestoreWarehouseProduct(int warehouseProductId)
        {
            try
            {
                await _warehouseProductManager.RestoreWarehouseProductAsync(warehouseProductId);
                return Ok(new { success = true, message = "Depodaki ürün geri yüklendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Depodaki ürün geri yüklenirken hata oluştu: {ex.Message}" });
            }
        }
    }
}
