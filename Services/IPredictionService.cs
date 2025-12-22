using System.Collections.Generic;
using System.Threading.Tasks;
using HomeRadar.Models;

namespace HomeRadar.Services
{
    /// <summary>
    /// Prediction Service Interface - Tahminler için iş mantığı
    /// </summary>
    public interface IPredictionService
    {
        Task<IEnumerable<Prediction>> GetAllPredictionsAsync();
        Task<IEnumerable<Prediction>> GetPredictionsWithDetailsAsync();
        Task<Prediction?> GetPredictionByIdAsync(int id);
        Task<Prediction?> GetPredictionWithDetailsAsync(int id);
        Task<IEnumerable<Prediction>> GetPredictionsByUserAsync(int userId);
        Task<Prediction> CreatePredictionAsync(Prediction prediction);
        Task UpdatePredictionAsync(Prediction prediction);
        Task DeletePredictionAsync(int id);
        Task<bool> PredictionExistsAsync(int id);
        Task<IEnumerable<District>> GetDistrictsAsync();
        Task<IEnumerable<BuildingType>> GetBuildingTypesAsync();
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<decimal> GetAveragePredictedPriceAsync();
        Task<Prediction> CreatePredictionWithMLAsync(MLPredictionRequest request, int? userId = null, int? buildingTypeId = null);
    }
}

