using System.Collections.Generic;
using System.Threading.Tasks;
using HomeRadar.Models;

namespace HomeRadar.Repositories
{
    /// <summary>
    /// User Repository Interface - Kullanıcılar için özel repository metotları
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<User?> GetUserWithPredictionsAsync(int id);
    }
}

