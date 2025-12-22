using System.Collections.Generic;
using System.Threading.Tasks;
using HomeRadar.Models;

namespace HomeRadar.Repositories
{
    /// <summary>
    /// Listing Repository Interface - İlanlar için özel repository metotları
    /// </summary>
    public interface IListingRepository : IRepository<Listing>
    {
        Task<IEnumerable<Listing>> GetActiveListingsAsync();
        Task<IEnumerable<Listing>> GetListingsByDistrictAsync(int districtId);
        Task<IEnumerable<Listing>> GetListingsWithDetailsAsync();
        Task<Listing?> GetListingWithDetailsAsync(int id);
    }
}

