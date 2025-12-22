using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HomeRadar.Services
{
    /// <summary>
    /// ML Service Implementation - Node.js ML API'ye HTTP istek atar
    /// </summary>
    public class MLService : IMLService
    {
        private readonly HttpClient _httpClient;
        private readonly string _mlApiUrl;

        public MLService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Node.js SOA API'nin ML endpoint'i
            _mlApiUrl = "http://localhost:3000/api/v1/predict"; // Varsayılan port
        }

        public async Task<MLPredictionResult> PredictPriceAsync(MLPredictionRequest request)
        {
            try
            {
                // Node.js API'ye gönderilecek format
                var payload = new
                {
                    district = request.District,
                    square_meters = request.SquareMeters,
                    rooms = request.Rooms,
                    living_rooms = request.LivingRooms,
                    building_age = request.BuildingAge,
                    bathrooms = request.Bathrooms,
                    floor = request.Floor ?? 0,
                    balcony = request.Balcony ? 1 : 0,
                    elevator = request.Elevator ? 1 : 0,
                    garage = request.Garage ? 1 : 0,
                    furnished = request.Furnished ? 1 : 0,
                    swap = request.Swap ? 1 : 0,
                    usage_status = request.UsageStatus,
                    building_status = request.BuildingStatus ? 1 : 0,
                    title_deed = request.TitleDeed ? 1 : 0,
                    heating = request.Heating ?? 1
                };

                var json = JsonSerializer.Serialize(payload);
                
                // DEBUG: Gönderilen payload'ı logla
                System.Diagnostics.Debug.WriteLine($"[DEBUG] MLService - Gönderilen payload:");
                System.Diagnostics.Debug.WriteLine($"  URL: {_mlApiUrl}");
                System.Diagnostics.Debug.WriteLine($"  JSON: {json}");
                System.Diagnostics.Debug.WriteLine($"  district: '{payload.district}'");
                System.Diagnostics.Debug.WriteLine($"  square_meters: {payload.square_meters}");
                System.Diagnostics.Debug.WriteLine($"  rooms: {payload.rooms}");
                System.Diagnostics.Debug.WriteLine($"  building_age: {payload.building_age}");
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_mlApiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // DEBUG: Alınan response'u logla
                System.Diagnostics.Debug.WriteLine($"[DEBUG] MLService - Alınan response:");
                System.Diagnostics.Debug.WriteLine($"  StatusCode: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"  ResponseContent: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    if (result.TryGetProperty("success", out var success) && success.GetBoolean())
                    {
                        var prediction = result.GetProperty("prediction");
                        var price = prediction.GetProperty("price").GetDecimal();

                        return new MLPredictionResult
                        {
                            Success = true,
                            PredictedPrice = price,
                            ModelName = "XGBoost", // Model adını response'dan alabilirsiniz
                            ConfidenceScore = null // ML servisi confidence döndürüyorsa buraya eklenebilir
                        };
                    }
                    else
                    {
                        // success: false durumunda error mesajını parse et
                        string errorMessage = "Tahmin yapılırken bir hata oluştu. Lütfen tüm bilgileri kontrol edip tekrar deneyin.";
                        
                        if (result.TryGetProperty("error", out var errorElement))
                        {
                            var errorText = errorElement.GetString() ?? "";
                            errorMessage = GetUserFriendlyErrorMessage(errorText, System.Net.HttpStatusCode.OK);
                        }
                        
                        return new MLPredictionResult
                        {
                            Success = false,
                            ErrorMessage = errorMessage
                        };
                    }
                }
                else
                {
                    // HTTP hata mesajını kullanıcı dostu mesaja çevir
                    var userFriendlyMessage = ParseJsonAndGetUserFriendlyErrorMessage(responseContent, response.StatusCode);
                    
                    return new MLPredictionResult
                    {
                        Success = false,
                        ErrorMessage = userFriendlyMessage
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                // HTTP bağlantı hataları için özel mesaj
                return new MLPredictionResult
                {
                    Success = false,
                    ErrorMessage = "Tahmin servisi şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin."
                };
            }
            catch (TaskCanceledException)
            {
                // Timeout hataları için özel mesaj
                return new MLPredictionResult
                {
                    Success = false,
                    ErrorMessage = "Tahmin servisi yanıt vermiyor. Lütfen daha sonra tekrar deneyin."
                };
            }
            catch (Exception ex)
            {
                // Diğer genel hatalar için kullanıcı dostu mesaj
                // Teknik detayları gizle
                return new MLPredictionResult
                {
                    Success = false,
                    ErrorMessage = "Tahmin yapılırken bir hata oluştu. Lütfen tüm bilgileri kontrol edip tekrar deneyin."
                };
            }
        }

        /// <summary>
        /// JSON response'dan hata mesajını parse edip kullanıcı dostu mesaja çevirir
        /// </summary>
        private string ParseJsonAndGetUserFriendlyErrorMessage(string responseContent, System.Net.HttpStatusCode statusCode)
        {
            // JSON response'dan hata mesajını parse et
            try
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                if (result.TryGetProperty("error", out var errorElement))
                {
                    var errorText = errorElement.GetString() ?? "";
                    return GetUserFriendlyErrorMessage(errorText, statusCode);
                }
            }
            catch
            {
                // JSON parse edilemezse, responseContent'i direkt string olarak kullan
                if (!string.IsNullOrEmpty(responseContent))
                {
                    return GetUserFriendlyErrorMessage(responseContent, statusCode);
                }
            }
            
            // HTTP status code'a göre genel mesajlar
            if (statusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return "Gönderilen bilgiler geçersiz. Lütfen tüm alanları kontrol edip tekrar deneyin.";
            }
            if (statusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return "Tahmin servisi şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin.";
            }
            if (statusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                return "Tahmin servisi şu anda bakımda. Lütfen daha sonra tekrar deneyin.";
            }
            
            // Varsayılan mesaj
            return "Tahmin yapılırken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
        }

        /// <summary>
        /// Hata metnini kullanıcı dostu mesaja çevirir (overload)
        /// </summary>
        private string GetUserFriendlyErrorMessage(string errorText, System.Net.HttpStatusCode statusCode)
        {
            if (string.IsNullOrEmpty(errorText))
            {
                // HTTP status code'a göre genel mesajlar
                if (statusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return "Gönderilen bilgiler geçersiz. Lütfen tüm alanları kontrol edip tekrar deneyin.";
                }
                if (statusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    return "Tahmin servisi şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin.";
                }
                return "Tahmin yapılırken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
            }
            
            // Eksik alan hatası - sadece gerçekten eksik alan varsa göster
            // Önce "eksik", "required", "missing" gibi kelimeleri kontrol et
            bool isMissingFieldError = errorText.Contains("eksik", StringComparison.OrdinalIgnoreCase) ||
                                      errorText.Contains("required", StringComparison.OrdinalIgnoreCase) ||
                                      errorText.Contains("missing", StringComparison.OrdinalIgnoreCase) ||
                                      errorText.Contains("Tüm alanlar gereklidir", StringComparison.OrdinalIgnoreCase) ||
                                      errorText.Contains("All fields are required", StringComparison.OrdinalIgnoreCase);
            
            if (isMissingFieldError)
            {
                // Hangi alanların eksik olduğunu kontrol et
                var missingFields = new List<string>();
                
                if (errorText.Contains("district", StringComparison.OrdinalIgnoreCase))
                    missingFields.Add("İlçe");
                if (errorText.Contains("square_meters", StringComparison.OrdinalIgnoreCase))
                    missingFields.Add("Metrekare");
                if (errorText.Contains("rooms", StringComparison.OrdinalIgnoreCase))
                    missingFields.Add("Oda Sayısı");
                if (errorText.Contains("building_age", StringComparison.OrdinalIgnoreCase))
                    missingFields.Add("Bina Yaşı");
                
                if (missingFields.Any())
                {
                    return $"Lütfen şu alanları doldurun: {string.Join(", ", missingFields)}.";
                }
                
                return "Lütfen tüm zorunlu alanları doldurun: İlçe, Metrekare, Oda Sayısı ve Bina Yaşı.";
            }
            
            // Diğer hata mesajları için genel mesaj (sadece gerçekten ilgili hata varsa)
            // Burada "eksik", "required", "missing" kelimeleri olmadan hata mesajı döndürmüyoruz
            
            // Genel hata mesajı (teknik detayları gizle)
            return "Tahmin yapılırken bir hata oluştu. Lütfen tüm bilgileri kontrol edip tekrar deneyin.";
        }
    }
}

