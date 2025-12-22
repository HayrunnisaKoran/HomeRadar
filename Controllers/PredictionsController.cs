using Microsoft.AspNetCore.Mvc;
using HomeRadar.Models;
using HomeRadar.Services;
using HomeRadar.Attributes;
using System;
using System.Linq;

namespace HomeRadar.Controllers
{
    public class PredictionsController : Controller
    {
        private readonly IPredictionService _predictionService;
        private readonly AuthService _authService;

        public PredictionsController(IPredictionService predictionService, AuthService authService)
        {
            _predictionService = predictionService;
            _authService = authService;
        }

        // GET: Predictions - READ işlemi
        public async Task<IActionResult> Index()
        {
            var predictions = await _predictionService.GetPredictionsWithDetailsAsync();

            ViewBag.Message = "Tahmin Geçmişi";
            ViewBag.TotalPredictions = predictions.Count();
            ViewBag.AvgPrice = await _predictionService.GetAveragePredictedPriceAsync();

            return View(predictions);
        }

        // GET: Predictions/Details/5 - READ işlemi (detay)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prediction = await _predictionService.GetPredictionWithDetailsAsync(id.Value);

            if (prediction == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Tahmin Detayı";
            ViewData["PriceRange"] = $"{prediction.PredictedPriceMin:C0} - {prediction.PredictedPriceMax:C0}";

            return View(prediction);
        }

        // GET: Predictions/Create - CREATE işlemi (form)
        public async Task<IActionResult> Create()
        {
            var districts = await _predictionService.GetDistrictsAsync();
            var buildingTypes = await _predictionService.GetBuildingTypesAsync();

            // İlçe isimlerini düzgün görüntülemek için SelectList oluştur
            // Veritabanındaki isimler doğru ama view'da encoding sorunu olmaması için
            var districtItems = districts.Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name // ASP.NET Core otomatik HTML encode eder, encoding sorunu olmamalı
            }).ToList();

            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(districtItems, "Value", "Text");
            ViewBag.BuildingTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(buildingTypes, "Id", "Name");
            ViewBag.Message = "Yeni Tahmin Oluştur";

