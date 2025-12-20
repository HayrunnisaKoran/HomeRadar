using System.Linq;
using System.Web.Mvc;
using HomeRadar.Data;

namespace HomeRadar.Controllers
{
    public class HomeController : Controller
    {
        private readonly EmlakContext _context;

        public HomeController()
        {
            _context = new EmlakContext();
        }

        public ActionResult Index()
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

        public ActionResult About()
        {
            ViewBag.Message = "Hakkında";
            ViewBag.Title = "Hakkında";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "İletişim";
            ViewBag.Title = "İletişim";
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

