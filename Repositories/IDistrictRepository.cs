using System.Collections.Generic;
using System.Threading.Tasks;
using HomeRadar.Models;

namespace HomeRadar.Repositories
{
    /// <summary>
    /// District Repository Interface - İlçeler için özel repository metotları
    /// </summary>
    public interface IDistrictRepository : IRepository<District>
    {
        Task<IEnumerable<District>> GetDistrictsWithListingsAsync();
        Task<District?> GetDistrictWithListingsAsync(int id);
    }
}

