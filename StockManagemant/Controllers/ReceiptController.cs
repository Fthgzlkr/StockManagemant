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

        public ReceiptController(IReceiptManager receiptManager, IReceiptDetailManager receiptDetailManager, IProductManager productManager)
        {
            _receiptManager = receiptManager;
            _receiptDetailManager = receiptDetailManager;
            _productManager = productManager;
        }

        // ✅ **Fişleri listeleme sayfası**
        public IActionResult List()
        {
            return View();
        }

        // ✅ **JQGrid için fişleri JSON formatında getirir**
        [HttpGet]
        public async Task<IActionResult> GetReceipts([FromQuery] ReceiptFilter filter, int page = 1, int rows = 10, string sidx = "id", string sord = "asc")
        {
            var totalReceipts = await _receiptManager.GetTotalReceiptCountAsync(filter);
            var receipts = await _receiptManager.GetPagedReceiptAsync(page, rows, filter);

            // 🚀 **İyileştirme:** Dinamik sıralamayı LINQ ile sunucu tarafında yapalım.
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


        // ✅ **Fiş detaylarını görüntüleme**
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var receipt = await _receiptManager.GetReceiptByIdAsync(id);
            if (receipt == null) return NotFound(new { success = false, message = "Fiş bulunamadı." });

            var filteredDetails = await _receiptManager.GetReceiptDetailsAsync(id);

            if (filteredDetails == null || !filteredDetails.Any())
            {
                return NotFound(new { success = false, message = "Fiş detayları bulunamadı." });
            }

            var model = new Tuple<ReceiptDto, List<ReceiptDetailDto>>(receipt, filteredDetails);
            return View("Details", model);
        }


        // ✅ **Seçilen fişin ürünlerini JSON olarak getirir**
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


        // ✅ **Fiş silme (Soft Delete)**
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


        // ✅ **Fişe ürün ekleme**
        [HttpPost]
        public async Task<IActionResult> AddProductToReceipt(int receiptId, int productId, int quantity)
        {
            try
            {
                await _receiptManager.AddProductToReceiptAsync(receiptId, productId, quantity);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ✅ **Fişten ürün kaldırma (Soft Delete)**
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

        // ✅ **Fişteki ürün miktarını güncelleme**
        [HttpPost]
        public async Task<IActionResult> UpdateProductQuantityInReceipt(int receiptDetailId, int newQuantity)
        {
            try
            {
                await _receiptManager.UpdateProductQuantityInReceiptAsync(receiptDetailId, newQuantity);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }



        public IActionResult Create()
        {
            return View();
        }
        // ✅ **Yeni fiş oluşturma**
        [HttpPost]
        public async Task<IActionResult> CreateReceipt()
        {
            try
            {
                int newReceiptId = await _receiptManager.AddReceiptAsync();
                return Json(new { success = true, receiptId = newReceiptId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ✅ **Fiş güncelleme (Tarih güncelleme)**
        [HttpPost]
        public async Task<IActionResult> UpdateReceipt(int receiptId, DateTime date)
        {
            try
            {
                var receipt = await _receiptManager.GetReceiptByIdAsync(receiptId);
                if (receipt == null) return NotFound(new { success = false, message = "Fiş bulunamadı!" });

                // DTO kullanarak güncelleme işlemi yap
                var updateDto = new UpdateReceiptDto
                {
                    Id = receiptId,
                    Date = date,
                    TotalAmount = receipt.TotalAmount
                };

                // ✅ Metodu doğru parametre ile çağır!
                await _receiptManager.UpdateReceiptDateAsync(updateDto);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


    }
}
