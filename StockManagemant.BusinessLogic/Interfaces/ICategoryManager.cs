using StockManagemant.Entities.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.Business.Managers
{
    public interface ICategoryManager
    {
        Task<int> GetTotalCategoryCountAsync();
        Task<List<GeneralDto>> GetAllCategoriesAsync();
        Task<GeneralDto> GetCategoryByIdAsync(int id);
        Task AddCategoryAsync(GeneralDto categoryDto);
        Task UpdateCategoryAsync(GeneralDto categoryDto);
        Task DeleteCategoryAsync(int id);
        Task<GeneralDto> GetCategoryByNameAsync(string name);
    }
}
