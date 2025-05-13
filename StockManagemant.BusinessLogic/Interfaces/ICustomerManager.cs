using StockManagemant.Entities.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.Business.Managers
{
    public interface ICustomerManager
    {
        Task<List<CustomersDto>> GetAllCustomersAsync();
        Task<CustomersDto> GetCustomerByIdAsync(int id);
        Task<int> AddCustomerAsync(CustomersDto customerDto);
        Task UpdateCustomerAsync(CustomersDto customerDto);
        Task DeleteCustomerAsync(int id);
        Task RestoreCustomerAsync(int id);
    }
}