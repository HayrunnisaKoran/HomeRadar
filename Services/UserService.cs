using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeRadar.Models;
using HomeRadar.Repositories;

namespace HomeRadar.Services
{
    /// <summary>
    /// User Service Implementation - Kullanıcılar için iş mantığı
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _userRepository.GetActiveUsersAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User?> GetUserWithPredictionsAsync(int id)
        {
            return await _userRepository.GetUserWithPredictionsAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            return await _userRepository.GetUsersByRoleAsync(role);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            user.CreatedAt = System.DateTime.UtcNow;
            user.IsActive = true;
            return await _userRepository.AddAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user != null)
            {
                // Soft delete
                user.IsActive = false;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _userRepository.AnyAsync(u => u.Id == id);
        }

        public async Task<int> GetUserCountAsync()
        {
            return await _userRepository.CountAsync();
        }

        public async Task<int> GetAdminCountAsync()
        {
            var admins = await _userRepository.GetUsersByRoleAsync("Admin");
            return admins.Count();
        }

        public async Task<int> GetRegularUserCountAsync()
        {
            var users = await _userRepository.GetUsersByRoleAsync("User");
            return users.Count();
        }
    }
}

