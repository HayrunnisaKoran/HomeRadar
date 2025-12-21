using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeRadar.Data;
using HomeRadar.Models;

namespace HomeRadar.Controllers
{
    public class DistrictsController : Controller
    {
        private readonly EmlakContext _context;

        public DistrictsController(EmlakContext context)
        {
            _context = context;
        }

        // GET: Districts - READ işlemi
        public async Task<IActionResult> Index()
        {
            var districts = await _context.Districts
                .Include(d => d.Listings)
                .OrderBy(d => d.Name)
                .ToListAsync();

            // ViewBag ile istatistikler
            ViewBag.Message = "İlçe Yönetimi";
            ViewBag.TotalDistricts = districts.Count;
            ViewBag.TotalListings = districts.Sum(d => d.Listings?.Count(l => l.IsActive) ?? 0);

            return View(districts);
        }

        // GET: Districts/Details/5 - READ işlemi (detay)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var district = await _context.Districts
                .Include(d => d.Listings!)
                    .ThenInclude(l => l.BuildingType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (district == null)
            {
                return NotFound();
            }

            ViewData["Title"] = district.Name + " Detayı";
            ViewData["ListingCount"] = district.Listings?.Count(l => l.IsActive) ?? 0;

            return View(district);
        }

        // GET: Districts/Create - CREATE işlemi (form)
        public IActionResult Create()
        {
            ViewBag.Message = "Yeni İlçe Ekle";
            return View();
        }

        // POST: Districts/Create - CREATE işlemi (kaydet)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,City")] District district)
        {
            if (ModelState.IsValid)
            {
                _context.Add(district);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "İlçe başarıyla eklendi!";
                return RedirectToAction(nameof(Index));
            }

            return View(district);
        }

        // GET: Districts/Edit/5 - UPDATE işlemi (form)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var district = await _context.Districts.FindAsync(id);
            if (district == null)
            {
                return NotFound();
            }

            ViewBag.Message = "İlçe Düzenle";
            return View(district);
        }

        // POST: Districts/Edit/5 - UPDATE işlemi (kaydet)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,City")] District district)
        {
            if (id != district.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(district);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "İlçe başarıyla güncellendi!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DistrictExists(district.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(district);
        }

        // GET: Districts/Delete/5 - DELETE işlemi (onay)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var district = await _context.Districts
                .Include(d => d.Listings)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (district == null)
            {
                return NotFound();
            }

            return View(district);
        }

        // POST: Districts/Delete/5 - DELETE işlemi (sil)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var district = await _context.Districts.FindAsync(id);
            if (district != null)
            {
                _context.Districts.Remove(district);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "İlçe başarıyla silindi!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DistrictExists(int id)
        {
            return _context.Districts.Any(e => e.Id == id);
        }
    }
}

