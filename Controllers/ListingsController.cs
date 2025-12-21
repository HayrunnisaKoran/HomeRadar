using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeRadar.Data;
using HomeRadar.Models;
using HomeRadar.Services;

namespace HomeRadar.Controllers
{
    public class ListingsController : Controller
    {
        private readonly EmlakContext _context;
        private readonly AuthService _authService;

        public ListingsController(EmlakContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // GET: Listings - READ işlemi
        public async Task<IActionResult> Index()
        {
            var listings = await _context.Listings
                .Include(l => l.District)
                .Include(l => l.BuildingType)
                .Where(l => l.IsActive)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            // ViewBag kullanımı
            ViewBag.Message = "İlan Listesi";
            ViewBag.TotalCount = listings.Count;
            
            // Rol bazlı içerik farklılığı
            ViewBag.IsAdmin = _authService.IsAdmin();
            ViewBag.CanCreate = _authService.IsAuthenticated(); // Giriş yapmış kullanıcılar oluşturabilir

            return View(listings);
        }

        // GET: Listings/Details/5 - READ işlemi (detay)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _context.Listings
                .Include(l => l.District)
                .Include(l => l.BuildingType)
                .Include(l => l.ListingFeatures!)
                    .ThenInclude(lf => lf.Feature)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (listing == null)
            {
                return NotFound();
            }

            // ViewData kullanımı
            ViewData["Title"] = "İlan Detayı";
            ViewData["DistrictName"] = listing.District?.Name ?? "Bilinmiyor";

            return View(listing);
        }

        // GET: Listings/Create - CREATE işlemi (form)
        public IActionResult Create()
        {
            // ViewBag ile dropdown verileri (SelectList kullanarak)
            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.Districts.OrderBy(d => d.Name), "Id", "Name");
            ViewBag.BuildingTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.BuildingTypes.OrderBy(b => b.Name), "Id", "Name");
            ViewBag.Features = _context.Features.OrderBy(f => f.Name).ToList();

            ViewBag.Message = "Yeni İlan Ekle";
            return View();
        }

        // POST: Listings/Create - CREATE işlemi (kaydet)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DistrictId,BuildingTypeId,Price,SquareMeters,RoomCount,SalonCount,BuildingAge")] Listing listing)
        {
            if (ModelState.IsValid)
            {
                listing.CreatedAt = DateTime.Now;
                listing.ListingDate = DateTime.Now;
                listing.IsActive = true;

                _context.Add(listing);
                await _context.SaveChangesAsync();

                // TempData kullanımı - başarı mesajı
                TempData["SuccessMessage"] = "İlan başarıyla eklendi!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.Districts.OrderBy(d => d.Name), "Id", "Name", listing.DistrictId);
            ViewBag.BuildingTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.BuildingTypes.OrderBy(b => b.Name), "Id", "Name", listing.BuildingTypeId);
            return View(listing);
        }

        // GET: Listings/Edit/5 - UPDATE işlemi (form)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _context.Listings.FindAsync(id);
            if (listing == null)
            {
                return NotFound();
            }

            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.Districts.OrderBy(d => d.Name), "Id", "Name", listing.DistrictId);
            ViewBag.BuildingTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.BuildingTypes.OrderBy(b => b.Name), "Id", "Name", listing.BuildingTypeId);
            ViewBag.Message = "İlan Düzenle";

            return View(listing);
        }

        // POST: Listings/Edit/5 - UPDATE işlemi (kaydet)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DistrictId,BuildingTypeId,Price,SquareMeters,RoomCount,SalonCount,BuildingAge,CreatedAt,IsActive")] Listing listing)
        {
            if (id != listing.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listing);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "İlan başarıyla güncellendi!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListingExists(listing.Id))
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

            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.Districts.OrderBy(d => d.Name), "Id", "Name", listing.DistrictId);
            ViewBag.BuildingTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.BuildingTypes.OrderBy(b => b.Name), "Id", "Name", listing.BuildingTypeId);
            return View(listing);
        }

        // GET: Listings/Delete/5 - DELETE işlemi (onay)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _context.Listings
                .Include(l => l.District)
                .Include(l => l.BuildingType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (listing == null)
            {
                return NotFound();
            }

            return View(listing);
        }

        // POST: Listings/Delete/5 - DELETE işlemi (sil)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing != null)
            {
                // Soft delete - IsActive = false
                listing.IsActive = false;
                _context.Update(listing);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "İlan başarıyla silindi!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ListingExists(int id)
        {
            return _context.Listings.Any(e => e.Id == id);
        }
    }
}

