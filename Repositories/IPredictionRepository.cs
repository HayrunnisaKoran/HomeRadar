using System.Collections.Generic;
using System.Threading.Tasks;
using HomeRadar.Models;

namespace HomeRadar.Repositories
{
    /// <summary>
    /// Prediction Repository Interface - Tahminler için özel repository metotları
    /// </summary>
    public interface IPredictionRepository : IRepository<Prediction>
    {
        Task<IEnumerable<Prediction>> GetPredictionsWithDetailsAsync();
        Task<Prediction?> GetPredictionWithDetailsAsync(int id);
        Task<IEnumerable<Prediction>> GetPredictionsByUserAsync(int userId);
    }
}

