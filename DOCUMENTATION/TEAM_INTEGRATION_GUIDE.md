# SmartValue - Grup Üyeleri Entegrasyon Rehberi

## 👥 Grup Üyeleri ve Görevleri

### Üye 1: Veritabanı Yöneticisi & Takım Lideri (SEN)
**Odak**: Veritabanı Dersi İsterleri

**Görevler:**
- ✅ PostgreSQL kurulumu ve 6 tablonun tasarlanması (ER Diyagramı)
- ✅ 2 Stored Procedure, 2 Kullanıcı Tanımlı Fonksiyon ve 5 View'in yazılması
- ✅ Veritabanı kullanıcı yetkilendirmeleri ve güvenlik kısıtları (Constraints)
- ✅ Diğer ekiplere veritabanı bağlantı bilgilerini sağlamak

**Teslim Ettiğin Dosyalar:**
- `App.config` - Connection string bilgileri
- `SQL/` klasörü - Tüm SQL scriptleri
- `Models/` klasörü - Entity Framework modelleri
- `Data/EmlakContext.cs` - DbContext sınıfı
- `DOCUMENTATION/` klasörü - Dokümantasyonlar

---

### Üye 2: Veri Bilimcisi (Machine Learning Specialist)
**Odak**: Makine Öğrenmesi Dersi İsterleri

**Görevler:**
- Python (BeautifulSoup) ile emlak sitesinden 2000+ verinin çekilmesi (Scraping)
- Verinin temizlenmesi (EDA) ve eksik verilerin doldurulması
- Verinin PostgreSQL'e aktarılması (ETL)
- Lineer Regresyon ve Karar Ağacı modellerinin eğitilmesi
- Modeli bir Flask API (mikroservis) olarak yayınlamak

**Senden İhtiyaç Duyduğu:**
1. **Connection String**: `App.config` dosyasındaki connection string
2. **Tablo Yapısı**: `Models/Listing.cs` dosyasındaki kolon yapısı
3. **Veri Formatı**: Hangi kolonların zorunlu olduğu bilgisi

**Ona Vereceğin Bilgiler:**

#### Connection String (Python için)
```python
# Python'da psycopg2 kullanarak bağlanma
import psycopg2

connection_string = {
    'host': 'localhost',
    'port': 5432,
    'database': 'HomeRadar_db',
    'user': 'homeradar_app_user',
    'password': 'HomeRadar2024!SecurePass'
}

conn = psycopg2.connect(**connection_string)
```

#### Veri Ekleme Örneği (Python)
```python
import psycopg2
from datetime import datetime

# Bağlantı
conn = psycopg2.connect(
    host='localhost',
    port=5432,
    database='HomeRadar_db',
    user='homeradar_app_user',
    password='HomeRadar2024!SecurePass'
)
cur = conn.cursor()

# İlçe ID'sini al (önce Districts tablosuna eklemesi gerekir)
cur.execute("SELECT \"Id\" FROM \"Districts\" WHERE \"Name\" = %s", ('Yunusemre',))
district_id = cur.fetchone()[0]

# Bina tipi ID'sini al
cur.execute("SELECT \"Id\" FROM \"BuildingTypes\" WHERE \"Name\" = %s", ('Daire',))
building_type_id = cur.fetchone()[0]

# İlan ekle
cur.execute("""
    INSERT INTO "Listings" (
        "DistrictId", "BuildingTypeId", "Price", "SquareMeters",
        "RoomCount", "SalonCount", "BuildingAge", "ListingDate", 
        "CreatedAt", "IsActive"
    ) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
""", (
    district_id,
    building_type_id,
    450000,  # Price
    120,     # SquareMeters
    3,       # RoomCount
    1,       # SalonCount
    5,       # BuildingAge
    datetime.now(),  # ListingDate
    datetime.now(),  # CreatedAt
    True     # IsActive
))

conn.commit()
cur.close()
conn.close()
```

#### Zorunlu Kolonlar (Listings Tablosu)
- ✅ **DistrictId**: İlçe ID'si (önce Districts tablosuna eklenmeli)
- ✅ **BuildingTypeId**: Bina tipi ID'si (önce BuildingTypes tablosuna eklenmeli)
- ✅ **Price**: Fiyat (decimal, > 0)
- ✅ **SquareMeters**: Metrekare (decimal, > 0)
- ✅ **RoomCount**: Oda sayısı (int, > 0)
- ✅ **SalonCount**: Salon sayısı (int, > 0)
- ✅ **BuildingAge**: Bina yaşı (int, >= 0)
- ✅ **ListingDate**: İlan tarihi (datetime)
- ✅ **CreatedAt**: Oluşturulma tarihi (datetime)
- ✅ **IsActive**: Aktif mi (boolean, default: true)

