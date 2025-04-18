using AutoMapper;
using StockManagemant.Entities.DTO;
using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockManagemant.DataAccess.Repositories.Interfaces;

namespace StockManagemant.Business.Managers
{
    public class AppUserManager : IAppUserManager
    {
        private readonly IAppUserRepository _userRepository;
        private readonly IMapper _mapper;

        public AppUserManager(IAppUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<AppUserDto?> AuthenticateAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAndPasswordAsync(username, password);
            return user != null ? _mapper.Map<AppUserDto>(user) : null;
        }

        public async Task<List<AppUserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<List<AppUserDto>>(users);
        }

        public async Task<AppUserDto?> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? _mapper.Map<AppUserDto>(user) : null;
        }

        public async Task AddAsync(AppUserCreateDto userDto)
        {
            var user = _mapper.Map<AppUser>(userDto);
            await _userRepository.AddAsync(user);
        }

        public async Task UpdateAsync(int id, AppUserCreateDto userDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user != null)
            {
                user.Username = userDto.Username;
                user.Password = userDto.Password;
                user.Role = userDto.Role;
                user.AssignedWarehouseId = userDto.AssignedWarehouseId;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task DeleteAsync(int id)
        {
            await _userRepository.DeleteAsync(id);
        }

        public async Task RestoreAsync(int id)
        {
            await _userRepository.RestoreAsync(id);
        }
    }
}
