using System.Collections.Generic;
using System.Threading.Tasks;
using HomeRadar.Models;

namespace HomeRadar.Services
{
    /// <summary>
    /// Listing Service Interface - İlanlar için iş mantığı
    /// </summary>
    public interface IListingService
    {
        Task<IEnumerable<Listing>> GetAllListingsAsync();
        Task<IEnumerable<Listing>> GetActiveListingsAsync();
        Task<Listing?> GetListingByIdAsync(int id);
        Task<Listing?> GetListingWithDetailsAsync(int id);
        Task<IEnumerable<Listing>> GetListingsByDistrictAsync(int districtId);
        Task<Listing> CreateListingAsync(Listing listing);
        Task UpdateListingAsync(Listing listing);
        Task DeleteListingAsync(int id);
        Task<bool> ListingExistsAsync(int id);
        Task<IEnumerable<District>> GetDistrictsAsync();
        Task<IEnumerable<BuildingType>> GetBuildingTypesAsync();
        Task<IEnumerable<Feature>> GetFeaturesAsync();
        Task<(int SuccessCount, int ErrorCount, List<string> Errors)> ImportListingsFromCsvAsync(string csvFilePath);
        Task<(int SuccessCount, int ErrorCount, List<string> Errors)> ImportListingsFromOnehotCsvAsync(string csvFilePath); // ONEHOT CSV import
        Task<int> DeleteAllListingsAsync(); // Tüm ilanları siler (hard delete)
    }
}

