using System.Threading.Tasks;

namespace HomeRadar.Services
{
    /// <summary>
    /// ML Service Interface - Machine Learning modeli için tahmin servisi
    /// </summary>
    public interface IMLService
    {
        Task<MLPredictionResult> PredictPriceAsync(MLPredictionRequest request);
    }

    /// <summary>
    /// ML Tahmin İsteği
    /// </summary>
    public class MLPredictionRequest
    {
        public string District { get; set; } = string.Empty; // İlçe adı (string)
        public decimal SquareMeters { get; set; }
        public int Rooms { get; set; }
        public int LivingRooms { get; set; } = 1;
        public int BuildingAge { get; set; }
        public int Bathrooms { get; set; } = 1;
        public int? Floor { get; set; }
        public bool Balcony { get; set; }
        public bool Elevator { get; set; }
        public bool Garage { get; set; }
        public bool Furnished { get; set; }
        public bool Swap { get; set; }
        public string UsageStatus { get; set; } = "Owner"; // "Empty", "Tenant", "Owner"
        public bool BuildingStatus { get; set; } // 0=Sıfır, 1=İkinci El
        public bool TitleDeed { get; set; } = true;
        public int? Heating { get; set; } = 1; // 0=Yok, 1-3=Çeşitli
    }

    /// <summary>
    /// ML Tahmin Sonucu
    /// </summary>
    public class MLPredictionResult
    {
        public bool Success { get; set; }
        public decimal PredictedPrice { get; set; }
        public string? ModelName { get; set; }
        public decimal? ConfidenceScore { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

