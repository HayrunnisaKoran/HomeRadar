using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HomeRadar.Models;
using HomeRadar.Repositories;
using HomeRadar.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Services
{
    /// <summary>
    /// Listing Service Implementation - İlanlar için iş mantığı
    /// </summary>
    public class ListingService : IListingService
    {
        private readonly IListingRepository _listingRepository;
        private readonly IDistrictRepository _districtRepository;
        private readonly IBuildingTypeRepository _buildingTypeRepository;
        private readonly IFeatureRepository _featureRepository;
        private readonly Data.EmlakContext _context;

        public ListingService(
            IListingRepository listingRepository,
            IDistrictRepository districtRepository,
            IBuildingTypeRepository buildingTypeRepository,
            IFeatureRepository featureRepository,
            Data.EmlakContext context)
        {
            _listingRepository = listingRepository;
            _districtRepository = districtRepository;
            _buildingTypeRepository = buildingTypeRepository;
            _featureRepository = featureRepository;
            _context = context;
        }

        public async Task<IEnumerable<Listing>> GetAllListingsAsync()
        {
            return await _listingRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Listing>> GetActiveListingsAsync()
        {
            return await _listingRepository.GetActiveListingsAsync();
        }

        public async Task<Listing?> GetListingByIdAsync(int id)
        {
            return await _listingRepository.GetByIdAsync(id);
        }

        public async Task<Listing?> GetListingWithDetailsAsync(int id)
        {
            return await _listingRepository.GetListingWithDetailsAsync(id);
        }

        public async Task<IEnumerable<Listing>> GetListingsByDistrictAsync(int districtId)
        {
            return await _listingRepository.GetListingsByDistrictAsync(districtId);
        }

        public async Task<Listing> CreateListingAsync(Listing listing)
        {
            listing.CreatedAt = System.DateTime.UtcNow;
            listing.ListingDate = System.DateTime.UtcNow;
            listing.IsActive = true;
            return await _listingRepository.AddAsync(listing);
        }

        public async Task UpdateListingAsync(Listing listing)
        {
            await _listingRepository.UpdateAsync(listing);
        }

        public async Task DeleteListingAsync(int id)
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing != null)
            {
                // Soft delete
                listing.IsActive = false;
                await _listingRepository.UpdateAsync(listing);
            }
        }

        public async Task<bool> ListingExistsAsync(int id)
        {
            return await _listingRepository.AnyAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<District>> GetDistrictsAsync()
        {
            return await _districtRepository.GetAllAsync();
        }

        public async Task<IEnumerable<BuildingType>> GetBuildingTypesAsync()
        {
            return await _buildingTypeRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Feature>> GetFeaturesAsync()
        {
            return await _featureRepository.GetAllAsync();
        }

        /// <summary>
        /// CSV dosyasından ilanları import eder
        /// </summary>
        public async Task<(int SuccessCount, int ErrorCount, List<string> Errors)> ImportListingsFromCsvAsync(string csvFilePath)
        {
            var successCount = 0;
            var errorCount = 0;
            var errors = new List<string>();

            if (!File.Exists(csvFilePath))
            {
                errors.Add($"CSV dosyası bulunamadı: {csvFilePath}");
                return (0, 0, errors);
            }

            // ÖNCE: Districts tablosundaki encoding hatalarını düzelt (GetAllAsync çağrılmadan önce)
            // Bu, veritabanından okuma yapmadan önce bozuk kayıtları düzeltir
            // .NET string parametreleri ile direkt UPDATE
            try
            {
                await _context.Database.ExecuteSqlRawAsync(@"UPDATE ""Districts"" SET ""Name"" = {0} WHERE ""Id"" = 2", "Şehzadeler");
                await _context.Database.ExecuteSqlRawAsync(@"UPDATE ""Districts"" SET ""Name"" = {0} WHERE ""Id"" = 7", "Alaşehir");
                await _context.Database.ExecuteSqlRawAsync(@"UPDATE ""Districts"" SET ""Name"" = {0} WHERE ""Id"" = 8", "Saruhanlı");
            }
            catch
            {
                // Düzeltme başarısız olsa bile devam et (belki zaten düzeltilmiş)
            }

            // District ve BuildingType cache'leri
            // Encoding hatası olabileceği için try-catch ile koruyoruz
            List<District> districts;
            List<BuildingType> buildingTypes;
            
            try
            {
                districts = (await _districtRepository.GetAllAsync()).ToList();
            }
            catch (Exception ex)
            {
                errors.Add($"⚠️ İlçeler yüklenemedi (encoding hatası). Districts tablosunda geçersiz UTF-8 karakterler var. Lütfen Districts tablosundaki TÜM kayıtları kontrol edin ve Türkçe karakter içeren kayıtları düzeltin: {ex.Message}");
                return (0, 0, errors);
            }
            
            try
            {
                buildingTypes = (await _buildingTypeRepository.GetAllAsync()).ToList();
            }
            catch (Exception ex)
            {
                errors.Add($"⚠️ Bina tipleri yüklenemedi (encoding hatası). Veritabanındaki BuildingTypes tablosunda geçersiz UTF-8 karakterler var. Lütfen BuildingTypes tablosunu kontrol edin: {ex.Message}");
                return (0, 0, errors);
            }
            
            // Default BuildingType (Daire)
            var defaultBuildingType = buildingTypes.FirstOrDefault(bt => bt.Name.Equals("Daire", StringComparison.OrdinalIgnoreCase));
            if (defaultBuildingType == null)
            {
                errors.Add("Varsayılan bina tipi (Daire) bulunamadı!");
                return (0, 0, errors);
            }

            // Eksik ilçeleri topla (CSV'den okuyup kontrol et)
            var missingDistricts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var listingsToAdd = new List<Listing>();
            var lineNumber = 0;

            // NOT: Duplicate kontrolü kaldırıldı - "Önce temizle" seçeneği kullanılıyor
            // Veritabanında encoding hatası olan kayıtlar olduğunda GetAllAsync() hata veriyor

            try
            {
                // CSV dosyası UTF-8 BOM ile başlıyor, UTF-8 encoding kullan
                // StreamReader BOM'u otomatik algılayacak (detectEncodingFromByteOrderMarks: true)
                using var reader = new StreamReader(csvFilePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                
                // Header satırını oku
                var headerLine = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(headerLine))
                {
                    errors.Add("CSV dosyası boş veya header satırı bulunamadı!");
                    return (0, 0, errors);
                }

                // CSV satırlarını oku
                while (!reader.EndOfStream)
                {
                    lineNumber++;
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        // İlçe adını önce kontrol et (parse etmeden önce)
                        var values = ParseCsvLineValues(line);
                        if (values.Count >= 1)
                        {
                            var ilceName = values[0]?.Trim() ?? "";
                            var normalizedIlce = NormalizeTurkishChars(ilceName);
                            var districtExists = districts.Any(d => 
                                NormalizeTurkishChars(d.Name).Equals(normalizedIlce, StringComparison.OrdinalIgnoreCase));
                            
                            if (!districtExists && !string.IsNullOrWhiteSpace(ilceName))
                            {
                                missingDistricts.Add(ilceName);
                            }
                        }

                        var listing = ParseCsvLine(line, districts, defaultBuildingType.Id, lineNumber);
                        if (listing != null)
                        {
                            // Duplicate kontrolü kaldırıldı - direkt ekle
                            listingsToAdd.Add(listing);
                        }
                        else
                        {
                            errorCount++;
                            errors.Add($"Satır {lineNumber}: Parse edilemedi");
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.Add($"Satır {lineNumber}: {ex.Message}");
                    }
                }

                // Toplu ekleme (performans için)
                if (listingsToAdd.Any())
                {
                    await _listingRepository.AddRangeAsync(listingsToAdd);
                    successCount = listingsToAdd.Count;
                }
                else if (lineNumber == 0)
                {
                    // Hiç satır okunmadı (sadece header var veya dosya boş)
                    errors.Add("⚠️ CSV dosyasında veri satırı bulunamadı! Dosya sadece header içeriyor olabilir.");
                }

                // Eksik ilçeler varsa kullanıcıyı bilgilendir
                if (missingDistricts.Any())
                {
                    var missingList = string.Join(", ", missingDistricts.OrderBy(d => d));
                    errors.Insert(0, $"⚠️ Eksik İlçeler (veritabanına eklenmeli): {missingList}. Bu ilçeler için ilanlar eklenemiyor.");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"CSV okuma hatası: {ex.Message}");
            }

            return (successCount, errorCount, errors);
        }

        private Listing? ParseCsvLine(string line, List<District> districts, int defaultBuildingTypeId, int lineNumber)
        {
            // CSV parsing (basit split, tırnak içindeki virgülleri dikkate alır)
            var values = ParseCsvLineValues(line);
            
            // CSV'de 17 kolon var: Ilce,M2,Banyo,Kat,Bina_Yasi,Isinma,Balkon,Asansor,Garaj,Yapı Durumu,Kullanım Durumu,Tapu Durumu,Takas,Eşya Durumu,Fiyat,oda_sayisi,salon_sayisi
            if (values.Count < 17)
            {
                return null; // Yeterli kolon yok
            }

            try
            {
                // CSV kolonları: Ilce,M2,Banyo,Kat,Bina_Yasi,Isinma,Balkon,Asansor,Garaj,Yapı Durumu,Kullanım Durumu,Tapu Durumu,Takas,Eşya Durumu,Fiyat,oda_sayisi,salon_sayisi
                // UTF-8 geçersiz karakterleri temizle
                var ilce = CleanInvalidUtf8Chars(values[0]?.Trim() ?? "");
                var m2 = ParseDecimal(values[1]);
                var banyo = ParseInt(values[2]);
                var kat = CleanInvalidUtf8Chars(values[3]?.Trim() ?? "");
                var binaYasi = ParseBuildingAge(values[4]);
                var isinma = CleanInvalidUtf8Chars(values[5]?.Trim() ?? "");
                var balkon = ParseBoolean(values[6]);
                var asansor = ParseBoolean(values[7]);
                var garaj = ParseBoolean(values[8]);
                var yapiDurumu = CleanInvalidUtf8Chars(values[9]?.Trim() ?? "");
                var kullanimDurumu = CleanInvalidUtf8Chars(values[10]?.Trim() ?? "");
                var tapuDurumu = CleanInvalidUtf8Chars(values[11]?.Trim() ?? "");
                var takas = ParseBooleanFromTakas(values[12]);
                var esyaDurumu = CleanInvalidUtf8Chars(values[13]?.Trim() ?? "");
                var fiyat = ParseDecimal(values[14]);
                var odaSayisi = ParseInt(values[15]);
                var salonSayisi = ParseInt(values[16]);

                // District bul (Türkçe karakterleri normalize ederek)
                var normalizedIlce = NormalizeTurkishChars(ilce);
                var district = districts.FirstOrDefault(d => 
                    NormalizeTurkishChars(d.Name).Equals(normalizedIlce, StringComparison.OrdinalIgnoreCase));
                
                if (district == null)
                {
                    throw new Exception($"İlçe bulunamadı: {ilce}");
                }

                // Validation
                if (m2 <= 0) throw new Exception("Metrekare 0'dan büyük olmalıdır");
                if (fiyat <= 0) throw new Exception("Fiyat 0'dan büyük olmalıdır");
                if (odaSayisi <= 0) throw new Exception("Oda sayısı 0'dan büyük olmalıdır");
                if (salonSayisi < 0) throw new Exception("Salon sayısı negatif olamaz");
                if (binaYasi < 0) throw new Exception("Bina yaşı negatif olamaz");

                var listing = new Listing
                {
                    DistrictId = district.Id,
                    BuildingTypeId = defaultBuildingTypeId, // Varsayılan: Daire
                    Price = fiyat,
                    SquareMeters = m2,
                    RoomCount = odaSayisi,
                    SalonCount = salonSayisi,
                    BathroomCount = banyo > 0 ? banyo : null,
                    Floor = string.IsNullOrWhiteSpace(kat) ? null : kat,
                    BuildingAge = binaYasi,
                    HeatingType = string.IsNullOrWhiteSpace(isinma) ? null : isinma,
                    BuildingStatus = string.IsNullOrWhiteSpace(yapiDurumu) || yapiDurumu == "-" ? null : yapiDurumu,
                    UsageStatus = string.IsNullOrWhiteSpace(kullanimDurumu) ? null : kullanimDurumu,
                    DeedStatus = string.IsNullOrWhiteSpace(tapuDurumu) ? null : tapuDurumu,
                    FurnitureStatus = string.IsNullOrWhiteSpace(esyaDurumu) ? null : esyaDurumu,
                    HasBalcony = balkon,
                    HasElevator = asansor,
                    HasGarage = garaj,
                    IsExchangeable = takas,
                    ListingDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                return listing;
            }
            catch (Exception ex)
            {
                throw new Exception($"Satır {lineNumber} parse hatası: {ex.Message}");
            }
        }

        private List<string> ParseCsvLineValues(string line)
        {
            var values = new List<string>();
            var currentValue = new StringBuilder();
            var inQuotes = false;

            foreach (var c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }
            values.Add(currentValue.ToString()); // Son değer

            return values;
        }

        private decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;
            
            // Nokta veya virgül ile decimal parse
            value = value.Replace(",", ".");
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            
            return 0;
        }

        private int ParseInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;
            
            if (int.TryParse(value, out var result))
                return result;
            
            return 0;
        }

        private int ParseBuildingAge(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;
            
            // "7 Yaşında" -> 7
            var match = Regex.Match(value, @"(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var age))
                return age;
            
            return 0;
        }

        private bool? ParseBoolean(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            
            value = value.Trim();
            if (value.Equals("Var", StringComparison.OrdinalIgnoreCase) || 
                value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("True", StringComparison.OrdinalIgnoreCase))
                return true;
            
            if (value.Equals("Yok", StringComparison.OrdinalIgnoreCase) || 
                value.Equals("0", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("False", StringComparison.OrdinalIgnoreCase))
                return false;
            
            return null;
        }

        private bool? ParseBooleanFromTakas(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            
            value = value.Trim();
            if (value.Equals("Evet", StringComparison.OrdinalIgnoreCase) || 
                value.Equals("Var", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("1", StringComparison.OrdinalIgnoreCase))
                return true;
            
            return false; // "-", "Hayır" vs. -> false
        }

        /// <summary>
        /// String'i UTF-8 geçerli hale getirir (geçersiz byte sequence'leri temizler)
        /// </summary>
        private string CleanInvalidUtf8Chars(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text ?? "";

            try
            {
                // Latin1 (ISO-8859-1) encoding ile byte'a çevir (her byte geçerlidir)
                var latin1 = Encoding.GetEncoding("ISO-8859-1");
                var bytes = latin1.GetBytes(text);
                
                // UTF-8 decoder ile decode et, geçersiz byte'ları ? ile değiştir
                var decoder = Encoding.UTF8.GetDecoder();
                decoder.Fallback = new DecoderReplacementFallback("?");
                var charCount = decoder.GetCharCount(bytes, 0, bytes.Length);
                var chars = new char[charCount];
                decoder.GetChars(bytes, 0, bytes.Length, chars, 0);
                
                return new string(chars);
            }
            catch
            {
                // Hata olursa, geçersiz karakterleri kaldır
                var sanitized = new StringBuilder();
                foreach (var c in text)
                {
                    if (!char.IsSurrogate(c) && (c <= 0xFFFF))
                    {
                        sanitized.Append(c);
                    }
                }
                return sanitized.ToString();
            }
        }

        /// <summary>
        /// Türkçe karakterleri normalize eder (ş->s, ğ->g, ı->i, ü->u, ö->o, ç->c, Ş->S, Ğ->G, İ->I, Ü->U, Ö->O, Ç->C)
        /// CSV'deki ilçe isimleri ile veritabanındaki isimleri eşleştirmek için kullanılır
        /// </summary>
        private string NormalizeTurkishChars(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";
            
            var normalized = text
                .Replace("ş", "s").Replace("Ş", "S")
                .Replace("ğ", "g").Replace("Ğ", "G")
                .Replace("ı", "i").Replace("İ", "I")
                .Replace("ü", "u").Replace("Ü", "U")
                .Replace("ö", "o").Replace("Ö", "O")
                .Replace("ç", "c").Replace("Ç", "C");
            
            return normalized;
        }

        /// <summary>
        /// ONEHOT CSV dosyasından ilanları import eder (ONEHOT encoding kullanır, ilçeler kolonlardan çıkarılır)
        /// </summary>
        public async Task<(int SuccessCount, int ErrorCount, List<string> Errors)> ImportListingsFromOnehotCsvAsync(string csvFilePath)
        {
            var successCount = 0;
            var errorCount = 0;
            var errors = new List<string>();

            if (!File.Exists(csvFilePath))
            {
                errors.Add($"CSV dosyası bulunamadı: {csvFilePath}");
                return (0, 0, errors);
            }

            // ÖNCE: Districts tablosundaki encoding hatalarını düzelt (GetAllAsync çağrılmadan önce)
            // Bu, veritabanından okuma yapmadan önce bozuk kayıtları düzeltir
            // .NET string parametreleri ile direkt UPDATE
            try
            {
                await _context.Database.ExecuteSqlRawAsync(@"UPDATE ""Districts"" SET ""Name"" = {0} WHERE ""Id"" = 2", "Şehzadeler");
                await _context.Database.ExecuteSqlRawAsync(@"UPDATE ""Districts"" SET ""Name"" = {0} WHERE ""Id"" = 7", "Alaşehir");
                await _context.Database.ExecuteSqlRawAsync(@"UPDATE ""Districts"" SET ""Name"" = {0} WHERE ""Id"" = 8", "Saruhanlı");
            }
            catch
            {
                // Düzeltme başarısız olsa bile devam et (belki zaten düzeltilmiş)
            }

            // District ve BuildingType cache'leri
            // Encoding hatası olabileceği için try-catch ile koruyoruz
            List<District> districts;
            List<BuildingType> buildingTypes;
            
            try
            {
                districts = (await _districtRepository.GetAllAsync()).ToList();
            }
            catch (Exception ex)
            {
                errors.Add($"⚠️ İlçeler yüklenemedi (encoding hatası). Districts tablosunda geçersiz UTF-8 karakterler var. Lütfen Districts tablosundaki TÜM kayıtları kontrol edin ve Türkçe karakter içeren kayıtları düzeltin: {ex.Message}");
                return (0, 0, errors);
            }
            
            try
            {
                buildingTypes = (await _buildingTypeRepository.GetAllAsync()).ToList();
            }
            catch (Exception ex)
            {
                errors.Add($"⚠️ Bina tipleri yüklenemedi (encoding hatası). Veritabanındaki BuildingTypes tablosunda geçersiz UTF-8 karakterler var. Lütfen BuildingTypes tablosunu kontrol edin: {ex.Message}");
                return (0, 0, errors);
            }
            
            // Default BuildingType (Daire)
            var defaultBuildingType = buildingTypes.FirstOrDefault(bt => bt.Name.Equals("Daire", StringComparison.OrdinalIgnoreCase));
            if (defaultBuildingType == null)
            {
                errors.Add("Varsayılan bina tipi (Daire) bulunamadı!");
                return (0, 0, errors);
            }

            // İlçe kolon mapping (ONEHOT kolon adı -> DB'deki ilçe adı)
            var ilceMapping = new Dictionary<string, string>
            {
                { "Ilce_Akhisar", "Akhisar" },
                { "Ilce_Alasehir", "Alaşehir" },
                { "Ilce_Demirci", "Demirci" },
                { "Ilce_Gordes", "Gördes" },
                { "Ilce_Kirkagac", "Kırkağaç" },
                { "Ilce_Koprubasi", "Köprübaşı" },
                { "Ilce_Kula", "Kula" },
                { "Ilce_Salihli", "Salihli" },
                { "Ilce_Sarigol", "Sarıgöl" },
                { "Ilce_Saruhanli", "Saruhanlı" },
                { "Ilce_Sehzadeler", "Şehzadeler" },
                { "Ilce_Selendi", "Selendi" },
                { "Ilce_Soma", "Soma" },
                { "Ilce_Turgutlu", "Turgutlu" },
                { "Ilce_Yunusemre", "Yunusemre" },
                { "Ilce_Ahmetli", "Ahmetli" }
            };

            var listingsToAdd = new List<Listing>();
            var lineNumber = 0;

            // NOT: Duplicate kontrolü kaldırıldı - "Önce temizle" seçeneği kullanılıyor
            // Veritabanında encoding hatası olan kayıtlar olduğunda GetAllAsync() hata veriyor

            try
            {
                // UTF-8 encoding ile oku (ONEHOT CSV UTF-8)
                using var reader = new StreamReader(csvFilePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                
                // Header satırını oku ve kolon indexlerini bul
                var headerLine = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(headerLine))
                {
                    errors.Add("CSV dosyası boş veya header satırı bulunamadı!");
                    return (0, 0, errors);
                }

                var headers = headerLine.Split(',').Select(h => h.Trim()).ToArray();
                var headerDict = new Dictionary<string, int>();
                for (int i = 0; i < headers.Length; i++)
                {
                    headerDict[headers[i]] = i;
                }

                // İlçe kolon indexlerini bul
                var ilceColIndexes = new Dictionary<string, int>();
                foreach (var kvp in ilceMapping)
                {
                    if (headerDict.ContainsKey(kvp.Key))
                    {
                        ilceColIndexes[kvp.Key] = headerDict[kvp.Key];
                    }
                }

                // CSV satırlarını oku
                while (!reader.EndOfStream)
                {
                    lineNumber++;
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        var values = line.Split(',');
                        if (values.Length < headers.Length)
                        {
                            errorCount++;
                            errors.Add($"Satır {lineNumber}: Yeterli kolon bulunamadı");
                            continue;
                        }

                        // İlçeyi one-hot kolonlarından çıkar
                        string? ilceAdi = null;
                        foreach (var kvp in ilceColIndexes)
                        {
                            if (kvp.Value < values.Length && int.TryParse(values[kvp.Value].Trim(), out var val) && val == 1)
                            {
                                ilceAdi = ilceMapping[kvp.Key];
                                break;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(ilceAdi))
                        {
                            errorCount++;
                            errors.Add($"Satır {lineNumber}: İlçe bulunamadı (one-hot kolonlarında 1 değeri yok)");
                            continue;
                        }

                        // District bul
                        var normalizedIlce = NormalizeTurkishChars(ilceAdi);
                        var district = districts.FirstOrDefault(d => 
                            NormalizeTurkishChars(d.Name).Equals(normalizedIlce, StringComparison.OrdinalIgnoreCase));
                        
                        if (district == null)
                        {
                            errorCount++;
                            errors.Add($"Satır {lineNumber}: İlçe bulunamadı: {ilceAdi}");
                            continue;
                        }

                        // Değerleri parse et
                        var m2 = ParseDecimal(GetValue(values, headerDict, "M2"));
                        var banyo = ParseInt(GetValue(values, headerDict, "Banyo"));
                        var binaYasi = ParseInt(GetValue(values, headerDict, "Bina_Yasi"));
                        var balkon = ParseInt(GetValue(values, headerDict, "Balkon")) == 1;
                        var asansor = ParseInt(GetValue(values, headerDict, "Asansor")) == 1;
                        var garaj = ParseInt(GetValue(values, headerDict, "Garaj")) == 1;
                        var takas = ParseInt(GetValue(values, headerDict, "Takas")) == 1;
                        var esyaDurumu = ParseInt(GetValue(values, headerDict, "Eşya Durumu")) == 1 ? "Eşyalı" : "Eşyalı Değil";
                        var fiyat = ParseDecimal(GetValue(values, headerDict, "Fiyat"));
                        var odaSayisi = ParseInt(GetValue(values, headerDict, "oda_sayisi"));
                        var salonSayisi = ParseInt(GetValue(values, headerDict, "salon_sayisi"));
                        var kat = GetValue(values, headerDict, "Kat");
                        var isinma = ParseInt(GetValue(values, headerDict, "Isinma")).ToString();
                        var yapiDurumu = ParseInt(GetValue(values, headerDict, "yapi_durumu")) == 1 ? "İkinci El" : "Sıfır";
                        
                        // Kullanım durumu one-hot'dan çıkar
                        string kullanimDurumu = "Mülk Sahibi";
                        if (ParseInt(GetValue(values, headerDict, "Kullanim_Boş")) == 1) kullanimDurumu = "Boş";
                        else if (ParseInt(GetValue(values, headerDict, "Kullanim_Kiracılı")) == 1) kullanimDurumu = "Kiracılı";
                        else if (ParseInt(GetValue(values, headerDict, "Kullanim_Mülk Sahibi")) == 1) kullanimDurumu = "Mülk Sahibi";
                        
                        var tapuDurumu = ParseInt(GetValue(values, headerDict, "tapu_durumu")).ToString();

                        // Validation
                        if (m2 <= 0) throw new Exception("Metrekare 0'dan büyük olmalıdır");
                        if (fiyat <= 0) throw new Exception("Fiyat 0'dan büyük olmalıdır");
                        if (odaSayisi <= 0) throw new Exception("Oda sayısı 0'dan büyük olmalıdır");
                        if (salonSayisi < 0) throw new Exception("Salon sayısı negatif olamaz");
                        if (binaYasi < 0) throw new Exception("Bina yaşı negatif olamaz");

                        // Duplicate kontrolü kaldırıldı - direkt oluştur ve ekle
                        var listing = new Listing
                        {
                            DistrictId = district.Id,
                            BuildingTypeId = defaultBuildingType.Id,
                            Price = fiyat,
                            SquareMeters = m2,
                            RoomCount = odaSayisi,
                            SalonCount = salonSayisi,
                            BathroomCount = banyo > 0 ? banyo : null,
                            Floor = string.IsNullOrWhiteSpace(kat) || kat == "0" ? null : kat,
                            BuildingAge = binaYasi,
                            HeatingType = isinma == "0" ? null : isinma,
                            BuildingStatus = yapiDurumu == "-" ? null : yapiDurumu,
                            UsageStatus = kullanimDurumu,
                            DeedStatus = tapuDurumu == "0" ? null : tapuDurumu,
                            FurnitureStatus = esyaDurumu,
                            HasBalcony = balkon,
                            HasElevator = asansor,
                            HasGarage = garaj,
                            IsExchangeable = takas,
                            ListingDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };

                        listingsToAdd.Add(listing);
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.Add($"Satır {lineNumber}: {ex.Message}");
                    }
                }

                // Toplu ekleme
                if (listingsToAdd.Any())
                {
                    await _listingRepository.AddRangeAsync(listingsToAdd);
                    successCount = listingsToAdd.Count;
                }
                else if (lineNumber == 0)
                {
                    // Hiç satır okunmadı (sadece header var veya dosya boş)
                    errors.Add("⚠️ ONEHOT CSV dosyasında veri satırı bulunamadı! Dosya sadece header içeriyor olabilir.");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"ONEHOT CSV okuma hatası: {ex.Message}");
            }

            return (successCount, errorCount, errors);
        }

        private string GetValue(string[] values, Dictionary<string, int> headerDict, string columnName)
        {
            if (headerDict.ContainsKey(columnName) && headerDict[columnName] < values.Length)
            {
                return values[headerDict[columnName]]?.Trim() ?? "";
            }
            return "";
        }

        /// <summary>
        /// Tüm ilanları siler (hard delete - sadece admin için)
        /// SQL ile direkt silme yapılır (GetAllAsync encoding hatası verebilir)
        /// </summary>
        public async Task<int> DeleteAllListingsAsync()
        {
            // SQL ile direkt silme yap (GetAllAsync encoding hatası verebilir)
            try
            {
                // Önce sayıyı al (encoding hatası olmayabilir)
                int count = 0;
                try
                {
                    count = await _listingRepository.CountAsync(null);
                }
                catch
                {
                    // Count da hata verirse, direkt silme yap
                }

                // ListingFeatures'ı önce sil (CASCADE olabilir ama emin olmak için)
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"ListingFeatures\"");
                // Predictions'daki ListingId referanslarını NULL yap
                await _context.Database.ExecuteSqlRawAsync("UPDATE \"Predictions\" SET \"ListingId\" = NULL WHERE \"ListingId\" IS NOT NULL");
                // Tüm ilanları sil
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"Listings\"");

                return count;
            }
            catch (Exception)
            {
                // Hata durumunda 0 döndür
                return 0;
            }
        }
    }
}

