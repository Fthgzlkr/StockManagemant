using Microsoft.AspNetCore.Mvc;
using StockManagemant.Business.Managers;
using StockManagemant.Entities.Models;
using StockManagemant.DataAccess.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;
using StockManagemant.Entities.DTO;

namespace StockManagemant.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductManager _productManager;
        private readonly CategoryManager _categoryManager;

        public ProductController(ProductManager productManager, CategoryManager categoryManager)
        {
            _productManager = productManager;
            _categoryManager = categoryManager;
        }

        // ✅ Ürünleri sayfalama ile JSON olarak döndüren action
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductFilter filter, int page = 1, int rows = 5, string sidx = "id", string sord = "asc")
        {
            try
            {
                var totalProducts = await _productManager.GetTotalProductCountAsync(filter);
                var products = await _productManager.GetPagedProductAsync(page, rows, filter); // DTO olarak dönecek

                // 🔹 **Dinamik sıralama işlemi**
                products = sidx switch
                {
                    "price" => sord == "asc" ? products.OrderBy(p => p.Price).ToList() : products.OrderByDescending(p => p.Price).ToList(),
                    "stock" => sord == "asc" ? products.OrderBy(p => p.Stock).ToList() : products.OrderByDescending(p => p.Stock).ToList(),
                    _ => sord == "asc" ? products.OrderBy(p => p.Id).ToList() : products.OrderByDescending(p => p.Id).ToList()
                };

                var totalPages = (int)Math.Ceiling((double)totalProducts / rows);

                var jsonData = new
                {
                    total = totalPages,
                    page = page,
                    records = totalProducts,
                    rows = products.Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        price = p.Price,
                        category = p.CategoryName, // DTO içinde CategoryName olarak geliyor.
                        categoryId = p.CategoryId,
                        stock = p.Stock,
                        currencyType = p.Currency.ToString()
                    })
                };

                return Json(jsonData);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Ürünler getirilirken hata oluştu: {ex.Message}" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetCategoryDropdown()
        {
            var categories = await _categoryManager.GetAllCategoriesAsync();
            var categoryDropdown = categories.ToDictionary(c => c.Id.ToString(), c => c.Name);

            return Json(categoryDropdown);
        }


        // ✅ Ana sayfa view'ı
        public IActionResult Index()
        {
            return View();
        }

        // ✅ Ürün ekleme sayfası


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            int newProductId = await _productManager.AddProductAsync(dto);
            return Json(new { success = true, productId = newProductId });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Ürün eklenirken hata oluştu: {ex.Message}" });
        }
    }




        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productManager.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { success = false, message = "Ürün bulunamadı." });
            }

            return View(product);
        }

        // ✅ **Ürün düzenleme işlemi (DTO Kullanımı)**
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Geçersiz ürün bilgisi veya değerler.", errors = ModelState.Values });
            }

            try
            {
                await _productManager.UpdateProductAsync(dto);
                return Json(new { success = true, message = "Ürün başarıyla güncellendi.", id = dto.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.ToString()}");
                return StatusCode(500, new { success = false, message = $"Ürün güncellenirken hata oluştu: {ex.Message}" });
            }

        }

        // ✅ Ürün silme işlemi (Soft Delete)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _productManager.DeleteProductAsync(id);
                return Json(new { success = true, message = "Ürün silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ürün silinirken hata oluştu: " + ex.Message });
            }
        }

        // ✅ Silinen ürünü geri getirme (Restore)
        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                await _productManager.RestoreProductAsync(id);
                return Json(new { success = true, message = "Ürün geri yüklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ürün geri yüklenirken hata oluştu: " + ex.Message });
            }
        }

       


    }
}
