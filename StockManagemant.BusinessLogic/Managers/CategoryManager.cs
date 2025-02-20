using AutoMapper;
using StockManagemant.Entities.DTO;
using StockManagemant.DataAccess.Repositories;
using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.Business.Managers
{
    public class CategoryManager : ICategoryManager
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryManager(CategoryRepository categoryRepository,IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }


        public async Task<int> GetTotalCategoryCountAsync()
        {
            return await _categoryRepository.GetTotalCategoryCountAsync();
        }

        // ✅ Tüm kategorileri getir
        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        // ✅ ID'ye göre kategori getir (DTO kullanımı)
        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return _mapper.Map<CategoryDto>(category);
        }

        // ✅ Yeni kategori ekle (DTO kullanımı)
        public async Task AddCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            var category = _mapper.Map<Category>(createCategoryDto);
            await _categoryRepository.AddAsync(category);
        }

        // ✅ Kategori güncelle (DTO kullanımı)
        public async Task UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto)
        {
            var category = _mapper.Map<Category>(updateCategoryDto);
            await _categoryRepository.UpdateAsync(category);
        }

        // ✅ Kategori silme
        public async Task DeleteCategoryAsync(int id)
        {
            await _categoryRepository.DeleteAsync(id);
        }

        // ✅ Kategori ismine göre getir (DTO kullanımı)
        public async Task<CategoryDto> GetCategoryByNameAsync(string name)
        {
            var category = await _categoryRepository.GetByNameAsync(name);
            return _mapper.Map<CategoryDto>(category);
        }

    }
}
