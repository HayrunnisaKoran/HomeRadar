using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeRadar.Services
{
    /// <summary>
    /// Home Service Interface - Ana sayfa istatistikleri için
    /// </summary>
    public interface IHomeService
    {
        Task<int> GetListingCountAsync();
        Task<int> GetDistrictCountAsync();
        Task<int> GetUserCountAsync();
        Task<int> GetPredictionCountAsync();
        Task<decimal> GetAveragePredictedPriceAsync();
        Task<Dictionary<string, int>> GetTopDistrictsByPredictionCountAsync(int topCount = 10);
        Task<Dictionary<string, decimal>> GetAveragePriceByDistrictAsync();
        Task<Dictionary<string, int>> GetPredictionsByDateRangeAsync(int days = 30);
        Task<Dictionary<string, int>> GetPredictionsByBuildingTypeAsync();
        Task<Dictionary<string, int>> GetPredictionsByRoomCountAsync();
        Task<Dictionary<string, int>> GetPriceRangeDistributionAsync();
        Task<Dictionary<string, object>> GetDistrictStatisticsAsync();
        
        // Listings tablosundan gerçek ilan verilerine dayalı istatistikler
        Task<Dictionary<string, int>> GetTopDistrictsByListingCountAsync(int topCount = 10);
        Task<Dictionary<string, decimal>> GetAveragePriceByDistrictFromListingsAsync();
        Task<Dictionary<string, int>> GetListingsByBuildingTypeAsync();
        Task<Dictionary<string, int>> GetListingsByRoomCountAsync();
        Task<Dictionary<string, int>> GetListingPriceRangeDistributionAsync();
        Task<Dictionary<string, object>> GetDistrictStatisticsFromListingsAsync();
        Task<decimal> GetAverageListingPriceAsync();
        Task<int> GetTotalActiveListingCountAsync();
    }
}