            return View();
        }

        // POST: Predictions/Create - CREATE işlemi (ML ile tahmin yap ve kaydet)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int DistrictId,
            int? BuildingTypeId,
            int RoomCount,
            decimal SquareMeters,
            int BuildingAge,
            int LivingRooms,
            int Bathrooms,
            int? Floor,
            bool Balcony,
            bool Elevator,
            bool Garage,
            bool Furnished,
            bool Swap,
            string UsageStatus,
            bool BuildingStatus,
            bool TitleDeed,
            int Heating)
        {
            try
            {
                // DEBUG: Form verilerini logla
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Form verileri alındı:");
                System.Diagnostics.Debug.WriteLine($"  DistrictId: {DistrictId}");
                System.Diagnostics.Debug.WriteLine($"  BuildingTypeId: {BuildingTypeId}");
                System.Diagnostics.Debug.WriteLine($"  RoomCount: {RoomCount}");
                System.Diagnostics.Debug.WriteLine($"  SquareMeters: {SquareMeters}");
                System.Diagnostics.Debug.WriteLine($"  BuildingAge: {BuildingAge}");
                System.Diagnostics.Debug.WriteLine($"  LivingRooms: {LivingRooms}");
                System.Diagnostics.Debug.WriteLine($"  Bathrooms: {Bathrooms}");
                System.Diagnostics.Debug.WriteLine($"  Floor: {Floor}");
                
                // Session'dan kullanıcı ID'sini al (eğer giriş yapılmışsa)
                var userId = _authService.GetUserId();
                
                // District name'i al
                var districts = await _predictionService.GetDistrictsAsync();
                var district = districts.FirstOrDefault(d => d.Id == DistrictId);
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] District lookup:");
                System.Diagnostics.Debug.WriteLine($"  DistrictId: {DistrictId}");
                System.Diagnostics.Debug.WriteLine($"  District found: {district != null}");
                if (district != null)
                {
                    System.Diagnostics.Debug.WriteLine($"  District.Name (original): '{district.Name}'");
                }
                
                if (district == null)
                {
                    ModelState.AddModelError("", "İlçe bulunamadı!");
                    return await ReloadCreateView(DistrictId, BuildingTypeId, userId);
                }

                // İlçe ismini ML modelinin beklediği formata normalize et (Türkçe karakterleri kaldır)
                // Python modeli: Ilce_Sehzadeler, Ilce_Alasehir gibi format bekliyor (Türkçe karakter yok)
                var normalizedDistrictName = NormalizeDistrictNameForML(district.Name);
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] District normalization:");
                System.Diagnostics.Debug.WriteLine($"  Original: '{district.Name}'");
                System.Diagnostics.Debug.WriteLine($"  Normalized: '{normalizedDistrictName}'");
                System.Diagnostics.Debug.WriteLine($"  IsNullOrWhiteSpace: {string.IsNullOrWhiteSpace(normalizedDistrictName)}");
                
                // Normalize edilmiş ilçe adının boş olmadığından emin ol
                if (string.IsNullOrWhiteSpace(normalizedDistrictName))
                {
                    ModelState.AddModelError("", "İlçe adı geçersiz!");
                    return await ReloadCreateView(DistrictId, BuildingTypeId, userId);
                }

                // ML Request oluştur
                var mlRequest = new MLPredictionRequest
                {
                    District = normalizedDistrictName, // Normalize edilmiş ilçe adı (ML modeli için)
                    SquareMeters = SquareMeters,
                    Rooms = RoomCount,
                    LivingRooms = LivingRooms,
                    BuildingAge = BuildingAge,
                    Bathrooms = Bathrooms,
                    Floor = Floor,
                    Balcony = Balcony,
                    Elevator = Elevator,
                    Garage = Garage,
                    Furnished = Furnished,
                    Swap = Swap,
                    UsageStatus = UsageStatus ?? "Owner",
                    BuildingStatus = BuildingStatus,
                    TitleDeed = TitleDeed,
                    Heating = Heating
                };
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ML Request oluşturuldu:");
                System.Diagnostics.Debug.WriteLine($"  District: '{mlRequest.District}'");
                System.Diagnostics.Debug.WriteLine($"  SquareMeters: {mlRequest.SquareMeters}");
                System.Diagnostics.Debug.WriteLine($"  Rooms: {mlRequest.Rooms}");
                System.Diagnostics.Debug.WriteLine($"  BuildingAge: {mlRequest.BuildingAge}");
                System.Diagnostics.Debug.WriteLine($"  LivingRooms: {mlRequest.LivingRooms}");
                System.Diagnostics.Debug.WriteLine($"  Bathrooms: {mlRequest.Bathrooms}");

                // ML servisi ile tahmin yap ve kaydet (userId session'dan geliyor, BuildingTypeId form'dan geliyor)
                var prediction = await _predictionService.CreatePredictionWithMLAsync(mlRequest, userId, BuildingTypeId);

                TempData["SuccessMessage"] = $"Tahmin başarıyla yapıldı ve kaydedildi! Tahmini fiyat: {prediction.PredictedPriceAvg:C0}";
                return RedirectToAction(nameof(Details), new { id = prediction.Id });
            }
            catch (Exception ex)
            {
                // Kullanıcı dostu hata mesajı oluştur (teknik detayları gizle)
                string userFriendlyMessage;
                
                // ML servisi hataları için mesaj zaten kullanıcı dostu
                if (ex.Message.Contains("Lütfen") || ex.Message.Contains("eksik") || ex.Message.Contains("geçersiz") || 
                    ex.Message.Contains("Tahmin servisi") || ex.Message.Contains("kullanılamıyor"))
                {
                    userFriendlyMessage = ex.Message;
                }
                // Veritabanı hataları için
                else if (ex is Microsoft.EntityFrameworkCore.DbUpdateException)
                {
                    userFriendlyMessage = "Tahmin kaydedilirken bir hata oluştu. Lütfen tekrar deneyin.";
                }
                // İlçe bulunamadı hatası
                else if (ex.Message.Contains("İlçe bulunamadı"))
                {
                    userFriendlyMessage = "Seçilen ilçe bulunamadı. Lütfen geçerli bir ilçe seçin.";
                }
                // Diğer hatalar için genel mesaj
                else
                {
                    userFriendlyMessage = "Tahmin yapılırken bir hata oluştu. Lütfen tüm bilgileri kontrol edip tekrar deneyin.";
                }
                
                // Session'dan kullanıcı ID'sini al (hata durumunda)
                var userId = _authService.GetUserId();
                
                ModelState.AddModelError("", userFriendlyMessage);
                return await ReloadCreateView(DistrictId, BuildingTypeId, userId);
            }
        }

        /// <summary>
        /// İlçe ismini ML modelinin beklediği formata normalize eder (Türkçe karakterleri kaldırır)
        /// Python modeli: Ilce_Sehzadeler, Ilce_Alasehir gibi format bekliyor
        /// </summary>
        private string NormalizeDistrictNameForML(string districtName)
        {
            if (string.IsNullOrEmpty(districtName))
                return districtName;

            // Türkçe karakterleri İngilizce karşılıklarına çevir
            var normalized = districtName
                .Replace("Ş", "S")
                .Replace("ş", "s")
                .Replace("Ğ", "G")
                .Replace("ğ", "g")
                .Replace("İ", "I")
                .Replace("ı", "i")
                .Replace("Ö", "O")
                .Replace("ö", "o")
                .Replace("Ü", "U")
                .Replace("ü", "u")
                .Replace("Ç", "C")
                .Replace("ç", "c");

            return normalized;
        }

        private async Task<IActionResult> ReloadCreateView(int? districtId = null, int? buildingTypeId = null, int? userId = null)
        {
            var districts = await _predictionService.GetDistrictsAsync();
            var buildingTypes = await _predictionService.GetBuildingTypesAsync();

            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(districts, "Id", "Name", districtId);
            ViewBag.BuildingTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(buildingTypes, "Id", "Name", buildingTypeId);
            ViewBag.Message = "Yeni Tahmin Oluştur";

            return View(new Prediction());
        }

        // GET: Predictions/Delete/5 - DELETE işlemi (onay)
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prediction = await _predictionService.GetPredictionWithDetailsAsync(id.Value);

            if (prediction == null)
            {
                return NotFound();
            }

            return View(prediction);
        }

        // POST: Predictions/Delete/5 - DELETE işlemi (sil)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _predictionService.DeletePredictionAsync(id);

            TempData["SuccessMessage"] = "Tahmin başarıyla silindi!";

            return RedirectToAction(nameof(Index));
        }
    }
}

