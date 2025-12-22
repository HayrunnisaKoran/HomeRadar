using Microsoft.AspNetCore.Mvc;
using HomeRadar.Models;
using HomeRadar.Services;
using HomeRadar.Attributes;

namespace HomeRadar.Controllers
{
    public class DistrictsController : Controller
    {
        private readonly IDistrictService _districtService;

        public DistrictsController(IDistrictService districtService)
        {
            _districtService = districtService;
        }

        // GET: Districts - READ işlemi
        public async Task<IActionResult> Index()
        {
            var districts = await _districtService.GetDistrictsWithListingsAsync();

            // ViewBag ile istatistikler
            ViewBag.Message = "İlçe Yönetimi";
            ViewBag.TotalDistricts = districts.Count();
            ViewBag.TotalListings = await _districtService.GetTotalActiveListingsCountAsync();

            return View(districts);
        }

        // GET: Districts/Details/5 - READ işlemi (detay)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var district = await _districtService.GetDistrictWithListingsAsync(id.Value);

            if (district == null)
            {
                return NotFound();
            }

            ViewData["Title"] = district.Name + " Detayı";
            ViewData["ListingCount"] = district.Listings?.Count(l => l.IsActive) ?? 0;

            return View(district);
        }

        // GET: Districts/Create - CREATE işlemi (form)
        [AuthorizeRole("Admin")]
        public IActionResult Create()
        {
            ViewBag.Message = "Yeni İlçe Ekle";
            return View();
        }

        // POST: Districts/Create - CREATE işlemi (kaydet)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Create([Bind("Name,City")] District district)
        {
            if (ModelState.IsValid)
            {
                await _districtService.CreateDistrictAsync(district);

                TempData["SuccessMessage"] = "İlçe başarıyla eklendi!";
                return RedirectToAction(nameof(Index));
            }

            return View(district);
        }

        // GET: Districts/Edit/5 - UPDATE işlemi (form)
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var district = await _districtService.GetDistrictByIdAsync(id.Value);
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
        [AuthorizeRole("Admin")]
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
                    await _districtService.UpdateDistrictAsync(district);

                    TempData["SuccessMessage"] = "İlçe başarıyla güncellendi!";
                }
                catch
                {
                    if (!await _districtService.DistrictExistsAsync(district.Id))
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
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var district = await _districtService.GetDistrictWithListingsAsync(id.Value);

            if (district == null)
            {
                return NotFound();
            }

            return View(district);
        }

        // POST: Districts/Delete/5 - DELETE işlemi (sil)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _districtService.DeleteDistrictAsync(id);

            TempData["SuccessMessage"] = "İlçe başarıyla silindi!";

            return RedirectToAction(nameof(Index));
        }
    }
}

