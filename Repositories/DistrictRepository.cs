using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeRadar.Data;
using HomeRadar.Models;

namespace HomeRadar.Repositories
{
    /// <summary>
    /// District Repository Implementation
    /// </summary>
    public class DistrictRepository : Repository<District>, IDistrictRepository
    {
        public DistrictRepository(EmlakContext context) : base(context)
        {
        }

        public async Task<IEnumerable<District>> GetDistrictsWithListingsAsync()
        {
            return await _dbSet
                .Include(d => d.Listings!)
                    .ThenInclude(l => l.BuildingType)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<District?> GetDistrictWithListingsAsync(int id)
        {
            return await _dbSet
                .Include(d => d.Listings!)
                    .ThenInclude(l => l.BuildingType)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public override async Task<IEnumerable<District>> GetAllAsync()
        {
            return await _dbSet
                .OrderBy(d => d.Name)
                .ToListAsync();
        }
    }
}

