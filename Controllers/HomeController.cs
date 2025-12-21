using Microsoft.AspNetCore.Mvc;
using HomeRadar.Data;

namespace HomeRadar.Controllers
{
    public class HomeController : Controller
    {
        private readonly EmlakContext _context;

        public HomeController(EmlakContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // ViewBag kullanımı (Üye 4 için örnek)
            ViewBag.Message = "HomeRadar - Emlak Değerleme Sistemi";
            ViewBag.Title = "Ana Sayfa";
            
            try
            {
                ViewBag.ListingCount = _context.Listings.Count();
                ViewBag.DistrictCount = _context.Districts.Count();
                ViewBag.UserCount = _context.Users.Count();
            }
            catch
            {
                ViewBag.ListingCount = 0;
                ViewBag.DistrictCount = 0;
                ViewBag.UserCount = 0;
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

