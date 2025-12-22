using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeRadar.Data;
using HomeRadar.Models;

namespace HomeRadar.Repositories
{
    /// <summary>
    /// Prediction Repository Implementation
    /// </summary>
    public class PredictionRepository : Repository<Prediction>, IPredictionRepository
    {
        public PredictionRepository(EmlakContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Prediction>> GetPredictionsWithDetailsAsync()
        {
            return await _dbSet
                .Include(p => p.User)
                .Include(p => p.District)
                .Include(p => p.BuildingType)
                .Include(p => p.Listing)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Prediction?> GetPredictionWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.User)
                .Include(p => p.District)
                .Include(p => p.BuildingType)
                .Include(p => p.Listing)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Prediction>> GetPredictionsByUserAsync(int userId)
        {
            return await _dbSet
                .Include(p => p.District)
                .Include(p => p.BuildingType)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}

