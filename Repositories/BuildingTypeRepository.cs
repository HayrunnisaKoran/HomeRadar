using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using HomeRadar.Data;
using HomeRadar.Models;

namespace HomeRadar.Repositories
{
    /// <summary>
    /// BuildingType Repository Implementation
    /// </summary>
    public class BuildingTypeRepository : Repository<BuildingType>, IBuildingTypeRepository
    {
        public BuildingTypeRepository(EmlakContext context) : base(context)
        {
        }

        public override async Task<System.Collections.Generic.IEnumerable<BuildingType>> GetAllAsync()
        {
            return await _dbSet
                .OrderBy(bt => bt.Name)
                .ToListAsync();
        }
    }
}

