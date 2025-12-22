# HomeRadar - Web Uygulamasına Dönüştürme Rehberi

## 📋 İçindekiler
1. [Genel Bakış](#genel-bakış)
2. [Adım 1: NuGet Paketlerini Ekle](#adım-1-nuget-paketlerini-ekle)
3. [Adım 2: Proje Yapılandırması](#adım-2-proje-yapılandırması)
4. [Adım 3: Web.config Oluştur](#adım-3-webconfig-oluştur)
5. [Adım 4: Global.asax Ekle](#adım-4-globalasax-ekle)
6. [Adım 5: Klasör Yapısını Oluştur](#adım-5-klasör-yapısını-oluştur)
7. [Adım 6: İlk Controller'ı Oluştur](#adım-6-ilk-controllerı-oluştur)
8. [Üye 4 İçin Çalışma Rehberi](#üye-4-için-çalışma-rehberi)

---

## 📌 Genel Bakış

Mevcut **.NET Framework 4.8.1 Console Application** projesini **ASP.NET MVC 5** web uygulamasına dönüştüreceğiz.

**Önemli:**
- Mevcut Models/Data klasörleri korunacak
- App.config → Web.config'e dönüştürülecek
- Program.cs kaldırılacak (veya test için tutulabilir)
- Aynı framework (4.8.1) kullanılacak

---

## 📦 Adım 1: NuGet Paketlerini Ekle

**Visual Studio'da:**

1. **Solution Explorer'da** projeye sağ tık → **Manage NuGet Packages**
2. **Browse** sekmesinde şu paketleri ara ve yükle:

```
Microsoft.AspNet.Mvc (5.2.9 veya en son)
Microsoft.AspNet.WebApi (5.2.9 veya en son - opsiyonel)
Microsoft.AspNet.Identity.EntityFramework (2.2.3 - Identity için)
Microsoft.AspNet.Identity.Owin (2.2.3 - Identity için)
Microsoft.Owin.Host.SystemWeb (4.2.0 - OWIN için)
```

**Veya Package Manager Console'da:**

```powershell
Install-Package Microsoft.AspNet.Mvc -Version 5.2.9
Install-Package Microsoft.AspNet.WebApi -Version 5.2.9
Install-Package Microsoft.AspNet.Identity.EntityFramework -Version 2.2.3
Install-Package Microsoft.AspNet.Identity.Owin -Version 2.2.3
Install-Package Microsoft.Owin.Host.SystemWeb -Version 4.2.0
```

---

## ⚙️ Adım 2: Proje Yapılandırması

### 2.1. HomeRadar.csproj Dosyasını Düzenle

**Visual Studio'da:**
1. Projeye sağ tık → **Unload Project**
2. Tekrar sağ tık → **Edit HomeRadar.csproj**

**Değişiklikler:**

1. **OutputType'ı değiştir:**
   ```xml
   <!-- Eski -->
   <OutputType>Exe</OutputType>
   
   <!-- Yeni -->
   <OutputType>Library</OutputType>
   ```

2. **MVC referanslarını ekle** (ItemGroup içine):
   ```xml
   <Reference Include="System.Web" />
   <Reference Include="System.Web.Mvc" />
   <Reference Include="System.Web.WebPages" />
   <Reference Include="System.Web.Routing" />
   ```

3. **Projeyi yeniden yükle:**
   - Sağ tık → **Reload Project**

---

## 📄 Adım 3: Web.config Oluştur

**App.config'i Web.config'e dönüştür:**

1. **App.config** dosyasını kopyala
2. Adını **Web.config** olarak değiştir
3. **Web.config** dosyasını aç ve şu eklemeleri yap:

**Connection Strings bölümü (zaten var, kontrol et):**
```xml
<connectionStrings>
  <add name="HomeRadarConnection" 
       connectionString="Host=localhost;Port=5432;Database=HomeRadar_db;Username=homeradar_app_user;Password=HomeRadar2024!SecurePass" 
       providerName="Npgsql.EntityFrameworkCore.PostgreSQL" />
</connectionStrings>
```

**system.web bölümü ekle:**
```xml
<system.web>
  <compilation debug="true" targetFramework="4.8.1" />
  <httpRuntime targetFramework="4.8.1" />
  <pages>
    <namespaces>
      <add namespace="System.Web.Mvc" />
      <add namespace="System.Web.Mvc.Ajax" />
      <add namespace="System.Web.Mvc.Html" />
      <add namespace="System.Web.Routing" />
    </namespaces>
  </pages>
</system.web>
```

**system.webServer bölümü ekle:**
```xml
<system.webServer>
  <handlers>
    <remove name="BlockViewHandler"/>
    <add name="BlockViewHandler" type="System.Web.HttpNotFoundHandler" path="*" verb="*" preCondition="integratedMode" />
  </handlers>
</system.webServer>
```

---

## 🔧 Adım 4: Global.asax Ekle

**Yeni dosya oluştur:** `Global.asax`

**İçerik:**
```csharp
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HomeRadar
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            // Entity Framework için
            // Database.SetInitializer<EmlakContext>(null);
        }
    }
}
```

**Yeni dosya oluştur:** `App_Start/RouteConfig.cs`

**İçerik:**
```csharp
using System.Web.Mvc;
using System.Web.Routing;

namespace HomeRadar
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
```

---

## 📁 Adım 5: Klasör Yapısını Oluştur

**Aşağıdaki klasörleri oluştur:**

```
HomeRadar/
├── Controllers/          # Üye 4 buraya Controller'ları ekleyecek
├── Views/                # Üye 4 ve Üye 5 buraya View'leri ekleyecek
│   ├── Home/
│   ├── Shared/
│   │   └── _Layout.cshtml
│   └── _ViewStart.cshtml
├── App_Start/            # RouteConfig.cs burada
├── Content/              # CSS dosyaları (Üye 5 için)
├── Scripts/              # JavaScript dosyaları (Üye 5 için)
└── Models/               # Zaten var ✅
└── Data/                 # Zaten var ✅
```

**Manuel oluştur veya Visual Studio'da:**
- Solution Explorer'da projeye sağ tık → **Add** → **New Folder**

---

## 🎮 Adım 6: İlk Controller'ı Oluştur

**Yeni dosya oluştur:** `Controllers/HomeController.cs`

**İçerik:**
```csharp
using System.Web.Mvc;
using HomeRadar.Data;

namespace HomeRadar.Controllers
{
    public class HomeController : Controller
    {
        private readonly EmlakContext _context;

        public HomeController()
        {
            _context = new EmlakContext();
        }

        public ActionResult Index()
        {
            // ViewBag kullanımı (Üye 4 için örnek)
            ViewBag.Message = "HomeRadar - Emlak Değerleme Sistemi";
            ViewBag.ListingCount = _context.Listings.Count();
            
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Hakkında";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "İletişim";
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

**Yeni dosya oluştur:** `Views/Home/Index.cshtml`

**İçerik:**
```html
@{
    ViewBag.Title = "Ana Sayfa";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<h2>@ViewBag.Message</h2>
<p>Toplam İlan Sayısı: @ViewBag.ListingCount</p>
```

**Yeni dosya oluştur:** `Views/Shared/_Layout.cshtml`

**İçerik:**
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>@ViewBag.Title - HomeRadar</title>
</head>
<body>
    <div class="container">
        <div class="navbar">
            <div class="navbar-header">
                <a href="@Url.Action("Index", "Home")" class="navbar-brand">HomeRadar</a>
            </div>
        </div>
        
        <div class="body-content">
            @RenderBody()
        </div>
        
        <footer>
            <p>&copy; @DateTime.Now.Year - HomeRadar</p>
        </footer>
    </div>
</body>
</html>
```

**Yeni dosya oluştur:** `Views/_ViewStart.cshtml`

**İçerik:**
```html
@{
    Layout = "~/Views/Shared/_Layout.cshtml";
}
```

---

## 👨‍💻 Üye 4 İçin Çalışma Rehberi

### Görevlerin

1. **Controller'ları yaz** (Min 5 Controller, her birinde 3 Action)
2. **Identity sistemi kur** (Kullanıcı giriş/kayıt, Admin/User rolleri)
3. **ViewBag ve TempData kullan**
4. **Üye 3'ün API'sini çağır**

### 1. Controller Oluşturma

**Örnek: ListingController**

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
        [Authorize] // Sadece giriş yapmış kullanıcılar
        public ActionResult Create()
        {
            // ViewBag ile dropdown listeleri için veri gönder
            ViewBag.DistrictId = new SelectList(_context.Districts, "Id", "Name");
            ViewBag.BuildingTypeId = new SelectList(_context.BuildingTypes, "Id", "Name");
            
            return View();
        }

        // POST: Listing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create(Listing listing)
        {
            if (ModelState.IsValid)
            {
                _context.Listings.Add(listing);
                _context.SaveChanges();
                
                // TempData kullanımı
                TempData["SuccessMessage"] = "İlan başarıyla eklendi!";
                
                return RedirectToAction("Index");
            }

            ViewBag.DistrictId = new SelectList(_context.Districts, "Id", "Name", listing.DistrictId);
            ViewBag.BuildingTypeId = new SelectList(_context.BuildingTypes, "Id", "Name", listing.BuildingTypeId);
            
            return View(listing);
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

### 2. ViewBag Kullanımı

```csharp
// Controller'da
ViewBag.Message = "Hoş geldiniz";
ViewBag.UserName = User.Identity.Name;
ViewBag.ListingCount = _context.Listings.Count();

// View'de
<h1>@ViewBag.Message</h1>
<p>Kullanıcı: @ViewBag.UserName</p>
```

### 3. TempData Kullanımı

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

### 4. Identity Sistemi Kurulumu

**Adım 1: Identity Paketlerini Ekle**
```powershell
Install-Package Microsoft.AspNet.Identity.EntityFramework
Install-Package Microsoft.AspNet.Identity.Owin
Install-Package Microsoft.Owin.Host.SystemWeb
```

**Adım 2: ApplicationUser Sınıfı Oluştur**

`Models/ApplicationUser.cs`:
```csharp
using Microsoft.AspNet.Identity.EntityFramework;

namespace HomeRadar.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Ekstra özellikler eklenebilir
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

**Adım 4: Startup.cs Oluştur**

`App_Start/Startup.cs`:
```csharp
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using HomeRadar.Data;
using HomeRadar.Models;

[assembly: OwinStartup(typeof(HomeRadar.Startup))]

namespace HomeRadar
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
        }
    }
}
```

**Adım 5: AccountController Oluştur**

`Controllers/AccountController.cs`:
```csharp
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace HomeRadar.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            // Login işlemi
            // ...
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string email, string password)
        {
            // Kayıt işlemi
            // ...
            return RedirectToAction("Login");
        }
    }
}
```

### 5. Rol Kontrolü

```csharp
[Authorize(Roles = "Admin")]
public ActionResult AdminPanel()
{
    ViewBag.Message = "Admin Paneli";
    return View();
}

[Authorize(Roles = "User,Admin")]
public ActionResult UserDashboard()
{
    ViewBag.Message = "Kullanıcı Paneli";
    return View();
}
```

### 6. Üye 3'ün API'sini Çağırma

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
            else
            {
                ViewBag.Error = "Tahmin yapılamadı";
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

---

## ✅ Kontrol Listesi

- [ ] NuGet paketleri yüklendi
- [ ] Web.config oluşturuldu
- [ ] Global.asax eklendi
- [ ] RouteConfig.cs eklendi
- [ ] Controllers klasörü oluşturuldu
- [ ] Views klasörü oluşturuldu
- [ ] HomeController oluşturuldu
- [ ] İlk View oluşturuldu
- [ ] Proje çalışıyor (F5)

---

## 🚀 Test Etme

1. **Visual Studio'da F5'e bas**
2. **Tarayıcıda açılmalı:** `http://localhost:xxxxx`
3. **Home/Index sayfası görünmeli**

---

## 📝 Notlar

- **Program.cs:** İstersen kaldırabilirsin veya test için tutabilirsin
- **App.config:** Web.config'e dönüştürüldü, artık kullanılmıyor
- **Models/Data:** Aynen kullanılabilir, değişiklik yok
- **SQL Scriptleri:** Aynen kullanılabilir

---

## 🎯 Sonuç

Artık projen bir **ASP.NET MVC 5 Web Application**! 

**Üye 4 şimdi:**
- Controller'ları ekleyebilir
- View'leri oluşturabilir
- Identity sistemi kurabilir
- ViewBag/TempData kullanabilir

**Üye 5 şimdi:**
- Layout'u tasarlayabilir
- CSS ekleyebilir
- PartialView'leri yapabilir

**İyi çalışmalar! 🚀**

