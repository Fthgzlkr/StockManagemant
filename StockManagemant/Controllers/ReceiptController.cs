using Microsoft.AspNetCore.Mvc;
using StockManagemant.Business.Managers;
using StockManagemant.Entities.DTO;
using StockManagemant.Web.Helpers;
using StockManagemant.DataAccess.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagemant.Controllers
{

    public class ReceiptController : Controller
    {
        private readonly IReceiptManager _receiptManager;
        private readonly IReceiptDetailManager _receiptDetailManager;
        private readonly IProductManager _productManager;
        private readonly IWarehouseProductManager _warehouseProductManager;

        public ReceiptController(IReceiptManager receiptManager, IReceiptDetailManager receiptDetailManager, IProductManager productManager, IWarehouseProductManager warehouseProductManager)
        {
            _receiptManager = receiptManager;
            _receiptDetailManager = receiptDetailManager;
            _productManager = productManager;
            _warehouseProductManager = warehouseProductManager;
        }

        // Fişleri listeleme sayfası
        public IActionResult List()
        {
            return View();
        }

        //Depo Fişlerini dönen sayfa
        [HttpGet]
        public IActionResult ListByWarehouse(int warehouseId)
        {
            if (warehouseId <= 0)
            {
                return BadRequest("Geçersiz depo ID!");
            }

            ViewData["WarehouseId"] = warehouseId; 
            return View("ListByWarehouse"); 
        }


        // JQGrid için fişleri JSON formatında getirir
        [HttpGet]
        public async Task<IActionResult> GetReceipts([FromQuery] ReceiptFilter filter, int page = 1, int rows = 10, string sidx = "id", string sord = "asc")
        {
            var totalReceipts = await _receiptManager.GetTotalReceiptCountAsync(filter);
            var receipts = await _receiptManager.GetPagedReceiptAsync(page, rows, filter);

            // 
            receipts = sidx switch
            {
                "id" => sord == "asc" ? receipts.OrderBy(r => r.Id).ToList() : receipts.OrderByDescending(r => r.Id).ToList(),
                "totalAmount" => sord == "asc" ? receipts.OrderBy(r => r.TotalAmount).ToList() : receipts.OrderByDescending(r => r.TotalAmount).ToList(),
                _ => receipts
            };

            var totalPages = (int)Math.Ceiling((double)totalReceipts / rows);

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalReceipts,
                rows = receipts.Select(r => new
                {
                    id = r.Id,
                    date = r.FormattedDate,
                    totalAmount = r.TotalAmount.ToString("C")
                })
            };

            return Json(jsonData);
        }


        // Fiş detaylarını görüntüleme
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var receipt = await _receiptManager.GetReceiptByIdAsync(id);
            if (receipt == null) return NotFound(new { success = false, message = "Fiş bulunamadı." });

            return View("Details", receipt); // ✅ Sadece `ReceiptDto` gönderiliyor
        }



        // Seçilen fişin ürünlerini JSON olarak getirir
        [HttpGet]
        public async Task<IActionResult> GetReceiptDetails(int receiptId)
        {
            var filteredDetails = await _receiptDetailManager.GetReceiptDetailsByReceiptIdAsync(receiptId);

            var jsonData = new
            {
                rows = filteredDetails.Select(async d =>
                {
                    var product = await _productManager.GetProductByIdWithDeletedAsync(d.ProductId);
                    return new
                    {
                        id = d.Id,
                        productId = product?.Id,
                        productName = product?.Name,
                        quantity = d.Quantity,
                        unitPrice = CurrencyHelper.FormatPrice(d.ProductPriceAtSale, product?.Currency.ToString()),
                        subTotal = d.SubTotal.ToString("C")
                    };
                }).Select(t => t.Result) // Asenkron metodları senkron hale getiriyoruz
            };

            return Json(jsonData);
        }


        // Fiş silme (Soft Delete)
        [HttpPost]
        public async Task<IActionResult> DeleteReceipt(int receiptId)
        {
            Console.WriteLine("Silme isteği alındı, ID: " + receiptId); // Console'a yazdır
            try
            {
                await _receiptManager.DeleteReceiptAsync(receiptId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


    



        // Fişten ürün kaldırma (Soft Delete)
        [HttpPost]
        public async Task<IActionResult> RemoveProductFromReceipt( int receiptDetailId)
        {
            try
            {
                await _receiptManager.RemoveProductFromReceiptAsync(receiptDetailId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RemoveProductFromReceipt] Hata: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }

        }

        // Fişteki ürün miktarını güncelleme
        [HttpPost]
        public async Task<IActionResult> UpdateProductQuantityInReceipt(int receiptDetailId, int newQuantity)
        {
            try
            {
                await _receiptDetailManager.UpdateProductQuantityInReceiptAsync(receiptDetailId, newQuantity);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }



        [HttpGet]
        public IActionResult Create(int warehouseId)
        {
            if (warehouseId <= 0)
            {
                return BadRequest("Geçersiz depo ID!");
            }

            ViewData["WarehouseId"] = warehouseId;
            return View("Create");
        }

        public class ProductModel
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateReceipt([FromBody] ReceiptDto receiptDto)
        {
            try
            {
                if (receiptDto == null || receiptDto.WareHouseId == 0)
                {
                    return BadRequest(new { success = false, message = "Geçersiz veri: Depo ID boş olamaz!" });
                }

                int newReceiptId = await _receiptManager.AddReceiptAsync(receiptDto);
                return Json(new { success = true, receiptId = newReceiptId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateReceipt] Hata: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddProductsToReceipt(int receiptId, [FromBody] List<ProductModel> products)
        {
            try
            {
                if (receiptId == 0 || products == null || !products.Any())
                {
                    return BadRequest(new { success = false, message = "Eksik veya geçersiz veri!" });
                }

                foreach (var product in products)
                {
                    await _receiptDetailManager.AddProductToReceiptAsync(receiptId, product.ProductId, product.Quantity);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddProductsToReceipt] Hata: {ex.Message}");
                return StatusCode(500, new { success = false, message = $"Ürün ekleme hatası: {ex.Message}" });
            }
        }





        //Fiş güncelleme (Tarih güncelleme)
        [HttpPost]
        public async Task<IActionResult> UpdateReceipt(int receiptId, DateTime date)
        {
            try
            {
                var receipt = await _receiptManager.GetReceiptByIdAsync(receiptId);
                if (receipt == null) return NotFound(new { success = false, message = "Fiş bulunamadı!" });

                var updateDto = new ReceiptDto
                {
                    Id = receiptId,
                    Date = date,
                    TotalAmount = receipt.TotalAmount
                };

                await _receiptManager.UpdateReceiptDateAsync(updateDto);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetWarehouseProduct(int warehouseId, int productId)
        {
            if (warehouseId <= 0 || productId <= 0)
            {
                return BadRequest(new { success = false, message = "Geçersiz depo veya ürün ID!" });
            }

            try
            {
                var warehouseProduct = await _warehouseProductManager.GetProductInWarehouseByIdAsync(warehouseId, productId);
                if (warehouseProduct == null)
                {
                    return NotFound(new { success = false, message = "Ürün bu depoda bulunamadı!" });
                }

                return Ok(new { success = true, data = warehouseProduct });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Depo ürünü getirilirken hata oluştu: {ex.Message}" });
            }
        }



    }
}