#### Opsiyonel Kolonlar
- BathroomCount, Floor, Aidat, HeatingType, Direction, BuildingStatus, UsageStatus, DeedStatus, FurnitureStatus, HasBalcony, HasElevator, HasGarage, IsInComplex, HasSecurity, IsCreditSuitable, IsExchangeable, Neighborhood

**Ondan Alacağın:**
- Flask API endpoint URL'i (örn: `http://localhost:5000/predict`)
- Model input formatı (hangi parametreler gerekli)
- Model output formatı (tahmin sonucu formatı)

**Entegrasyon Noktası:**
- Üye 2, verileri `Listings` tablosuna ekleyecek
- Üye 2, tahmin sonuçlarını `Predictions` tablosuna ekleyecek (opsiyonel)

---

### Üye 3: Servis Geliştirici (Backend - API & SOA)
**Odak**: Servis Odaklı Mimari (SOA) İsterleri

**Görevler:**
- Node.js veya Vue.js ile ana API'nin yazılması
- gRPC ve SOAP protokolleri ile iletişim kurgulanması
- Dışarıdan bir Hazır API (Google Maps veya Hava Durumu) entegrasyonu
- 6 Katmanlı SOA mimarisinin kurgulanması

**Senden İhtiyaç Duyduğu:**
1. **Connection String**: PostgreSQL bağlantı bilgileri
2. **API Endpoint'leri**: Hangi verileri nasıl çekeceği
3. **View'ler**: Hazır view'lerden veri çekme
4. **Stored Procedures**: Hazır procedure'leri kullanma

**Ona Vereceğin Bilgiler:**

#### Connection String (Node.js için)
```javascript
// Node.js'de pg kütüphanesi ile bağlanma
const { Pool } = require('pg');

const pool = new Pool({
  host: 'localhost',
  port: 5432,
  database: 'HomeRadar_db',
  user: 'homeradar_app_user',
  password: 'HomeRadar2024!SecurePass',
  max: 20, // connection pool size
  idleTimeoutMillis: 30000,
  connectionTimeoutMillis: 2000,
});
```

#### API Endpoint Örnekleri

**1. İlçe Listesi Getir:**
```javascript
app.get('/api/districts', async (req, res) => {
  const result = await pool.query('SELECT * FROM "Districts" ORDER BY "Name"');
  res.json(result.rows);
});
```

**2. View Kullanarak İlçe Ortalama Fiyatları:**
```javascript
app.get('/api/districts/avg-prices', async (req, res) => {
  const result = await pool.query('SELECT * FROM vw_district_avg_prices');
  res.json(result.rows);
});
```

**3. Stored Procedure Kullanarak İlanları Getir:**
```javascript
app.get('/api/listings', async (req, res) => {
  const { districtId, roomCount, minPrice, maxPrice } = req.query;
  const result = await pool.query(
    'SELECT * FROM sp_get_listings_by_criteria($1, $2, $3, $4, NULL, NULL)',
    [districtId || null, roomCount || null, minPrice || null, maxPrice || null]
  );
  res.json(result.rows);
});
```

**4. Tahmin Ekle (Stored Procedure ile):**
```javascript
app.post('/api/predictions', async (req, res) => {
  const { userId, districtId, roomCount, squareMeters, buildingAge, 
          buildingTypeId, predictedPriceMin, predictedPriceMax, modelName, confidenceScore } = req.body;
  
  const result = await pool.query(
    'SELECT sp_insert_prediction($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)',
    [userId, districtId, roomCount, squareMeters, buildingAge, 
     buildingTypeId, predictedPriceMin, predictedPriceMax, modelName, confidenceScore]
  );
  
  res.json({ predictionId: result.rows[0].sp_insert_prediction });
});
```

**5. Kullanıcı Tahmin Geçmişi (View ile):**
```javascript
app.get('/api/users/:userId/predictions', async (req, res) => {
  const { userId } = req.params;
  const result = await pool.query(
    'SELECT * FROM vw_user_predictions WHERE "UserId" = $1 ORDER BY "PredictionDate" DESC',
    [userId]
  );
  res.json(result.rows);
});
```

