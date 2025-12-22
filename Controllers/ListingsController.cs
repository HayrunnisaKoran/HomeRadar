using Microsoft.AspNetCore.Mvc;
using HomeRadar.Models;
using HomeRadar.Services;
using HomeRadar.Attributes;
using System.IO;

namespace HomeRadar.Controllers
{
    public class ListingsController : Controller
    {
        private readonly IListingService _listingService;
        private readonly AuthService _authService;

        public ListingsController(IListingService listingService, AuthService authService)
        {
            _listingService = listingService;
            _authService = authService;
        }

        // GET: Listings - READ işlemi (Sadece Admin)
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Index()
        {
            var listings = await _listingService.GetActiveListingsAsync();

            // ViewBag kullanımı
            ViewBag.Message = "İlan Listesi";
            ViewBag.TotalCount = listings.Count();
            
            // Rol bazlı içerik farklılığı
            ViewBag.IsAdmin = _authService.IsAdmin();

            return View(listings);
        }

        // GET: Listings/Details/5 - READ işlemi (detay) (Sadece Admin)
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _listingService.GetListingWithDetailsAsync(id.Value);

            if (listing == null)
            {
                return NotFound();
            }

            // ViewData kullanımı
            ViewData["Title"] = "İlan Detayı";
            ViewData["DistrictName"] = listing.District?.Name ?? "Bilinmiyor";

            return View(listing);
        }

        // GET: Listings/Edit/5 - UPDATE işlemi (form) (Sadece Admin)
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _listingService.GetListingByIdAsync(id.Value);
            if (listing == null)
            {
                return NotFound();
            }

            var districts = await _listingService.GetDistrictsAsync();
            var buildingTypes = await _listingService.GetBuildingTypesAsync();

            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(districts, "Id", "Name", listing.DistrictId);
            ViewBag.BuildingTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(buildingTypes, "Id", "Name", listing.BuildingTypeId);
            ViewBag.Message = "İlan Düzenle";

            return View(listing);
        }

        // POST: Listings/Edit/5 - UPDATE işlemi (kaydet) (Sadece Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
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
                    await _listingService.UpdateListingAsync(listing);

                    TempData["SuccessMessage"] = "İlan başarıyla güncellendi!";
                }
                catch
                {
                    if (!await _listingService.ListingExistsAsync(listing.Id))
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

            var districts = await _listingService.GetDistrictsAsync();
            var buildingTypes = await _listingService.GetBuildingTypesAsync();

            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(districts, "Id", "Name", listing.DistrictId);
            ViewBag.BuildingTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(buildingTypes, "Id", "Name", listing.BuildingTypeId);
            return View(listing);
        }

        // GET: Listings/Delete/5 - DELETE işlemi (onay) (Sadece Admin)
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _listingService.GetListingByIdAsync(id.Value);
            if (listing == null)
            {
                return NotFound();
            }

            return View(listing);
        }

        // POST: Listings/Delete/5 - DELETE işlemi (sil) (Sadece Admin)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _listingService.DeleteListingAsync(id);

            TempData["SuccessMessage"] = "İlan başarıyla silindi!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Listings/Import - CSV Import sayfası (Admin only)
        [AuthorizeRole("Admin")]
        public IActionResult Import()
        {
            ViewBag.Message = "CSV'den İlan İçe Aktar";
            return View();
        }

        // POST: Listings/Import - CSV Import işlemi (Admin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Import(string csvFilePath, bool clearBeforeImport = false)
        {
            try
            {
                // CSV dosya yolu kontrolü
                if (string.IsNullOrWhiteSpace(csvFilePath))
                {
                    ModelState.AddModelError("", "CSV dosya yolu belirtilmedi!");
                    return View();
                }

                // Dosya yolunu tam path'e çevir (web'den gelen / karakterlerini \'ye çevir)
                // Path.Combine zaten doğru separator'ı kullanır ama normalize edelim
                var normalizedPath = csvFilePath.Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.IsPathRooted(normalizedPath) 
                    ? normalizedPath 
                    : Path.Combine(Directory.GetCurrentDirectory(), normalizedPath);

                if (!System.IO.File.Exists(fullPath))
                {
                    ModelState.AddModelError("", $"CSV dosyası bulunamadı: {fullPath}");
                    return View();
                }

                // Önce temizleme işlemi (eğer istenirse)
                if (clearBeforeImport)
                {
                    var deletedCount = await _listingService.DeleteAllListingsAsync();
                    TempData["InfoMessage"] = $"🗑️ {deletedCount} mevcut ilan silindi. Yeni import başlatılıyor...";
                }

                // Import işlemini başlat (ONEHOT CSV kontrolü)
                var (successCount, errorCount, errors) = csvFilePath.Contains("ONEHOT", StringComparison.OrdinalIgnoreCase)
                    ? await _listingService.ImportListingsFromOnehotCsvAsync(fullPath)
                    : await _listingService.ImportListingsFromCsvAsync(fullPath);

                if (successCount > 0)
                {
                    TempData["SuccessMessage"] = $"✅ {successCount} ilan başarıyla içe aktarıldı!";
                }

                // Hata mesajlarını göster (errorCount > 0 VEYA errors listesi dolu ise)
                if (errorCount > 0 || errors.Count > 0)
                {
                    var errorDetails = string.Join("<br/>", errors.Take(10)); // İlk 10 hatayı göster
                    if (errorCount > 0)
                    {
                        TempData["ErrorMessage"] = $"⚠️ {errorCount} satırda hata oluştu:<br/>{errorDetails}";
                        if (errors.Count > 10)
                        {
                            TempData["ErrorMessage"] += $"<br/>... ve {errors.Count - 10} hata daha";
                        }
                    }
                    else
                    {
                        // errorCount=0 ama errors listesi dolu (örneğin Districts/BuildingTypes yüklenemedi)
                        TempData["ErrorMessage"] = $"⚠️ İçe aktarma başarısız:<br/>{errorDetails}";
                        if (errors.Count > 10)
                        {
                            TempData["ErrorMessage"] += $"<br/>... ve {errors.Count - 10} hata daha";
                        }
                    }
                }

                if (successCount == 0 && errorCount == 0 && errors.Count == 0)
                {
                    TempData["ErrorMessage"] = "Hiçbir ilan içe aktarılamadı! CSV dosyası boş olabilir veya tüm satırlar geçersiz olabilir.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"İçe aktarma hatası: {ex.Message}");
                return View();
            }
        }
    }
}

