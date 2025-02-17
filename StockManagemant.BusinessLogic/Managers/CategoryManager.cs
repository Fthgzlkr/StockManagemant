using StockManagemant.DataAccess.Repositories;
using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.Business.Managers
{
    public class CategoryManager
    {
        private readonly CategoryRepository _categoryRepository;

        public CategoryManager(CategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }


        public async Task<int> GetTotalCategoryCountAsync()
        {
            return await _categoryRepository.GetTotalCategoryCountAsync();
        }

        // ✅ Tüm kategorileri getir
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return new List<Category>(await _categoryRepository.GetAllAsync());
        }

        // ✅ ID'ye göre kategori getir
        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        // ✅ Yeni kategori ekle
        public async Task AddCategoryAsync(Category category)
        {
            await _categoryRepository.AddAsync(category);
        }

        // ✅ Kategori güncelle
        public async Task UpdateCategoryAsync(Category category)
        {
            await _categoryRepository.UpdateAsync(category);
        }

        // ✅ Kategori silme
        public async Task DeleteCategoryAsync(int id)
        {
            await _categoryRepository.DeleteAsync(id);
        }

        public async Task<Category> GetCategoryByNameAsync(string name)
        {
            return await _categoryRepository.GetByNameAsync(name);
        }

    }
}
