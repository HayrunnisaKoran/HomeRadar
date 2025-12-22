using System.Collections.Generic;
using System.Threading.Tasks;
using HomeRadar.Models;

namespace HomeRadar.Services
{
    /// <summary>
    /// User Service Interface - Kullanıcılar için iş mantığı
    /// </summary>
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserWithPredictionsAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<User> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(int id);
        Task<int> GetUserCountAsync();
        Task<int> GetAdminCountAsync();
        Task<int> GetRegularUserCountAsync();
    }
}