#### Hazır View'ler (5 adet)
1. **vw_district_avg_prices** - İlçe bazında ortalama fiyatlar
2. **vw_room_count_statistics** - Oda sayısına göre istatistikler
3. **vw_user_predictions** - Kullanıcı tahmin geçmişi
4. **vw_active_listings_detail** - Aktif ilanlar detaylı bilgi
5. **vw_building_age_price_analysis** - Bina yaşına göre fiyat analizi

#### Hazır Stored Procedures (2 adet)
1. **sp_get_listings_by_criteria** - Kriterlere göre ilan getir
   - Parametreler: districtId, roomCount, minPrice, maxPrice, minSquareMeters, maxSquareMeters
2. **sp_insert_prediction** - Tahmin kaydı ekle
   - Parametreler: userId, districtId, roomCount, squareMeters, buildingAge, buildingTypeId, predictedPriceMin, predictedPriceMax, modelName, confidenceScore

#### Hazır Functions (2 adet)
1. **fn_calculate_price_per_square_meter(price, squareMeters)** - Metrekare başına fiyat
2. **fn_estimate_price_by_district(districtId, roomCount, squareMeters, buildingAge)** - İlçe için ortalama fiyat tahmini

**Ondan Alacağın:**
- Node.js API base URL'i (örn: `http://localhost:3000/api`)
- API endpoint dokümantasyonu (Swagger/OpenAPI)
- gRPC ve SOAP endpoint'leri

**Entegrasyon Noktası:**
- Üye 3, veritabanından veri çekecek (SELECT işlemleri)
- Üye 3, tahmin sonuçlarını veritabanına ekleyecek (INSERT işlemleri)
- Üye 3, ML servisinden (Üye 2) tahmin alıp veritabanına kaydedecek

---

### Üye 4: Full-Stack Web Geliştirici (Logic & Controller)
**Odak**: İleri Web Programlama - Backend Logic

**Görevler:**
- ASP.NET Core MVC projesinin kurulması
- Controller ve Action'ların yazılması (Min 5 Controller, 3 Action)
- Kullanıcı Giriş/Kayıt sistemi (Identity) ve 2 farklı Rol (Admin/User) yönetimi
- ViewBag, TempData ile sayfalar arası veri taşıma
- Üye 3'ün yazdığı API'den verileri çekip ön yüze göndermek

**Senden İhtiyaç Duyduğu:**
1. **Entity Framework DbContext**: `EmlakContext` sınıfı
2. **User Modeli**: Kullanıcı yapısı (Admin/User rolleri)
3. **Connection String**: Veritabanı bağlantı bilgileri

**Ona Vereceğin Bilgiler:**

#### Entity Framework Kullanımı
```csharp
// Startup.cs veya Program.cs'de
using Microsoft.EntityFrameworkCore;
using HomeRadar.Data;

// DbContext'i DI container'a ekle
services.AddDbContext<EmlakContext>(options =>
    options.UseNpgsql(Configuration.GetConnectionString("HomeRadarConnection")));
```

#### Controller Örneği
```csharp
using Microsoft.AspNetCore.Mvc;
using HomeRadar.Data;
using HomeRadar.Models;
using Microsoft.EntityFrameworkCore;

public class ListingController : Controller
{
    private readonly EmlakContext _context;

    public ListingController(EmlakContext context)
    {
        _context = context;
    }

    // GET: Listing
    public async Task<IActionResult> Index()
    {
        var listings = await _context.Listings
            .Include(l => l.District)
            .Include(l => l.BuildingType)
            .Where(l => l.IsActive)
            .ToListAsync();
        
        return View(listings);
    }

    // GET: Listing/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var listing = await _context.Listings
            .Include(l => l.District)
            .Include(l => l.BuildingType)
            .Include(l => l.ListingFeatures)
                .ThenInclude(lf => lf.Feature)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (listing == null)
        {
            return NotFound();
        }

        return View(listing);
    }
}
```

#### View Kullanımı (Raw SQL)
```csharp
// View'den veri çekme
var districtStats = await _context.Database
    .SqlQuery<DistrictStatsViewModel>(
        "SELECT * FROM vw_district_avg_prices")
    .ToListAsync();
```

#### Stored Procedure Kullanımı
```csharp
// Stored procedure çağırma
var listings = await _context.Database
    .SqlQuery<Listing>(
        "SELECT * FROM sp_get_listings_by_criteria({0}, {1}, {2}, {3}, NULL, NULL)",
        districtId, roomCount, minPrice, maxPrice)
    .ToListAsync();
```

