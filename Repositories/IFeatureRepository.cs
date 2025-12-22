using System.Collections.Generic;
using System.Threading.Tasks;
using HomeRadar.Models;

namespace HomeRadar.Repositories
{
    /// <summary>
    /// Feature Repository Interface
    /// </summary>
    public interface IFeatureRepository : IRepository<Feature>
    {
        // Generic repository yeterli, şimdilik ek metod yok
    }
}

