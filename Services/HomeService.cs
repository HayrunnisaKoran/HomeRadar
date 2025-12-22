using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeRadar.Models;
using HomeRadar.Repositories;

namespace HomeRadar.Services
{
    /// <summary>
    /// Home Service Implementation - Ana sayfa istatistikleri için
    /// </summary>
    public class HomeService : IHomeService
    {
        private readonly IListingRepository _listingRepository;
        private readonly IDistrictRepository _districtRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPredictionRepository _predictionRepository;
        private readonly IBuildingTypeRepository _buildingTypeRepository;
        private readonly IListingService _listingService;

        public HomeService(
            IListingRepository listingRepository,
            IDistrictRepository districtRepository,
            IUserRepository userRepository,
            IPredictionRepository predictionRepository,
            IBuildingTypeRepository buildingTypeRepository,
            IListingService listingService)
        {
            _listingRepository = listingRepository;
            _districtRepository = districtRepository;
            _userRepository = userRepository;
            _predictionRepository = predictionRepository;
            _buildingTypeRepository = buildingTypeRepository;
            _listingService = listingService;
        }

        public async Task<int> GetListingCountAsync()
        {
            return await _listingRepository.CountAsync();
        }

        public async Task<int> GetDistrictCountAsync()
        {
            return await _districtRepository.CountAsync();
        }

        public async Task<int> GetUserCountAsync()
        {
            return await _userRepository.CountAsync();
        }

        public async Task<int> GetPredictionCountAsync()
        {
            return await _predictionRepository.CountAsync();
        }

        public async Task<decimal> GetAveragePredictedPriceAsync()
        {
            var predictions = await _predictionRepository.GetAllAsync();
            var predictionsList = predictions.ToList();
            
            if (!predictionsList.Any())
                return 0;

            return predictionsList.Average(p => p.PredictedPriceAvg);
        }

        public async Task<Dictionary<string, int>> GetTopDistrictsByPredictionCountAsync(int topCount = 10)
        {
            var predictions = await _predictionRepository.GetAllAsync();
            var predictionsList = predictions.ToList();
            
            var districts = await _districtRepository.GetAllAsync();
            var districtsDict = districts.ToDictionary(d => d.Id, d => d.Name);

            var result = predictionsList
                .Where(p => p.DistrictId > 0 && districtsDict.ContainsKey(p.DistrictId))
                .GroupBy(p => districtsDict[p.DistrictId])
                .Select(g => new { DistrictName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topCount)
                .ToDictionary(x => x.DistrictName, x => x.Count);

            return result;
        }

        public async Task<Dictionary<string, decimal>> GetAveragePriceByDistrictAsync()
        {
            var predictions = await _predictionRepository.GetAllAsync();
            var predictionsList = predictions.ToList();
            
            var districts = await _districtRepository.GetAllAsync();
            var districtsDict = districts.ToDictionary(d => d.Id, d => d.Name);

            var result = predictionsList
                .Where(p => p.DistrictId > 0 && districtsDict.ContainsKey(p.DistrictId))
                .GroupBy(p => districtsDict[p.DistrictId])
                .Select(g => new { DistrictName = g.Key, AvgPrice = g.Average(p => p.PredictedPriceAvg) })
                .ToDictionary(x => x.DistrictName, x => x.AvgPrice);

            return result;
        }

        public async Task<Dictionary<string, int>> GetPredictionsByDateRangeAsync(int days = 30)
        {
            var predictions = await _predictionRepository.GetAllAsync();
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            
            var result = predictions
                .Where(p => p.CreatedAt >= cutoffDate)
                .GroupBy(p => p.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => g.Key.ToString("dd.MM.yyyy"),
                    g => g.Count()
                );

            return result;
        }

        public async Task<Dictionary<string, int>> GetPredictionsByBuildingTypeAsync()
        {
            var predictions = await _predictionRepository.GetAllAsync();
            var predictionsList = predictions.ToList();
            
            var buildingTypes = await _buildingTypeRepository.GetAllAsync();
            var buildingTypesDict = buildingTypes.ToDictionary(bt => bt.Id, bt => bt.Name);

            var result = predictionsList
                .Where(p => p.BuildingTypeId.HasValue && buildingTypesDict.ContainsKey(p.BuildingTypeId.Value))
                .GroupBy(p => buildingTypesDict[p.BuildingTypeId.Value])
                .Select(g => new { 
                    BuildingType = g.Key,
                    Count = g.Count()
                })
                .ToDictionary(x => x.BuildingType, x => x.Count);

            return result;
        }

        public async Task<Dictionary<string, int>> GetPredictionsByRoomCountAsync()
        {
            var predictions = await _predictionRepository.GetAllAsync();
            var predictionsList = predictions.ToList();
            
            var result = predictionsList
                .GroupBy(p => p.RoomCount)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => $"{g.Key}+1",
                    g => g.Count()
                );

            return result;
        }

