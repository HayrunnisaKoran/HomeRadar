using System.Collections.Generic;
using System.Threading.Tasks;
using HomeRadar.Models;

namespace HomeRadar.Services
{
    /// <summary>
    /// District Service Interface - İlçeler için iş mantığı
    /// </summary>
    public interface IDistrictService
    {
        Task<IEnumerable<District>> GetAllDistrictsAsync();
        Task<IEnumerable<District>> GetDistrictsWithListingsAsync();
        Task<District?> GetDistrictByIdAsync(int id);
        Task<District?> GetDistrictWithListingsAsync(int id);
        Task<District> CreateDistrictAsync(District district);
        Task UpdateDistrictAsync(District district);
        Task DeleteDistrictAsync(int id);
        Task<bool> DistrictExistsAsync(int id);
        Task<int> GetTotalDistrictsCountAsync();
        Task<int> GetTotalActiveListingsCountAsync();
    }
}

