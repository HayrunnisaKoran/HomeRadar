using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeRadar.Data;
using HomeRadar.Models;

namespace HomeRadar.Repositories
{
    /// <summary>
    /// Listing Repository Implementation
    /// </summary>
    public class ListingRepository : Repository<Listing>, IListingRepository
    {
        public ListingRepository(EmlakContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Listing>> GetActiveListingsAsync()
        {
            return await _dbSet
                .Where(l => l.IsActive)
                .Include(l => l.District)
                .Include(l => l.BuildingType)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Listing>> GetListingsByDistrictAsync(int districtId)
        {
            return await _dbSet
                .Where(l => l.DistrictId == districtId && l.IsActive)
                .Include(l => l.District)
                .Include(l => l.BuildingType)
                .ToListAsync();
        }

        public async Task<IEnumerable<Listing>> GetListingsWithDetailsAsync()
        {
            return await _dbSet
                .Include(l => l.District)
                .Include(l => l.BuildingType)
                .Include(l => l.ListingFeatures!)
                    .ThenInclude(lf => lf.Feature)
                .Where(l => l.IsActive)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<Listing?> GetListingWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(l => l.District)
                .Include(l => l.BuildingType)
                .Include(l => l.ListingFeatures!)
                    .ThenInclude(lf => lf.Feature)
                .FirstOrDefaultAsync(l => l.Id == id);
        }
    }
}