        public async Task<Dictionary<string, int>> GetPriceRangeDistributionAsync()
        {
            var predictions = await _predictionRepository.GetAllAsync();
            var predictionsList = predictions.ToList();
            
            var ranges = new[]
            {
                new { Label = "0-1M", Min = 0m, Max = 1000000m },
                new { Label = "1M-2M", Min = 1000000m, Max = 2000000m },
                new { Label = "2M-3M", Min = 2000000m, Max = 3000000m },
                new { Label = "3M-4M", Min = 3000000m, Max = 4000000m },
                new { Label = "4M-5M", Min = 4000000m, Max = 5000000m },
                new { Label = "5M+", Min = 5000000m, Max = decimal.MaxValue }
            };

            var result = new Dictionary<string, int>();
            foreach (var range in ranges)
            {
                var count = predictionsList.Count(p => p.PredictedPriceAvg >= range.Min && p.PredictedPriceAvg < range.Max);
                result[range.Label] = count;
            }

            return result;
        }

        public async Task<Dictionary<string, object>> GetDistrictStatisticsAsync()
        {
            var predictions = await _predictionRepository.GetAllAsync();
            var predictionsList = predictions.ToList();
            
            var districts = await _districtRepository.GetAllAsync();
            var districtsDict = districts.ToDictionary(d => d.Id, d => d.Name);

            var result = new Dictionary<string, object>();
            
            var districtStats = predictionsList
                .Where(p => p.DistrictId > 0 && districtsDict.ContainsKey(p.DistrictId))
                .GroupBy(p => districtsDict[p.DistrictId])
                .Select(g => new
                {
                    DistrictName = g.Key,
                    Count = g.Count(),
                    AvgPrice = g.Average(p => p.PredictedPriceAvg),
                    MinPrice = g.Min(p => p.PredictedPriceMin),
                    MaxPrice = g.Max(p => p.PredictedPriceMax)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            foreach (var stat in districtStats)
            {
                result[stat.DistrictName] = new
                {
                    Count = stat.Count,
                    AvgPrice = stat.AvgPrice,
                    MinPrice = stat.MinPrice,
                    MaxPrice = stat.MaxPrice
                };
            }

            return result;
        }

        // ========== LISTINGS TABLOSUNDAN GERÇEK İLAN VERİLERİNE DAYALI İSTATİSTİKLER ==========

        public async Task<Dictionary<string, int>> GetTopDistrictsByListingCountAsync(int topCount = 10)
        {
            try
            {
                var listings = await _listingService.GetActiveListingsAsync();
                var listingsList = listings.ToList();
                
                System.Diagnostics.Debug.WriteLine($"GetTopDistrictsByListingCountAsync: Toplam {listingsList.Count} aktif ilan bulundu.");
                
                if (!listingsList.Any())
                {
                    System.Diagnostics.Debug.WriteLine("UYARI: Hiç aktif ilan bulunamadı!");
                    return new Dictionary<string, int>();
                }
                
                var districts = await _districtRepository.GetAllAsync();
                var districtsDict = districts.ToDictionary(d => d.Id, d => d.Name);
                
                System.Diagnostics.Debug.WriteLine($"Toplam {districtsDict.Count} ilçe bulundu.");

                var result = listingsList
                    .Where(l => l.DistrictId > 0 && districtsDict.ContainsKey(l.DistrictId))
                    .GroupBy(l => districtsDict[l.DistrictId])
                    .Select(g => new { DistrictName = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(topCount)
                    .ToDictionary(x => x.DistrictName, x => x.Count);
                
                System.Diagnostics.Debug.WriteLine($"GetTopDistrictsByListingCountAsync: {result.Count} ilçe için veri döndürüldü.");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTopDistrictsByListingCountAsync HATASI: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new Dictionary<string, int>();
            }
        }

        public async Task<Dictionary<string, decimal>> GetAveragePriceByDistrictFromListingsAsync()
        {
            try
            {
                var listings = await _listingService.GetActiveListingsAsync();
                var listingsList = listings.ToList();
                
                System.Diagnostics.Debug.WriteLine($"GetAveragePriceByDistrictFromListingsAsync: Toplam {listingsList.Count} aktif ilan bulundu.");
                
                if (!listingsList.Any())
                {
                    System.Diagnostics.Debug.WriteLine("UYARI: Hiç aktif ilan bulunamadı!");
                    return new Dictionary<string, decimal>();
                }
                
                var districts = await _districtRepository.GetAllAsync();
                var districtsDict = districts.ToDictionary(d => d.Id, d => d.Name);

                var result = listingsList
                    .Where(l => l.DistrictId > 0 && districtsDict.ContainsKey(l.DistrictId))
                    .GroupBy(l => districtsDict[l.DistrictId])
                    .Select(g => new { DistrictName = g.Key, AvgPrice = g.Average(l => l.Price) })
                    .ToDictionary(x => x.DistrictName, x => x.AvgPrice);
                
                System.Diagnostics.Debug.WriteLine($"GetAveragePriceByDistrictFromListingsAsync: {result.Count} ilçe için ortalama fiyat hesaplandı.");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAveragePriceByDistrictFromListingsAsync HATASI: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, int>> GetListingsByBuildingTypeAsync()
        {
            var listings = await _listingService.GetActiveListingsAsync();
            var listingsList = listings.ToList();
            
            var buildingTypes = await _buildingTypeRepository.GetAllAsync();
            var buildingTypesDict = buildingTypes.ToDictionary(bt => bt.Id, bt => bt.Name);

            var result = listingsList
                .Where(l => buildingTypesDict.ContainsKey(l.BuildingTypeId))
                .GroupBy(l => buildingTypesDict[l.BuildingTypeId])
                .Select(g => new { 
                    BuildingType = g.Key,
                    Count = g.Count()
                })
                .ToDictionary(x => x.BuildingType, x => x.Count);

            return result;
        }

        public async Task<Dictionary<string, int>> GetListingsByRoomCountAsync()
        {
            var listings = await _listingService.GetActiveListingsAsync();
            var listingsList = listings.ToList();
            
            var result = listingsList
                .GroupBy(l => l.RoomCount)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => $"{g.Key}+{g.First().SalonCount}",
                    g => g.Count()
                );

            return result;
        }

        public async Task<Dictionary<string, int>> GetListingPriceRangeDistributionAsync()
        {
            var listings = await _listingService.GetActiveListingsAsync();
            var listingsList = listings.ToList();
            
            var ranges = new[]
            {
                new { Label = "0-1M", Min = 0m, Max = 1000000m },
                new { Label = "1M-2M", Min = 1000000m, Max = 2000000m },
                new { Label = "2M-3M", Min = 2000000m, Max = 3000000m },
                new { Label = "3M-4M", Min = 3000000m, Max = 4000000m },
                new { Label = "4M-5M", Min = 4000000m, Max = 5000000m },
                new { Label = "5M+", Min = 5000000m, Max = decimal.MaxValue }
            };

            var result = new Dictionary<string, int>();
            foreach (var range in ranges)
            {
                var count = listingsList.Count(l => l.Price >= range.Min && l.Price < range.Max);
                result[range.Label] = count;
            }

            return result;
        }

        public async Task<Dictionary<string, object>> GetDistrictStatisticsFromListingsAsync()
        {
            var listings = await _listingService.GetActiveListingsAsync();
            var listingsList = listings.ToList();
            
            var districts = await _districtRepository.GetAllAsync();
            var districtsDict = districts.ToDictionary(d => d.Id, d => d.Name);

            var result = new Dictionary<string, object>();
            
            var districtStats = listingsList
                .Where(l => l.DistrictId > 0 && districtsDict.ContainsKey(l.DistrictId))
                .GroupBy(l => districtsDict[l.DistrictId])
                .Select(g => new
                {
                    DistrictName = g.Key,
                    Count = g.Count(),
                    AvgPrice = g.Average(l => l.Price),
                    MinPrice = g.Min(l => l.Price),
                    MaxPrice = g.Max(l => l.Price)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            foreach (var stat in districtStats)
            {
                result[stat.DistrictName] = new
                {
                    Count = stat.Count,
                    AvgPrice = stat.AvgPrice,
                    MinPrice = stat.MinPrice,
                    MaxPrice = stat.MaxPrice
                };
            }

            return result;
        }

        public async Task<decimal> GetAverageListingPriceAsync()
        {
            var listings = await _listingService.GetActiveListingsAsync();
            var listingsList = listings.ToList();
            
            if (!listingsList.Any())
                return 0;

            return listingsList.Average(l => l.Price);
        }

        public async Task<int> GetTotalActiveListingCountAsync()
        {
            var listings = await _listingService.GetActiveListingsAsync();
            return listings.Count();
        }
    }
}