#### User Modeli ve Roller
```csharp
// User modeli zaten hazır (Models/User.cs)
// Role: "Admin" veya "User"

// Controller'da rol kontrolü
[Authorize(Roles = "Admin")]
public IActionResult AdminPanel()
{
    return View();
}

[Authorize(Roles = "User,Admin")]
public IActionResult UserDashboard()
{
    return View();
}
```

**Ondan Alacağın:**
- ASP.NET Core MVC projesi
- Controller'lar ve Action'lar
- View'ler (HTML/Razor)

**Entegrasyon Noktası:**
- Üye 4, Entity Framework ile veritabanına bağlanacak
- Üye 4, CRUD işlemlerini yapacak
- Üye 4, Üye 3'ün API'sini çağıracak (ML tahminleri için)

---

### Üye 5: Frontend Tasarımcı & Web Geliştirici (UI/UX)
**Odak**: İleri Web Programlama - Frontend Design

**Görevler:**
- Özgün bir Layout tasarımı ve bunun 3 view'de uygulanması
- PartialView ve ViewComponent yapılarını kullanarak sayfa parçalarının oluşturulması
- Esnek (Responsive) tasarımların yapılması
- Form tasarımları ve kullanıcı deneyimi

**Senden İhtiyaç Duyduğu:**
1. **Veri Yapısı**: Hangi verilerin gösterileceği
2. **Form Alanları**: Hangi form alanlarının olacağı

**Ona Vereceğin Bilgiler:**

#### Form Alanları (Tahmin Formu)
```html
<!-- Ne Kadar Eder? Sayfası için form -->
<form asp-action="Calculate" method="post">
    <div>
        <label>İlçe:</label>
        <select asp-for="DistrictId" asp-items="ViewBag.Districts"></select>
    </div>
    <div>
        <label>Oda Sayısı:</label>
        <input asp-for="RoomCount" type="number" min="1" max="10" />
    </div>
    <div>
        <label>Salon Sayısı:</label>
        <input asp-for="SalonCount" type="number" min="0" max="5" />
    </div>
    <div>
        <label>Metrekare:</label>
        <input asp-for="SquareMeters" type="number" min="1" step="0.01" />
    </div>
    <div>
        <label>Bina Yaşı:</label>
        <input asp-for="BuildingAge" type="number" min="0" />
    </div>
    <div>
        <label>Bina Tipi:</label>
        <select asp-for="BuildingTypeId" asp-items="ViewBag.BuildingTypes"></select>
    </div>
    <button type="submit">Hesapla</button>
</form>
```

#### ViewModel Örneği
```csharp
public class PredictionViewModel
{
    public int DistrictId { get; set; }
    public int RoomCount { get; set; }
    public int SalonCount { get; set; }
    public decimal SquareMeters { get; set; }
    public int BuildingAge { get; set; }
    public int? BuildingTypeId { get; set; }
    
    // Sonuç
    public decimal PredictedPriceMin { get; set; }
    public decimal PredictedPriceMax { get; set; }
    public decimal PredictedPriceAvg { get; set; }
}
```

**Ondan Alacağın:**
- Layout dosyası
- View dosyaları (Razor)
- CSS dosyaları
- JavaScript dosyaları

**Entegrasyon Noktası:**
- Üye 5, Üye 4'ün hazırladığı Controller'ları kullanacak
- Üye 5, ViewBag ve TempData ile veri alışverişi yapacak

---

## 🔄 Veri Akışı (Data Flow)

```
┌─────────────┐
│  Üye 2     │
│  (ML)      │
│            │
│  Scraping  │
│  → ETL     │
│  → Model   │
│  → Flask   │
└─────┬──────┘
      │
      │ Veri ekleme (Listings)
      │ Tahmin sonuçları (Predictions)
      ▼
┌─────────────┐
│ PostgreSQL │
│  Database  │
│            │
│  Views     │
│  SPs       │
│  Functions │
└─────┬──────┘
      │
      │ Veri çekme
      │ Veri ekleme
      ▼
┌─────────────┐
│  Üye 3     │
│  (SOA)     │
│            │
│  Node.js    │
│  API        │
│  gRPC/SOAP  │
└─────┬──────┘
      │
      │ API çağrıları
      │ Veri çekme
      ▼
┌─────────────┐
│  Üye 4     │
│  (Web BE)  │
│            │
│  Controllers│
│  Actions    │
│  Identity   │
└─────┬──────┘
      │
      │ ViewBag/TempData
      │ Model binding
      ▼
┌─────────────┐
│  Üye 5     │
│  (Web FE)  │
│            │
│  Views     │
│  Layout    │
│  CSS/JS    │
└────────────┘
```

