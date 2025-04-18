using StockManagemant.Entities.DTO;
using StockManagemant.DataAccess.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.Business.Managers
{
   public interface IAppUserManager
    {
        Task<AppUserDto?> AuthenticateAsync(string username, string password);
        Task<List<AppUserDto>> GetAllAsync();
        Task<AppUserDto?> GetByIdAsync(int id);
        Task AddAsync(AppUserCreateDto userDto);
        Task UpdateAsync(int id, AppUserCreateDto userDto);
        Task DeleteAsync(int id);
        Task RestoreAsync(int id);
    }
}
