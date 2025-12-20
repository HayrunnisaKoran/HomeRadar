# HomeRadar - Üye 4 İçin Başlangıç Rehberi

## 🎯 Proje Durumu

Proje **ASP.NET MVC 5** web uygulamasına dönüştürüldü! Artık web uygulaması olarak çalışıyor.

## ✅ Yapılanlar

- ✅ Proje web uygulamasına dönüştürüldü
- ✅ Web.config oluşturuldu
- ✅ Global.asax ve RouteConfig eklendi
- ✅ HomeController oluşturuldu (örnek)
- ✅ Temel View'ler oluşturuldu
- ✅ ViewBag kullanımı örneklendi

## 📋 Yapman Gerekenler

### 1. NuGet Paketlerini Yükle

**Visual Studio'da:**
1. Solution Explorer'da projeye sağ tık → **Manage NuGet Packages**
2. **Browse** sekmesinde şu paketleri ara ve yükle:

```
Microsoft.AspNet.Mvc (5.2.9)
Microsoft.AspNet.WebPages (3.2.9)
Microsoft.AspNet.Razor (3.2.9)
```

**Veya Package Manager Console'da:**
```powershell
Install-Package Microsoft.AspNet.Mvc -Version 5.2.9
Install-Package Microsoft.AspNet.WebPages -Version 3.2.9
Install-Package Microsoft.AspNet.Razor -Version 3.2.9
```

### 2. Projeyi Test Et

1. Visual Studio'da **F5** tuşuna bas
2. Tarayıcıda `http://localhost:xxxxx` açılmalı
3. Ana sayfa görünmeli

### 3. Controller'ları Oluştur

**Min 5 Controller, her birinde 3 Action:**

Örnek Controller'lar:
- `ListingController` - İlanlar için
- `UserController` - Kullanıcılar için
- `DistrictController` - İlçeler için
- `PredictionController` - Tahminler için
- `AccountController` - Giriş/Kayıt için

**Örnek Controller:**
```csharp
using System.Linq;
using System.Web.Mvc;
using HomeRadar.Data;
using HomeRadar.Models;

namespace HomeRadar.Controllers
{
    public class ListingController : Controller
    {
        private readonly EmlakContext _context;

        public ListingController()
        {
            _context = new EmlakContext();
        }

        // GET: Listing
        public ActionResult Index()
        {
            var listings = _context.Listings
                .Include(l => l.District)
                .Include(l => l.BuildingType)
                .Where(l => l.IsActive)
                .ToList();
            
            // ViewBag kullanımı
            ViewBag.Message = "Aktif İlanlar";
            ViewBag.TotalCount = listings.Count;
            
            return View(listings);
        }

        // GET: Listing/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            var listing = _context.Listings
                .Include(l => l.District)
                .Include(l => l.BuildingType)
                .FirstOrDefault(l => l.Id == id);

            if (listing == null)
            {
                return HttpNotFound();
            }

            return View(listing);
        }

        // GET: Listing/Create
        public ActionResult Create()
        {
            // ViewBag ile dropdown listeleri için veri gönder
            ViewBag.DistrictId = new SelectList(_context.Districts, "Id", "Name");
            ViewBag.BuildingTypeId = new SelectList(_context.BuildingTypes, "Id", "Name");
            
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
```

### 4. ViewBag ve TempData Kullan

**ViewBag Örneği:**
```csharp
// Controller'da
ViewBag.Message = "Hoş geldiniz";
ViewBag.UserName = "Ahmet";
ViewBag.ListingCount = _context.Listings.Count();

// View'de
<h1>@ViewBag.Message</h1>
<p>Kullanıcı: @ViewBag.UserName</p>
```

**TempData Örneği:**
```csharp
// Controller'da (Create action'ında)
TempData["SuccessMessage"] = "İşlem başarılı!";
return RedirectToAction("Index");

// View'de (Index.cshtml)
@if (TempData["SuccessMessage"] != null)
{
    <div class="alert alert-success">
        @TempData["SuccessMessage"]
    </div>
}
```

### 5. Identity Sistemi Kur

**Adım 1: Paketleri Yükle**
```powershell
Install-Package Microsoft.AspNet.Identity.EntityFramework
Install-Package Microsoft.AspNet.Identity.Owin
Install-Package Microsoft.Owin.Host.SystemWeb
```

**Adım 2: ApplicationUser Oluştur**
`Models/ApplicationUser.cs`:
```csharp
using Microsoft.AspNet.Identity.EntityFramework;

namespace HomeRadar.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Ekstra özellikler
    }
}
```

**Adım 3: ApplicationDbContext Oluştur**
`Data/ApplicationDbContext.cs`:
```csharp
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace HomeRadar.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("HomeRadarConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
```

**Adım 4: AccountController Oluştur**
- Login action
- Register action
- Logout action
- Admin/User rol kontrolü

### 6. Üye 3'ün API'sini Çağır

```csharp
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class PredictionController : Controller
{
    private readonly HttpClient _httpClient;

    public PredictionController()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost:3000/api/"); // Üye 3'ün API'si
    }

    public async Task<ActionResult> Predict(int districtId, int roomCount, decimal squareMeters)
    {
        try
        {
            var response = await _httpClient.GetAsync($"predict?districtId={districtId}&roomCount={roomCount}&squareMeters={squareMeters}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var prediction = JsonConvert.DeserializeObject<PredictionResult>(content);
                
                ViewBag.PredictedPrice = prediction.Price;
                ViewBag.Confidence = prediction.Confidence;
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
        }

        return View();
    }
}
```

## 📁 Proje Yapısı

```
HomeRadar/
├── Controllers/          # Controller'ları buraya ekle
│   └── HomeController.cs (örnek)
├── Views/                # View'leri buraya ekle
│   ├── Home/
│   ├── Shared/
│   │   └── _Layout.cshtml
│   └── _ViewStart.cshtml
├── Models/               # Zaten var ✅
├── Data/                 # Zaten var ✅
├── App_Start/
│   └── RouteConfig.cs    # Route yapılandırması
├── Global.asax           # Uygulama başlangıcı
└── Web.config            # Web yapılandırması
```

## 🚀 Hızlı Başlangıç

1. **NuGet paketlerini yükle** (yukarıda)
2. **Projeyi çalıştır** (F5)
3. **Controller ekle** (örnek: ListingController)
4. **View oluştur** (Controller'a sağ tık → Add View)
5. **ViewBag/TempData kullan**

## 📝 Önemli Notlar

- **ViewBag:** Controller'dan View'e veri göndermek için
- **TempData:** Sayfalar arası veri taşımak için (Redirect sonrası)
- **Models/Data:** Zaten hazır, kullanabilirsin
- **Web.config:** Connection string burada, güncelle

## 🎯 Görevlerin

- [ ] Min 5 Controller oluştur
- [ ] Her Controller'da min 3 Action
- [ ] Identity sistemi kur (Login/Register)
- [ ] Admin/User rol yönetimi
- [ ] ViewBag kullan
- [ ] TempData kullan
- [ ] Üye 3'ün API'sini çağır

## 📚 Detaylı Rehber

Daha detaylı bilgi için: `DOCUMENTATION/WEB_APPLICATION_SETUP.md`

**İyi çalışmalar! 🚀**

