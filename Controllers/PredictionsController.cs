using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeRadar.Data;
using HomeRadar.Models;

namespace HomeRadar.Controllers
{
    public class PredictionsController : Controller
    {
        private readonly EmlakContext _context;

        public PredictionsController(EmlakContext context)
        {
            _context = context;
        }

        // GET: Predictions - READ işlemi
        public async Task<IActionResult> Index()
        {
            var predictions = await _context.Predictions
                .Include(p => p.User)
                .Include(p => p.District)
                .Include(p => p.BuildingType)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Message = "Tahmin Geçmişi";
            ViewBag.TotalPredictions = predictions.Count;
            ViewBag.AvgPrice = predictions.Any() ? predictions.Average(p => p.PredictedPriceAvg) : 0;

            return View(predictions);
        }

        // GET: Predictions/Details/5 - READ işlemi (detay)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prediction = await _context.Predictions
                .Include(p => p.User)
                .Include(p => p.District)
                .Include(p => p.BuildingType)
                .Include(p => p.Listing)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prediction == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Tahmin Detayı";
            ViewData["PriceRange"] = $"{prediction.PredictedPriceMin:C0} - {prediction.PredictedPriceMax:C0}";

            return View(prediction);
        }

        // GET: Predictions/Create - CREATE işlemi (form)
        public IActionResult Create()
        {
            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.Districts.OrderBy(d => d.Name), "Id", "Name");
            ViewBag.BuildingTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.BuildingTypes.OrderBy(b => b.Name), "Id", "Name");
            ViewBag.Users = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.Users.Where(u => u.IsActive).OrderBy(u => u.LastName).ThenBy(u => u.FirstName),
                "Id", "Email");
            ViewBag.Message = "Yeni Tahmin Oluştur";

            return View();
        }

        // POST: Predictions/Create - CREATE işlemi (kaydet)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,DistrictId,BuildingTypeId,RoomCount,SquareMeters,BuildingAge,PredictedPriceMin,PredictedPriceMax,PredictedPriceAvg,ModelName")] Prediction prediction)
        {
            if (ModelState.IsValid)
            {
                prediction.CreatedAt = DateTime.Now;
                
                // Ortalama fiyat hesapla
                if (prediction.PredictedPriceAvg == 0 && prediction.PredictedPriceMin > 0 && prediction.PredictedPriceMax > 0)
                {
                    prediction.PredictedPriceAvg = (prediction.PredictedPriceMin + prediction.PredictedPriceMax) / 2;
                }

                _context.Add(prediction);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tahmin başarıyla kaydedildi!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.Districts.OrderBy(d => d.Name), "Id", "Name", prediction.DistrictId);
            ViewBag.BuildingTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.BuildingTypes.OrderBy(b => b.Name), "Id", "Name", prediction.BuildingTypeId);
            ViewBag.Users = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.Users.Where(u => u.IsActive).OrderBy(u => u.LastName).ThenBy(u => u.FirstName),
                "Id", "Email", prediction.UserId);
            return View(prediction);
        }

        // GET: Predictions/Delete/5 - DELETE işlemi (onay)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prediction = await _context.Predictions
                .Include(p => p.User)
                .Include(p => p.District)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prediction == null)
            {
                return NotFound();
            }

            return View(prediction);
        }

        // POST: Predictions/Delete/5 - DELETE işlemi (sil)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prediction = await _context.Predictions.FindAsync(id);
            if (prediction != null)
            {
                _context.Predictions.Remove(prediction);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tahmin başarıyla silindi!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

