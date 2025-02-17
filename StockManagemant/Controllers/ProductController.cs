using Microsoft.AspNetCore.Mvc;
using StockManagemant.Business.Managers;
using StockManagemant.Entities.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> GetProducts(int page = 1, int rows = 5, string sidx = "id", string sord = "asc",string search = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            try
            {
                var totalProducts = await _productManager.GetTotalProductCountAsync(search, minPrice, maxPrice);
                var products = await _productManager.GetPagedProductAsyn(page, rows, search, minPrice, maxPrice);


                // 🔹 Dinamik sıralama işlemi
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
                        category = p.Category != null ? p.Category.Name : "Uncategorized", 
                        categoryId = p.CategoryId, 
                        stock = p.Stock,
                        currencyType = p.Currency.ToString() 
                    })
                };

                return Json(jsonData);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ürünler getirilirken hata oluştu: " + ex.Message });
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

        
    [HttpPost]
public async Task<IActionResult> Create([FromBody] Product product)
{
    if (product == null || product.Price <= 0 || product.Stock < 0 || product.CategoryId <= 0)
    {
        return Json(new { success = false, message = "Geçersiz ürün bilgisi veya değerler." });
    }

    try
    {
        await _productManager.AddProductAsync(product);
        return Json(new { success = true, id = product.Id });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Ürün eklenirken hata oluştu: " + ex.Message });
    }
}




        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productManager.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }


        // ✅ Ürün düzenleme işlemi
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] Product product)
        {
            if (product == null || product.Price <= 0 || product.Stock < 0 || product.CategoryId <= 0)
            {
                return Json(new { success = false, message = "Geçersiz ürün bilgisi veya değerler." });
            }

            try
            {
                await _productManager.UpdateProductAsync(product);
                return Json(new { success = true, id = product.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ürün güncellenirken hata oluştu: " + ex.Message });
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
