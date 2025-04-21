using Microsoft.AspNetCore.Mvc;
using StockManagemant.Business.Managers;
using StockManagemant.Entities.DTO;
using StockManagemant.Web.Helpers;
using StockManagemant.DataAccess.Filters;
using Microsoft.AspNetCore.Authorization;


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
         [Authorize(Roles = "Admin")]
        public IActionResult List()
        {
            return View();
        }

        //Depo Fişlerini dönen sayfa
        [Authorize(Roles = "Admin,Operator,BasicUser")]
        [HttpGet]
        public IActionResult ListByWarehouse(int warehouseId)
        {
            if (warehouseId <= 0)
            {
                return BadRequest("Geçersiz depo ID!");
            }

            if (User.IsInRole("BasicUser"))
            {
                var assignedWarehouseId = User.FindFirst("AssignedWarehouseId")?.Value;
                if (assignedWarehouseId == null || assignedWarehouseId != warehouseId.ToString())
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }
            }

            ViewData["WarehouseId"] = warehouseId; 
            return View("ListByWarehouse"); 
        }


        // JQGrid için fişleri JSON formatında getirir
        [Authorize(Roles = "Admin,Operator,BasicUser")]
        [HttpGet]
        public async Task<IActionResult> GetReceipts([FromQuery] ReceiptFilter filter, int page = 1, int rows = 10, string sidx = "id", string sord = "asc")
        {

             if (User.IsInRole("BasicUser"))
            {
                var assignedWarehouseId = User.FindFirst("AssignedWarehouseId")?.Value;
                if (string.IsNullOrEmpty(assignedWarehouseId) || filter.WarehouseId.ToString() != assignedWarehouseId)
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }
            }

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
                    totalAmount = r.TotalAmount.ToString("C"),
                    receiptType = r.ReceiptType.ToString(),
                    receiptNumber = r.ReceiptNumber,
                    description = r.Description
                })
            };

            return Json(jsonData);
        }


        // Fiş detaylarını görüntüleme
        [Authorize(Roles = "Admin,Operator,BasicUser")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var receipt = await _receiptManager.GetReceiptByIdAsync(id);
            if (receipt == null)
                return NotFound(new { success = false, message = "Fiş bulunamadı." });

            if (User.IsInRole("BasicUser"))
            {
                var assignedWarehouseId = User.FindFirst("AssignedWarehouseId")?.Value;
                if (assignedWarehouseId == null || receipt.WareHouseId.ToString() != assignedWarehouseId)
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }
            }

            return View("Details", receipt);
        }



        // Seçilen fişin ürünlerini JSON olarak getirir
        [Authorize(Roles = "Admin,Operator,BasicUser")]
        [HttpGet]
        public async Task<IActionResult> GetReceiptDetails(int receiptId)
        {
            // Erişim kontrolü: BasicUser sadece kendi deposuna ait fişleri görebilir
            if (User.IsInRole("BasicUser"))
            {
                var receipt = await _receiptManager.GetReceiptByIdAsync(receiptId);
                var assignedWarehouseId = User.FindFirst("AssignedWarehouseId")?.Value;

                if (receipt == null || assignedWarehouseId == null || receipt.WareHouseId.ToString() != assignedWarehouseId)
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }
            }

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
                }).Select(t => t.Result)
            };

            return Json(jsonData);
        }


        // Fiş silme (Soft Delete)
        [Authorize(Roles = "Admin,Operator")]
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
        [Authorize(Roles = "Admin,Operator")]
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
        [Authorize(Roles = "Admin,Operator")]
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



        [Authorize(Roles = "Admin,Operator")]
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

        [Authorize(Roles = "Admin,Operator")]
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


        [Authorize(Roles = "Admin,Operator")]
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
        [Authorize(Roles = "Admin,Operator")]
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

        // Fiş oluşturma sayfasında depo ürünü getirirken 
        [Authorize(Roles = "Admin,Operator")]
        [HttpGet]
        public async Task<IActionResult> GetWarehouseProduct(int warehouseId, string barcode)
        {
            if (warehouseId <= 0 || string.IsNullOrWhiteSpace(barcode))
            {
                return BadRequest(new { success = false, message = "Geçersiz depo ID veya barkod!" });
            }

            try
            {
                var warehouseProduct = await _warehouseProductManager.GetProductInWarehouseByBarcodeAsync(warehouseId, barcode);
                return Ok(new { success = true, data = warehouseProduct });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Depo ürünü getirilirken hata oluştu: {ex.Message}" });
            }
        }



    }
}