## 📋 Entegrasyon Kontrol Listesi

### Üye 1 (SEN) - Veritabanı
- [x] PostgreSQL kurulumu tamamlandı
- [x] Tablolar oluşturuldu (7 tablo)
- [x] View'ler oluşturuldu (5 view)
- [x] Stored procedure'ler oluşturuldu (2 procedure)
- [x] User defined function'lar oluşturuldu (2 function)
- [x] Constraints eklendi (5+ constraint)
- [x] Indexes eklendi
- [x] Kullanıcı yetkilendirmeleri yapıldı
- [x] Connection string dokümante edildi
- [ ] Üye 2'ye connection string verildi
- [ ] Üye 3'e API endpoint örnekleri verildi
- [ ] Üye 4'e Entity Framework kullanım örnekleri verildi
- [ ] Üye 5'e form alanları dokümante edildi

### Üye 2 (ML) - Veri Bilimcisi
- [ ] Connection string alındı
- [ ] Veri çekme botu yazıldı
- [ ] Veri temizleme (EDA) yapıldı
- [ ] Veriler PostgreSQL'e aktarıldı (2000+ kayıt)
- [ ] ML modelleri eğitildi (Linear Regression, Decision Tree)
- [ ] En iyi model seçildi
- [ ] Flask API oluşturuldu
- [ ] Flask API endpoint URL'i paylaşıldı

### Üye 3 (SOA) - Servis Geliştirici
- [ ] Connection string alındı
- [ ] Node.js API oluşturuldu
- [ ] View'lerden veri çekme endpoint'leri yazıldı
- [ ] Stored procedure'lerden veri çekme endpoint'leri yazıldı
- [ ] gRPC iletişimi kuruldu
- [ ] SOAP iletişimi kuruldu
- [ ] Dış API entegrasyonu yapıldı (Google Maps/Hava Durumu)
- [ ] 6 Katmanlı SOA mimarisi kuruldu
- [ ] API dokümantasyonu hazırlandı (Swagger/OpenAPI)
- [ ] API base URL'i paylaşıldı

### Üye 4 (Web BE) - Full-Stack Backend
- [ ] Entity Framework DbContext kullanıldı
- [ ] 5 Controller oluşturuldu
- [ ] Her controller'da en az 3 Action oluşturuldu
- [ ] Identity sistemi kuruldu
- [ ] Admin/User rolleri tanımlandı
- [ ] ViewBag/TempData kullanıldı
- [ ] Üye 3'ün API'si entegre edildi
- [ ] CRUD işlemleri yapıldı

### Üye 5 (Web FE) - Frontend Tasarımcı
- [ ] Özgün Layout tasarımı yapıldı
- [ ] Layout 3 view'de uygulandı
- [ ] PartialView'ler oluşturuldu (Header, Footer, Sidebar)
- [ ] ViewComponent'ler oluşturuldu
- [ ] Responsive tasarım yapıldı
- [ ] Form tasarımları yapıldı
- [ ] Üye 4'ün Controller'ları ile entegre edildi

## 🚨 Kritik Entegrasyon Noktaları

### 1. Veri Formatı Uyumu
- Üye 2'nin çektiği veriler, `Listing` modelindeki kolonlarla uyumlu olmalı
- Decimal değerler için precision uyumu (Price: 18,2; SquareMeters: 10,2)

### 2. Foreign Key Uyumu
- Üye 2, veri eklerken önce `Districts` ve `BuildingTypes` tablolarına veri eklemeli
- Foreign key ID'leri doğru olmalı

### 3. API Endpoint Uyumu
- Üye 3'ün API endpoint'leri, Üye 4'ün ihtiyaçlarına uygun olmalı
- Request/Response formatları uyumlu olmalı

### 4. Model Binding Uyumu
- Üye 4'ün ViewModel'leri, Üye 5'in form alanlarıyla uyumlu olmalı
- ViewBag/TempData kullanımı tutarlı olmalı

## 📞 İletişim

Sorularınız için:
- **Takım Lideri (Üye 1)**: Veritabanı konuları
- **GitHub Issues**: Teknik sorunlar
- **Teams**: Genel iletişim

## 📝 Notlar

- Tüm connection string'ler `.gitignore` dosyasına eklenmeli
- Şifreler asla kod içinde saklanmamalı
- Production ortamında environment variables kullanılmalı
- API endpoint'leri dokümante edilmeli (Swagger/OpenAPI)

