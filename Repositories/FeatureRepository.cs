using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using HomeRadar.Data;
using HomeRadar.Models;

namespace HomeRadar.Repositories
{
    /// <summary>
    /// Feature Repository Implementation
    /// </summary>
    public class FeatureRepository : Repository<Feature>, IFeatureRepository
    {
        public FeatureRepository(EmlakContext context) : base(context)
        {
        }

        public override async Task<System.Collections.Generic.IEnumerable<Feature>> GetAllAsync()
        {
            return await _dbSet
                .OrderBy(f => f.Name)
                .ToListAsync();
        }
    }
}

