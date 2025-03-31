using Microsoft.AspNetCore.Mvc;
using StockManagemant.Business.Managers;
using StockManagemant.Entities.Models;
using StockManagemant.Entities.DTO;
using AutoMapper;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace StockManagemant.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryManager _categoryManager;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoryManager categoryManager,IMapper mapper)
        {
            _categoryManager = categoryManager;
            _mapper = mapper;
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

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CategoryDto categoryDto)
        {
            if (!ModelState.IsValid || categoryDto == null || string.IsNullOrEmpty(categoryDto.Name))
            {
                return Json(new { success = false, message = "Geçersiz kategori bilgisi." });
            }

            try
            {
                await _categoryManager.AddCategoryAsync(categoryDto);
                return Json(new { success = true, message = "Kategori başarıyla eklendi." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Kategori eklenirken hata oluştu: " + ex.Message });
            }
        }





        [HttpPost]
        public async Task<IActionResult> EditCategory([FromBody] CategoryDto categoryDto)
        {
            if (!ModelState.IsValid || categoryDto == null || categoryDto.Id <= 0)
            {
                return BadRequest(new { success = false, message = "Geçersiz kategori verisi." });
            }

            try
            {
                await _categoryManager.UpdateCategoryAsync(categoryDto);
                return Ok(new { success = true, message = "Kategori başarıyla güncellendi." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Kategori güncellenirken hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { success = false, message = "Geçersiz kategori ID." });
            }

            try
            {
                await _categoryManager.DeleteCategoryAsync(id);
                return Json(new { success = true, message = "Kategori silindi." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Kategori silinirken hata oluştu: " + ex.Message });
            }
        }


    }
}
