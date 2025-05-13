using AutoMapper;
using StockManagemant.DataAccess.Repositories.Interfaces;
using StockManagemant.Entities.DTO;
using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.Business.Managers
{
    public class CustomerManager : ICustomerManager
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;

        public CustomerManager(ICustomerRepository customerRepository, IMapper mapper)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
        }

        public async Task<List<CustomersDto>> GetAllCustomersAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            return _mapper.Map<List<CustomersDto>>(customers);
        }

        public async Task<CustomersDto> GetCustomerByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            return _mapper.Map<CustomersDto>(customer);
        }

        public async Task<int> AddCustomerAsync(CustomersDto customerDto)
        {
            var customer = _mapper.Map<Customers>(customerDto);

            // Burayı ekliyoruz sadece:
            customer.CreatedAt = DateTime.UtcNow;

            await _customerRepository.AddAsync(customer);
            return customer.Id;
        }

        public async Task UpdateCustomerAsync(CustomersDto customerDto)
        {
            var existingCustomer = await _customerRepository.GetByIdAsync(customerDto.Id.Value); // Veritabanından çekiyoruz

            if (existingCustomer == null)
                throw new Exception("Güncellenecek müşteri bulunamadı.");

            // Sadece değişen alanları elle set et
            existingCustomer.Name = customerDto.Name;
            existingCustomer.Phone = customerDto.Phone;
            existingCustomer.Email = customerDto.Email;
            existingCustomer.Address = customerDto.Address;
            existingCustomer.UpdatedAt = DateTime.UtcNow;

            await _customerRepository.UpdateAsync(existingCustomer);
        }

        public async Task DeleteCustomerAsync(int id)
        {
            await _customerRepository.DeleteAsync(id);
        }

        public async Task RestoreCustomerAsync(int id)
        {
            await _customerRepository.RestoreAsync(id);
        }
    }
}