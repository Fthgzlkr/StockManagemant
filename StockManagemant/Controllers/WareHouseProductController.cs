using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StockManagemant.Business.Managers;
using StockManagemant.Entities.DTO;
using ExcelDataReader;
using StockManagemant.DataAccess.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;
using StockManagemant.DataAccess.Repositories.Filters;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Collections.Generic;

namespace StockManagemant.Controllers
{
    
    public class WarehouseProductController : Controller
    {
        private readonly IWarehouseProductManager _warehouseProductManager;

        public WarehouseProductController(IWarehouseProductManager warehouseProductManager)
        {
            _warehouseProductManager = warehouseProductManager;
        }


        [Authorize(Roles = "Admin,Operator,BasicUser")]
        public IActionResult WarehouseProducts(int warehouseId)
        {
            if (User.IsInRole("BasicUser"))
            {
                var assignedId = User.FindFirst("AssignedWarehouseId")?.Value;
                if (assignedId == null || assignedId != warehouseId.ToString())
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }
            }

            if (warehouseId <= 0)
            {
                return BadRequest("Geçersiz depo ID!");
            }

            return View("WarehouseProducts", warehouseId); 
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Operator,BasicUser")]
        public async Task<IActionResult> GetWarehouseProducts([FromQuery] WarehouseProductFilter filter, int warehouseId, int page = 1, int rows = 5)
        {
            if (User.IsInRole("BasicUser"))
            {
                var assignedId = User.FindFirst("AssignedWarehouseId")?.Value;
                if (assignedId == null || assignedId != warehouseId.ToString())
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }
            }

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
                        id = p.WarehouseProductId,
                        product_id = p.ProductId,
                        name = p.ProductName,
                        price = p.Price,
                        category = p.CategoryName,
                        currencyType = p.Currency?.ToString(),
                        stock = p.StockQuantity,
                        location = p.LocationDisplay,
                        stockcode=p.StockCode,
                        barcode = p.Barcode,
                        image_url =p.ImageUrl,
                        description= p.Description,
                    })
                };

                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Depodaki ürünler getirilirken hata oluştu: {ex.Message}" });
            }
        }





        [Authorize(Roles = "Admin,Operator,BasicUser")]
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
        public async Task<IActionResult> UpdateStock([FromBody] UpdateStockDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _warehouseProductManager.UpdateStockAsync(dto);
                return Ok(new { success = true, message = "Stok başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Stok güncellenirken hata oluştu: {ex.Message}" });
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

        [Authorize(Roles = "Admin,Operator")]
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

        [Authorize(Roles = "Admin,Operator")]
        [HttpPost]
        public async Task<IActionResult> UploadWarehouseProductsFromExcel(IFormFile file, int warehouseId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Dosya boş veya yüklenemedi.");
            }

            var warehouseExcelList = new List<WarehouseProductExcelDto>();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using var reader = ExcelReaderFactory.CreateReader(stream);
                var isFirstRow = true;

                while (reader.Read())
                {
                    if (isFirstRow)
                    {
                        isFirstRow = false;
                        continue;
                    }

                    var barcode = reader.GetValue(0)?.ToString()?.Trim();
                    var qtyStr = reader.GetValue(1)?.ToString();
                    var locationText = reader.GetValue(2)?.ToString()?.Trim();
                    var stockCode = reader.GetValue(3)?.ToString()?.Trim();

                    if (!int.TryParse(qtyStr, out int quantityChange))
                        continue;

                    warehouseExcelList.Add(new WarehouseProductExcelDto
                    {
                        Barcode = barcode,
                        QuantityChange = quantityChange,
                        LocationText = locationText,
                        StockCode = stockCode
                    });
                }

                string fileName = file.FileName;
                int? userId = null;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int parsedUserId))
                    userId = parsedUserId;

                var (insertedCount, updatedCount, errors) = await _warehouseProductManager
                    .UpsertWarehouseProductsFromExcelAsync(warehouseId, warehouseExcelList, fileName, userId ?? 0);

                return Ok(new
                {
                    success = true,
                    insertedCount,
                    updatedCount,
                    errors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Excel işlenirken hata oluştu: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin,Operator")]
        public IActionResult AddProduct()
        {
            return View();
        }
    }
}
