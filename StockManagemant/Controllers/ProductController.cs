using Microsoft.AspNetCore.Mvc;
using StockManagemant.Business.Managers;
using StockManagemant.Entities.Models;
using StockManagemant.Entities.Enums;
using ExcelDataReader;
using StockManagemant.DataAccess.Filters;
using Microsoft.AspNetCore.Authorization;
using StockManagemant.Entities.DTO;
using System.ComponentModel;

namespace StockManagemant.Controllers
{ 
       [Authorize]
       public class ProductController : Controller
    {
        private readonly IProductManager _productManager;
        private readonly ICategoryManager _categoryManager;

        public ProductController(IProductManager productManager, ICategoryManager categoryManager)
        {
            _productManager = productManager;
            _categoryManager = categoryManager;
        }

    public IActionResult ProductDetail(int id)
{
    ViewData["ProductId"] = id;
    return View();
}

        // ✅ Ürünleri sayfalama ile JSON olarak döndüren action
        [Authorize(Roles = "Admin,Operator")]
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
                        category = p.CategoryName,
                        categoryId = p.CategoryId,
                        barcode = p.Barcode,
                        image = p.ImageUrl,
                        description = p.Description,
                        currencyType = p.Currency.ToString(),
                        storage=p.StorageType.ToString()
                    })
                };

                return Json(jsonData);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Ürünler getirilirken hata oluştu: {ex.Message}" });
            }
        }

       [Authorize(Roles = "Admin,Operator,BasicUser")]
        [HttpGet]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productManager.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Ürün bulunamadı." });
            }

            // BasicUser ise kontrol et: ürün kendi deposuna mı ait?
            if (User.IsInRole("BasicUser"))
            {
                var assignedWarehouseId = User.FindFirst("AssignedWarehouseId")?.Value;
                if (string.IsNullOrEmpty(assignedWarehouseId))
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }

                var productIsInUserWarehouse = await _productManager.IsProductInWarehouseAsync(id, int.Parse(assignedWarehouseId));
                if (!productIsInUserWarehouse)
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }
            }

            return Json(product);
        }

        [Authorize(Roles = "Admin,Operator,BasicUser")]
