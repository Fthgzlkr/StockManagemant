using Microsoft.AspNetCore.Mvc;
using StockManagemant.Business.Managers;
using StockManagemant.Entities.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace StockManagemant.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly CategoryManager _categoryManager;

        public CategoriesController(CategoryManager categoryManager)
        {
            _categoryManager = categoryManager;
        }

        // Kategori yönetim sayfası
        public IActionResult Index()
        {
            return View();
        }

        // ✅ Tüm kategorileri listeleme (jqGrid formatına uygun şekilde)
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var totalCategories = await _categoryManager.GetTotalCategoryCountAsync();
                var categories = await _categoryManager.GetAllCategoriesAsync();

                var jsonData = new
                {
                    total = 1, 
                    page = 1,
                    records = totalCategories,
                    rows = categories.Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        
                    })
                };

                return Json(jsonData);
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Kategoriler getirilirken hata oluştu: " + ex.Message });
            }
        }

        // ✅ Yeni kategori ekleme
        [HttpPost]
        public async Task<IActionResult> AddCategory( Category category)
        {
            if (category == null || string.IsNullOrEmpty(category.Name))
            {
                return Json(new { success = false, message = "Geçersiz kategori bilgisi." });
            }

            try
            {
                await _categoryManager.AddCategoryAsync(category);
                return Json(new { success = true, id = category.Id }); // ✅ ID'yi döndürerek jqGrid'e gönderiyoruz
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Kategori eklenirken hata oluştu: " + ex.Message });
            }
        }





        [HttpPost]
        public async Task<IActionResult> EditCategory([FromBody] Category category)
        {
            if (category == null || category.Id <= 0)
            {
                return BadRequest(new { success = false, message = "Geçersiz kategori verisi." });
            }

            await _categoryManager.UpdateCategoryAsync(category);

            return Ok(new { success = true, message = "Kategori başarıyla güncellendi." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _categoryManager.DeleteCategoryAsync(id);
                return Json(new { success = true, message = "Kategori silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Kategori silinirken hata oluştu: " + ex.Message });
            }
        }


    }
}
