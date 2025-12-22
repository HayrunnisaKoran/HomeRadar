using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeRadar.Models;
using HomeRadar.Repositories;

namespace HomeRadar.Services
{
    /// <summary>
    /// District Service Implementation - İlçeler için iş mantığı
    /// </summary>
    public class DistrictService : IDistrictService
    {
        private readonly IDistrictRepository _districtRepository;

        public DistrictService(IDistrictRepository districtRepository)
        {
            _districtRepository = districtRepository;
        }

        public async Task<IEnumerable<District>> GetAllDistrictsAsync()
        {
            return await _districtRepository.GetAllAsync();
        }

        public async Task<IEnumerable<District>> GetDistrictsWithListingsAsync()
        {
            return await _districtRepository.GetDistrictsWithListingsAsync();
        }

        public async Task<District?> GetDistrictByIdAsync(int id)
        {
            return await _districtRepository.GetByIdAsync(id);
        }

        public async Task<District?> GetDistrictWithListingsAsync(int id)
        {
            return await _districtRepository.GetDistrictWithListingsAsync(id);
        }

        public async Task<District> CreateDistrictAsync(District district)
        {
            return await _districtRepository.AddAsync(district);
        }

        public async Task UpdateDistrictAsync(District district)
        {
            await _districtRepository.UpdateAsync(district);
        }

        public async Task DeleteDistrictAsync(int id)
        {
            await _districtRepository.DeleteAsync(id);
        }

        public async Task<bool> DistrictExistsAsync(int id)
        {
            return await _districtRepository.AnyAsync(d => d.Id == id);
        }

        public async Task<int> GetTotalDistrictsCountAsync()
        {
            return await _districtRepository.CountAsync();
        }

        public async Task<int> GetTotalActiveListingsCountAsync()
        {
            var districts = await _districtRepository.GetDistrictsWithListingsAsync();
            return districts.Sum(d => d.Listings?.Count(l => l.IsActive) ?? 0);
        }
    }
}