[HttpGet]
public async Task<IActionResult> GetProductByBarcode(string barcode)
{
    var product = await _productManager.GetProductByBarcode(barcode);

    if (product == null)
    {
        return NotFound(new { message = "Ürün bulunamadı." });
    }

    // BasicUser ise kontrol et: ürün kendi deposuna mı ait?
    if (User.IsInRole("BasicUser"))
    {
        var assignedWarehouseId = User.FindFirst("AssignedWarehouseId")?.Value;
        if (string.IsNullOrEmpty(assignedWarehouseId))
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

        var productIsInUserWarehouse = await _productManager.IsProductInWarehouseAsync(product.Id ?? 0, int.Parse(assignedWarehouseId));
        if (!productIsInUserWarehouse)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
    }

    return Json(product);
}




        [HttpGet]
        public async Task<IActionResult> GetCategoryDropdown()
        {
            var categories = await _categoryManager.GetAllCategoriesAsync();
            var categoryDropdown = categories.ToDictionary(c => c.Id.ToString(), c => c.Name);

            return Json(categoryDropdown);
        }

        [HttpGet]
        public IActionResult GetStorageTypeOptions()
        {
            var translations = new Dictionary<StorageType, string>
    {
        { StorageType.Undefined, "Belirsiz" },
        { StorageType.ColdStorage, "Soğuk Hava Deposu" },
        { StorageType.Flammable, "Yanıcı Madde" },
        { StorageType.Fragile, "Kırılabilir Ürün" },
        { StorageType.Standart, "Standart" },
        { StorageType.HumidProtected, "Neme Duyarlı" }
    };

            var options = translations.ToDictionary(kv => (int)kv.Key, kv => kv.Value);

            return Json(options);
        }




        // ✅ Ana sayfa view'ı
        [Authorize(Roles = "Admin ,Operator")]
        public IActionResult Index()
        {
            return View();
        }

        // ✅ Ürün ekleme sayfası
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Operator")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductDto productDto, [FromServices] ILogger<ProductController> logger)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        k => k.Key,
                        v => v.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
                    );

                logger.LogWarning("ModelState geçerli değil. Hatalar: {Errors}", string.Join(", ", errors.Select(e => $"{e.Key}: {e.Value}")));

                return Json(new { success = false, errors });
            }

            try
            {
                logger.LogInformation("Yeni ürün ekleniyor: {@ProductDto}", productDto);

                int newProductId = await _productManager.AddProductAsync(productDto);

                logger.LogInformation("Ürün başarıyla eklendi. ID: {ProductId}", newProductId);

                return Json(new { success = true, productId = newProductId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ürün eklenirken hata oluştu. Hata mesajı: {Message}, InnerException: {InnerException}",
                    ex.Message, ex.InnerException?.Message);

                return Json(new { success = false, message = $"Ürün eklenirken hata oluştu: {ex.Message}" });
            }
        }





        [Authorize(Roles = "Admin,Operator")]
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
        [Authorize(Roles = "Admin,Operator")]
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ProductDto productDto)
        {
                if (!ModelState.IsValid)
        {
            var errorList = ModelState
                .Where(kv => kv.Value.Errors.Any())
                .Select(kv => new {
                    Field = kv.Key,
                    Errors = kv.Value.Errors.Select(e => e.ErrorMessage).ToList()
                });

            return BadRequest(new
            {
                success = false,
                message = "Geçersiz ürün bilgisi veya değerler.",
                errors = errorList
            });
        }

            try
            {
                await _productManager.UpdateProductAsync(productDto);
                return Json(new { success = true, message = "Ürün başarıyla güncellendi.", id = productDto.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.ToString()}");
                return StatusCode(500, new { success = false, message = $"Ürün güncellenirken hata oluştu: {ex.Message}" });
            }

        }

        // ✅ Ürün silme işlemi (Soft Delete)
        [Authorize(Roles = "Admin,Operator")]
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
        [Authorize(Roles = "Admin")]
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

      [HttpPost]
[Authorize(Roles = "Admin,Operator")]
public async Task<IActionResult> UploadProductsFromExcel(IFormFile file)
{
    if (file == null || file.Length == 0)
    {
        return BadRequest("Dosya boş veya yüklenemedi.");
    }

    var rawProducts = new List<RawProductModel>();

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
                var storageTypeRaw = reader.GetValue(7)?.ToString()?.Trim();

    var storageTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Belirsiz", "Undefined" },
        { "Soğuk Hava Deposu", "ColdStorage" },
        { "Yanıcı Madde", "Flammable" },
        { "Kırılabilir Ürün", "Fragile" },
        { "Standart", "Standart" },
        { "Neme Duyarlı", "HumidProtected" }
    };

    var storageTypeText = storageTypeMap.TryGetValue(storageTypeRaw ?? "", out var mappedValue) ? mappedValue : "Undefined";


            rawProducts.Add(new RawProductModel
            {
                Name = reader.GetValue(0)?.ToString(),
                Price = reader.GetValue(1)?.ToString(),
                CategoryName = reader.GetValue(2)?.ToString(),
                CurrencyText = reader.GetValue(3)?.ToString(),
                Barcode = reader.GetValue(4)?.ToString(),
                ImageUrl = reader.GetValue(5)?.ToString(),
                Description = reader.GetValue(6)?.ToString(),
                StorageTypeText = storageTypeText 
            });
        }

        int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var fileName = file.FileName;

        var (insertedCount, errors) = await _productManager.AddProductsFromExcelAsync(rawProducts, userId, fileName);

        return Ok(new
        {
            success = true,
            message = $"{insertedCount} ürün başarıyla eklendi.",
            errors
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Excel işlenirken bir hata oluştu: {ex.Message}");
    }
}

       


    }
}
