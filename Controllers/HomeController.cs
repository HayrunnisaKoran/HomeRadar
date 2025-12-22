using Microsoft.AspNetCore.Mvc;
using HomeRadar.Services;
using System;

namespace HomeRadar.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        public async Task<IActionResult> Index()
        {
            // ViewBag kullanımı (Üye 4 için örnek)
            ViewBag.Message = "HomeRadar - Emlak Değerleme Sistemi";
            ViewBag.Title = "Ana Sayfa";
            
            try
            {
                // Temel istatistikler
                ViewBag.DistrictCount = await _homeService.GetDistrictCountAsync();
                ViewBag.UserCount = await _homeService.GetUserCountAsync();
                ViewBag.PredictionCount = await _homeService.GetPredictionCountAsync();
                
                // Listings tablosundan gerçek ilan verilerine dayalı istatistikler
                ViewBag.ListingCount = await _homeService.GetTotalActiveListingCountAsync();
                ViewBag.AverageListingPrice = await _homeService.GetAverageListingPriceAsync();
                
                // Grafik verileri - Listings tablosundan
                ViewBag.TopDistricts = await _homeService.GetTopDistrictsByListingCountAsync(10);
                ViewBag.AveragePriceByDistrict = await _homeService.GetAveragePriceByDistrictFromListingsAsync();
                ViewBag.ListingsByBuildingType = await _homeService.GetListingsByBuildingTypeAsync();
                ViewBag.ListingsByRoomCount = await _homeService.GetListingsByRoomCountAsync();
                ViewBag.PriceRangeDistribution = await _homeService.GetListingPriceRangeDistributionAsync();
                ViewBag.DistrictStatistics = await _homeService.GetDistrictStatisticsFromListingsAsync();
                
                // Tahmin trendi için hala predictions kullanıyoruz (zaman bazlı analiz için)
                ViewBag.PredictionsByDate = await _homeService.GetPredictionsByDateRangeAsync(30);
            }
            catch (Exception ex)
            {
                // Hata loglama - Debug için
                System.Diagnostics.Debug.WriteLine($"HomeController Index hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                ViewBag.DistrictCount = 0;
                ViewBag.UserCount = 0;
                ViewBag.PredictionCount = 0;
                ViewBag.ListingCount = 0;
                ViewBag.AverageListingPrice = 0;
                ViewBag.TopDistricts = new Dictionary<string, int>();
                ViewBag.AveragePriceByDistrict = new Dictionary<string, decimal>();
                ViewBag.PredictionsByDate = new Dictionary<string, int>();
                ViewBag.ListingsByBuildingType = new Dictionary<string, int>();
                ViewBag.ListingsByRoomCount = new Dictionary<string, int>();
                ViewBag.PriceRangeDistribution = new Dictionary<string, int>();
                ViewBag.DistrictStatistics = new Dictionary<string, object>();
                
                // Hata mesajını ViewBag'e ekle (debug için)
                ViewBag.ErrorMessage = $"Veri yüklenirken hata oluştu: {ex.Message}";
            }
            
            return View();
        }

        public IActionResult About()
        {
            ViewBag.Message = "Hakkında";
            ViewBag.Title = "Hakkında";
            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.Message = "İletişim";
            ViewBag.Title = "İletişim";
            
            // ViewData kullanımı örneği
            ViewData["ContactEmail"] = "info@homeradar.com";
            ViewData["ContactPhone"] = "+90 555 123 4567";
            
            return View();
        }
    }
}

