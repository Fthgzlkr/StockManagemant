using Microsoft.AspNetCore.Mvc;
using StockManagemant.Business.Managers;
using StockManagemant.Entities.Models;
using StockManagemant.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagemant.Controllers
{

    public class ReceiptController : Controller
    {
        private readonly ReceiptManager _receiptManager;
        private readonly ReceiptDetailManager _receiptDetailManager;
        private readonly ProductManager _productManager;

        public ReceiptController(ReceiptManager receiptManager, ReceiptDetailManager receiptDetailManager, ProductManager productManager)
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
        public async Task<IActionResult> GetReceipts(int page = 1, int rows = 10, string sidx = "id", string sord = "asc", DateTime? startDate = null, DateTime? endDate = null)
        {
            var totalReceipts = await _receiptManager.GetTotalReceiptCountAsync(startDate, endDate);
            var receipts = await _receiptManager.GetPagedReceiptAsync(page, rows, startDate, endDate);

            // Dinamik sıralama
            if (sidx == "id")
                receipts = sord == "asc" ? receipts.OrderBy(r => r.Id).ToList() : receipts.OrderByDescending(r => r.Id).ToList();
            else if (sidx == "totalAmount")
                receipts = sord == "asc" ? receipts.OrderBy(r => r.TotalAmount).ToList() : receipts.OrderByDescending(r => r.TotalAmount).ToList();

            var totalPages = (int)Math.Ceiling((double)totalReceipts / rows);

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalReceipts,
                rows = receipts.Select(r => new
                {
                    id = r.Id,
                    date = r.Date.ToString("yyyy-MM-dd HH:mm"),
                    totalAmount = r.TotalAmount.ToString("C")
                })
            };

            return Json(jsonData);
        }

        // ✅ **Fiş detaylarını görüntüleme**
        public async Task<IActionResult> Details(int id)
        {
            var receipt = await _receiptManager.GetReceiptByIdAsync(id);
            if (receipt == null) return NotFound();

            var filteredDetails = await _receiptDetailManager.GetReceiptDetailsByReceiptIdAsync(id);

            var model = new Tuple<Receipt, List<ReceiptDetail>>(receipt, filteredDetails);
            return View("Details", model);
        }

        // ✅ **Seçilen fişin ürünlerini JSON olarak getirir**
        [HttpGet]
        public async Task<IActionResult> GetReceiptDetails(int receiptId)
        {
            var filteredDetails = await _receiptDetailManager.GetReceiptDetailsByReceiptIdAsync(receiptId);

            foreach (var detail in filteredDetails)
            {
                var product = await _productManager.GetProductByIdWithDeletedAsync(detail.ProductId);
                detail.Product = product;
            }

            var jsonData = new
            {
                rows = filteredDetails.Select(d => new
                {
                    id = d.Id,
                    productId = d.Product?.Id,
                    productName = d.Product?.Name,
                    quantity = d.Quantity,
                    unitPrice = CurrencyHelper.FormatPrice(d.ProductPriceAtSale, d.Product?.Currency.ToString()),
                    subTotal = d.SubTotal.ToString("C")
                })
            };

            return Json(jsonData);
        }

        // ✅ **Fiş silme (Soft Delete)**
        [HttpPost]
        public async Task<IActionResult> DeleteReceipt( int receiptId)
        {
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
                Console.WriteLine("Haattaaaa");
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
                if (receipt == null) return NotFound();

                receipt.Date = date;
                await _receiptManager.UpdateReceiptAsync(receiptId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
