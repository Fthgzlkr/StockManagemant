using AutoMapper;
using StockManagemant.Entities.DTO;
using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockManagemant.DataAccess.Repositories.Interfaces;

namespace StockManagemant.Business.Managers
{
    public class CategoryManager : ICategoryManager
    {
        private readonly ICategoryRepository _categoryRepository; 
        private readonly IMapper _mapper;

        public CategoryManager(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<int> GetTotalCategoryCountAsync()
        {
            return await _categoryRepository.GetTotalCategoryCountAsync();
        }

        // ✅ Tüm kategorileri getir
        public async Task<List<GeneralDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<List<GeneralDto>>(categories);
        }

        // ✅ ID'ye göre kategori getir (DTO kullanımı)
        public async Task<GeneralDto> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return _mapper.Map<GeneralDto>(category);
        }

        // ✅ Yeni kategori ekle (DTO kullanımı)
        public async Task AddCategoryAsync(GeneralDto categoryDto)
        {
            var category = _mapper.Map<Category>(categoryDto);
            await _categoryRepository.AddAsync(category);
        }


        // ✅ Kategori güncelle (DTO kullanımı)
        public async Task UpdateCategoryAsync(GeneralDto categoryDto)
        {
            var category = _mapper.Map<Category>(categoryDto);
            await _categoryRepository.UpdateAsync(category);
        }

        // ✅ Kategori silme
        public async Task DeleteCategoryAsync(int id)
        {
            await _categoryRepository.DeleteAsync(id);
        }

        // ✅ Kategori ismine göre getir (DTO kullanımı)
        public async Task<GeneralDto> GetCategoryByNameAsync(string name)
        {
            var category = await _categoryRepository.GetByNameAsync(name);
            return _mapper.Map<GeneralDto>(category);
        }

    }
}
