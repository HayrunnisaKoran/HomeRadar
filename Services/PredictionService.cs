using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeRadar.Models;
using HomeRadar.Repositories;

namespace HomeRadar.Services
{
    /// <summary>
    /// Prediction Service Implementation - Tahminler için iş mantığı
    /// </summary>
    public class PredictionService : IPredictionService
    {
        private readonly IPredictionRepository _predictionRepository;
        private readonly IDistrictRepository _districtRepository;
        private readonly IBuildingTypeRepository _buildingTypeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMLService _mlService;

        public PredictionService(
            IPredictionRepository predictionRepository,
            IDistrictRepository districtRepository,
            IBuildingTypeRepository buildingTypeRepository,
            IUserRepository userRepository,
            IMLService mlService)
        {
            _predictionRepository = predictionRepository;
            _districtRepository = districtRepository;
            _buildingTypeRepository = buildingTypeRepository;
            _userRepository = userRepository;
            _mlService = mlService;
        }

        public async Task<IEnumerable<Prediction>> GetAllPredictionsAsync()
        {
            return await _predictionRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Prediction>> GetPredictionsWithDetailsAsync()
        {
            return await _predictionRepository.GetPredictionsWithDetailsAsync();
        }

        public async Task<Prediction?> GetPredictionByIdAsync(int id)
        {
            return await _predictionRepository.GetByIdAsync(id);
        }

        public async Task<Prediction?> GetPredictionWithDetailsAsync(int id)
        {
            return await _predictionRepository.GetPredictionWithDetailsAsync(id);
        }

        public async Task<IEnumerable<Prediction>> GetPredictionsByUserAsync(int userId)
        {
            return await _predictionRepository.GetPredictionsByUserAsync(userId);
        }

        public async Task<Prediction> CreatePredictionAsync(Prediction prediction)
        {
            prediction.CreatedAt = System.DateTime.UtcNow;

            // Ortalama fiyat hesapla
            if (prediction.PredictedPriceAvg == 0 && prediction.PredictedPriceMin > 0 && prediction.PredictedPriceMax > 0)
            {
                prediction.PredictedPriceAvg = (prediction.PredictedPriceMin + prediction.PredictedPriceMax) / 2;
            }

            return await _predictionRepository.AddAsync(prediction);
        }

        public async Task UpdatePredictionAsync(Prediction prediction)
        {
            await _predictionRepository.UpdateAsync(prediction);
        }

        public async Task DeletePredictionAsync(int id)
        {
            await _predictionRepository.DeleteAsync(id);
        }

        public async Task<bool> PredictionExistsAsync(int id)
        {
            return await _predictionRepository.AnyAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<District>> GetDistrictsAsync()
        {
            var districts = await _districtRepository.GetAllAsync();
            // Duplicate'leri kaldır (Name'e göre unique)
            return districts
                .GroupBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(d => d.Name);
        }

        public async Task<IEnumerable<BuildingType>> GetBuildingTypesAsync()
        {
            var buildingTypes = await _buildingTypeRepository.GetAllAsync();
            // Duplicate'leri kaldır (Name'e göre unique)
            return buildingTypes
                .GroupBy(bt => bt.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(bt => bt.Name);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _userRepository.GetActiveUsersAsync();
        }

        public async Task<decimal> GetAveragePredictedPriceAsync()
        {
            var predictions = await _predictionRepository.GetAllAsync();
            var predictionsList = predictions.ToList();
            
            if (!predictionsList.Any())
                return 0;

            return predictionsList.Average(p => p.PredictedPriceAvg);
        }

        /// <summary>
        /// ML servisi ile tahmin yap ve veritabanına kaydet
        /// </summary>
        public async Task<Prediction> CreatePredictionWithMLAsync(MLPredictionRequest request, int? userId = null, int? buildingTypeId = null)
        {
            // 1. ML servisine istek at
            var mlResult = await _mlService.PredictPriceAsync(request);

            if (!mlResult.Success)
            {
                // Kullanıcı dostu hata mesajı (teknik detayları gizle)
                throw new Exception(mlResult.ErrorMessage);
            }

            // 2. District ID'yi bul (District name'den - ML'den gelen normalize edilmiş isim ile)
            // ML servisine normalize edilmiş isim gönderildiği için, veritabanında da normalize edilmiş isimle arama yapıyoruz
            var normalizedRequestDistrict = NormalizeDistrictNameForML(request.District);
            var district = (await _districtRepository.GetAllAsync())
                .FirstOrDefault(d => NormalizeDistrictNameForML(d.Name).Equals(normalizedRequestDistrict, StringComparison.OrdinalIgnoreCase));

            if (district == null)
            {
                throw new Exception($"İlçe bulunamadı: {request.District}");
            }
            
            // District'in veritabanında gerçekten var olduğundan emin ol
            var districtExists = await _districtRepository.GetByIdAsync(district.Id);
            if (districtExists == null)
            {
                throw new Exception($"İlçe ID'si geçersiz: {district.Id} (İlçe: {request.District})");
            }

            // 3. Tahmin fiyatından min/max hesapla (%10 tolerans)
            var predictedPrice = mlResult.PredictedPrice;
            
            // Database constraint kontrolü: PredictedPriceMin > 0 olmalı
            if (predictedPrice <= 0)
            {
                throw new Exception($"ML servisi geçersiz fiyat döndü: {predictedPrice}. Fiyat 0'dan büyük olmalıdır.");
            }
            
            var minPrice = Math.Max(predictedPrice * 0.9m, 1m); // En az 1 TL
            var maxPrice = predictedPrice * 1.1m;
            var avgPrice = predictedPrice;
            
            // Constraint kontrolü: maxPrice >= minPrice (her zaman doğru olmalı ama emin olalım)
            if (maxPrice < minPrice)
            {
                maxPrice = minPrice;
            }

            // 4. BuildingTypeId kontrolü (eğer verilmişse)
            int? validBuildingTypeId = null;
            if (buildingTypeId.HasValue && buildingTypeId.Value > 0)
            {
                var buildingType = await _buildingTypeRepository.GetByIdAsync(buildingTypeId.Value);
                if (buildingType != null)
                {
                    validBuildingTypeId = buildingTypeId.Value;
                }
            }

            // 5. Prediction nesnesi oluştur
            var prediction = new Prediction
            {
                UserId = userId,
                DistrictId = district.Id,
                RoomCount = request.Rooms,
                SquareMeters = request.SquareMeters,
                BuildingAge = request.BuildingAge,
                BuildingTypeId = validBuildingTypeId, // Form'dan gelen bina tipi ID'si
                PredictedPriceMin = minPrice,
                PredictedPriceMax = maxPrice,
                PredictedPriceAvg = avgPrice,
                ModelName = mlResult.ModelName ?? "XGBoost",
                ConfidenceScore = mlResult.ConfidenceScore,
                CreatedAt = DateTime.UtcNow
            };

            // 6. Validation: Tüm gerekli alanların dolu olduğundan emin ol
            if (prediction.DistrictId <= 0)
                throw new Exception("Geçersiz DistrictId");
            if (prediction.RoomCount <= 0)
                throw new Exception("Oda sayısı 0'dan büyük olmalıdır");
            if (prediction.SquareMeters <= 0)
                throw new Exception("Metrekare 0'dan büyük olmalıdır");
            if (prediction.BuildingAge < 0)
                throw new Exception("Bina yaşı negatif olamaz");
            if (prediction.PredictedPriceMin <= 0)
                throw new Exception("Minimum tahmini fiyat 0'dan büyük olmalıdır");
            if (prediction.PredictedPriceMax <= 0)
                throw new Exception("Maximum tahmini fiyat 0'dan büyük olmalıdır");
            if (prediction.PredictedPriceAvg <= 0)
                throw new Exception("Ortalama tahmini fiyat 0'dan büyük olmalıdır");
            if (prediction.PredictedPriceMax < prediction.PredictedPriceMin)
                throw new Exception("Maximum fiyat minimum fiyattan küçük olamaz");

            // 7. Veritabanına kaydet
            return await _predictionRepository.AddAsync(prediction);
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
    }
}

