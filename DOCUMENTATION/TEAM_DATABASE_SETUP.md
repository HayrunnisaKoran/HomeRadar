# HomeRadar - Takım Üyeleri İçin Veritabanı Kurulum Rehberi

## 📋 İçindekiler
1. [Ortak Kurulum (Herkes İçin)](#ortak-kurulum-herkes-için)
2. [Üye 2: ML (Machine Learning) - Veri Bilimcisi](#üye-2-ml-machine-learning---veri-bilimcisi)
3. [Üye 3: SOA/API - Servis Geliştirici](#üye-3-soaapi---servis-geliştirici)
4. [Üye 4: Web Backend - Full-Stack Backend](#üye-4-web-backend---full-stack-backend)
5. [Üye 5: Frontend - UI/UX Tasarımcı](#üye-5-frontend---uiux-tasarımcı)
6. [Sorun Giderme](#sorun-giderme)

---

## 📌 Proje Yapısı Hakkında

**ÖNEMLİ:** Bu dokümantasyon, **HomeRadar** projesi için hazırlanmıştır.

- **Proje:** .NET Framework 4.8.1 **ASP.NET MVC 5 Web Application** ✅
- **Platform:** Windows (Tüm takım üyeleri Windows kullanıyor)
- **Veritabanı:** PostgreSQL (`HomeRadar_db`)

### 🎯 Herkesin Yapması Gerekenler (Özet)

**1. Ortak Kurulum (HERKES):**
   - ✅ PostgreSQL kurulumu
   - ✅ `HomeRadar_db` veritabanını oluşturma
   - ✅ SQL scriptlerini çalıştırma (01-05 arası)

**2. Her Üyenin Kendi Görevi:**
   - **Üye 2 (ML):** Verileri kendi veritabanına ekler (2000+ ilan)
   - **Üye 3 (SOA/API):** Node.js ile API yazar, veritabanından veri çeker
   - **Üye 4 (Web Backend):** ASP.NET MVC Controller'ları yazar, veritabanına bağlanır
   - **Üye 5 (Frontend):** View'leri tasarlar, web arayüzünü oluşturur

### ⚠️ ÖNEMLİ: Veri Paylaşımı

**Herkes kendi bilgisayarında çalışıyorsa:**
- Herkes kendi PostgreSQL'ini kurar
- Herkes kendi `HomeRadar_db` veritabanını oluşturur
- ML kişisi verileri **sadece kendi veritabanına** ekler
- Diğerleri kendi veritabanlarında çalışır (ML verileri olmaz)

**Ortak sunucu kullanıyorsanız:**
- Herkes aynı PostgreSQL sunucusuna bağlanır
- Herkes aynı `HomeRadar_db` veritabanını kullanır
- ML kişisi verileri eklediğinde, **herkes görebilir** ✅

**ML Verilerini Paylaşmak İçin:**
- ML kişisi verileri export edebilir (SQL dump veya CSV)
- Diğerleri bu verileri kendi veritabanlarına import edebilir
- Veya ortak bir sunucu kullanabilirsiniz

---

## 🔧 Ortak Kurulum (Herkes İçin)

### Adım 1: PostgreSQL Kurulumu (Windows)

1. **PostgreSQL İndirme:**
   - https://www.postgresql.org/download/windows/ adresine git
   - "Download the installer" butonuna tıkla
   - En son sürümü indir (PostgreSQL 15 veya 16)

2. **Kurulum Adımları:**
   - İndirilen `.exe` dosyasını çalıştır
   - "Next" butonlarına tıklayarak ilerle
   - **ÖNEMLİ:** Kurulum sırasında:
     - **Port:** `5432` (varsayılan - değiştirme)
     - **Superuser Password:** Güvenli bir şifre belirle (örn: `postgres123`)
     - **Locale:** Turkish, Turkey (opsiyonel)
   - Kurulum tamamlanınca "Finish" butonuna tıkla

3. **PostgreSQL Servisini Kontrol Et:**
   - `Win + R` tuşlarına bas
   - `services.msc` yaz ve Enter'a bas
   - "postgresql-x64-XX" servisini bul
   - Durumunun "Çalışıyor" olduğundan emin ol
   - Eğer durdurulmuşsa, sağ tık → "Başlat"

### Adım 2: Veritabanını Oluştur

**PowerShell'de:**
```powershell
# PostgreSQL'e bağlan
psql -U postgres
```

**psql içinde:**
```sql
-- UTF-8 encoding ile veritabanı oluştur
CREATE DATABASE "HomeRadar_db" WITH ENCODING = 'UTF8';

-- Veritabanının oluşturulduğunu kontrol et
\l

-- Çık
\q
```

### Adım 3: SQL Scriptlerini Çalıştır

**PowerShell'de (proje klasöründen):**
```powershell
# Proje klasörüne git
cd C:\PROJELER\HomeRadar
# (veya projenin bulunduğu klasör)

# Scriptleri SIRAYLA çalıştır (ÖNEMLİ: Bu sırayla!)
psql -U postgres -d HomeRadar_db -f SQL\01_Database_Schema.sql
psql -U postgres -d HomeRadar_db -f SQL\02_Views.sql
psql -U postgres -d HomeRadar_db -f SQL\03_StoredProcedures.sql
psql -U postgres -d HomeRadar_db -f SQL\04_UserDefinedFunctions.sql
psql -U postgres -d HomeRadar_db -f SQL\05_UserPermissions.sql
```

**Her komut için şifre sorulacak:** Kurulum sırasında belirlediğin `postgres` kullanıcısının şifresini gir.

**Beklenen Çıktı:**
```
Password for user postgres: 
CREATE TABLE
CREATE TABLE
...
CREATE VIEW
...
CREATE FUNCTION
...
DO
GRANT
...
```

**Not:** Encoding hatası görürsen (satır 75 civarında) endişelenme, bu sadece yorum satırlarındaki Türkçe karakterlerden kaynaklanıyor ve önemli komutlar zaten çalışmış oluyor.

---

## 👨‍💻 Üye 2: ML (Machine Learning) - Veri Bilimcisi


### Veritabanı Kurulumu

1. **Ortak Kurulum Adımlarını Tamamla:**
   - [Ortak Kurulum](#ortak-kurulum-herkes-için) bölümündeki tüm adımları yap

2. **Python Bağlantı Kütüphanesini Kur:**
   ```powershell
   pip install psycopg2-binary
   ```

3. **Connection String Yapılandırması:**

   **`config.py` veya bağlantı dosyasında:**
   ```python
   import psycopg2
   
   # Connection string
   DB_CONFIG = {
       'host': 'localhost',
       'port': 5432,
       'database': 'HomeRadar_db',
       'user': 'homeradar_app_user',
       'password': 'HomeRadar2024!SecurePass'
   }
   
   # Bağlantı testi
   def test_connection():
       try:
           conn = psycopg2.connect(**DB_CONFIG)
           cur = conn.cursor()
           cur.execute('SELECT COUNT(*) FROM "Users"')
           count = cur.fetchone()[0]
           print(f"✓ Bağlantı başarılı! Users tablosunda {count} kayıt var.")
           cur.close()
           conn.close()
           return True
       except Exception as e:
           print(f"✗ Bağlantı hatası: {e}")
           return False
   
   if __name__ == "__main__":
       test_connection()
   ```

4. **Veri Ekleme Örneği:**

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
   
   # ÖNEMLİ: Önce Districts ve BuildingTypes tablolarına veri ekle!
   # İlçe ekle
   cur.execute("""
       INSERT INTO "Districts" ("Name", "City") 
       VALUES (%s, %s) 
       ON CONFLICT DO NOTHING
       RETURNING "Id"
   """, ('Yunusemre', 'Manisa'))
   district_result = cur.fetchone()
   if district_result:
       district_id = district_result[0]
   else:
       # Eğer zaten varsa ID'sini al
       cur.execute('SELECT "Id" FROM "Districts" WHERE "Name" = %s', ('Yunusemre',))
       district_id = cur.fetchone()[0]
   
   # Bina tipi ekle
   cur.execute("""
       INSERT INTO "BuildingTypes" ("Name", "Description") 
       VALUES (%s, %s) 
       ON CONFLICT ("Name") DO NOTHING
       RETURNING "Id"
   """, ('Daire', 'Daire'))
   building_type_result = cur.fetchone()
   if building_type_result:
       building_type_id = building_type_result[0]
   else:
       cur.execute('SELECT "Id" FROM "BuildingTypes" WHERE "Name" = %s', ('Daire',))
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

5. **Bağlantı Testi:**
   ```powershell
   python test_connection.py
   ```

### Önemli Notlar

- **Önce Districts ve BuildingTypes tablolarına veri ekle:** `Listings` tablosu bu tablolara foreign key ile bağlı
- **2000+ veri ekle:** Scraping yaptıktan sonra tüm verileri `Listings` tablosuna ekle
- **Veriler SENİN veritabanında görünecek:** Kendi bilgisayarında çalışıyorsan, veriler sadece senin veritabanında olacak
- **Diğer ekip üyeleri:** Herkes kendi veritabanını kullanır, senin verilerini göremez (ortak sunucu yoksa)

### Veri Ekleme Adımları (Detaylı)

**1. Districts Tablosuna İlçeleri Ekle:**
```python
# Önce ilçeleri ekle
districts = ['Yunusemre', 'Şehzadeler', 'Akhisar', 'Salihli', 'Turgutlu', ...]

for district_name in districts:
    cur.execute("""
        INSERT INTO "Districts" ("Name", "City") 
        VALUES (%s, %s) 
        ON CONFLICT DO NOTHING
    """, (district_name, 'Manisa'))
    conn.commit()
```

**2. BuildingTypes Tablosuna Bina Tiplerini Ekle:**
```python
building_types = [
    ('Daire', 'Daire'),
    ('Villa', 'Villa'),
    ('Müstakil', 'Müstakil Ev'),
    ('Residence', 'Residence'),
    ('Dubleks', 'Dubleks'),
    ('Tripleks', 'Tripleks')
]

for name, desc in building_types:
    cur.execute("""
        INSERT INTO "BuildingTypes" ("Name", "Description") 
        VALUES (%s, %s) 
        ON CONFLICT ("Name") DO NOTHING
    """, (name, desc))
    conn.commit()
```

**3. Listings Tablosuna İlanları Ekle (2000+ veri):**
```python
# Scraping yaptıktan sonra
for listing_data in scraped_data:  # 2000+ kayıt
    # İlçe ID'sini al
    cur.execute('SELECT "Id" FROM "Districts" WHERE "Name" = %s', (listing_data['district'],))
    district_id = cur.fetchone()[0]
    
    # Bina tipi ID'sini al
    cur.execute('SELECT "Id" FROM "BuildingTypes" WHERE "Name" = %s', (listing_data['type'],))
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
        listing_data['price'],
        listing_data['square_meters'],
        listing_data['room_count'],
        listing_data['salon_count'],
        listing_data['building_age'],
        listing_data['listing_date'],
        datetime.now(),
        True
    ))
    conn.commit()
```

**4. Verileri Kontrol Et:**
```python
# Kaç kayıt eklendi?
cur.execute('SELECT COUNT(*) FROM "Listings"')
count = cur.fetchone()[0]
print(f"Toplam {count} ilan eklendi!")

# Örnek kayıtları göster
cur.execute('SELECT * FROM "Listings" LIMIT 5')
for row in cur.fetchall():
    print(row)
```

---

## 👨‍💻 Üye 3: SOA/API - Servis Geliştirici

### Görevin
- Node.js veya Vue.js ile ana API'nin yazılması
- gRPC ve SOAP protokolleri ile iletişim kurgulanması
- Dışarıdan bir Hazır API (Google Maps veya Hava Durumu) entegrasyonu
- 6 Katmanlı SOA mimarisinin kurgulanması
- **Veritabanından veri çekme ve API'den sunma** ← Veritabanından veri okuyacaksın

### ⚠️ ÖNEMLİ: Veri Kullanımı

**ML kişisinin verilerini kullanmak için:**
- Eğer ortak sunucu kullanıyorsanız: ML kişisinin verilerini görebilirsin
- Eğer herkes kendi bilgisayarında çalışıyorsa: Kendi veritabanını kullanırsın (ML kişisinin verileri olmaz)
- **Çözüm:** ML kişisinin verilerini export edip, sen de kendi veritabanına import edebilirsin

### Veritabanı Kurulumu

1. **Ortak Kurulum Adımlarını Tamamla:**
   - [Ortak Kurulum](#ortak-kurulum-herkes-için) bölümündeki tüm adımları yap
   - **ÖNEMLİ:** Veritabanını oluştur ve SQL scriptlerini çalıştır

2. **Node.js Bağlantı Kütüphanesini Kur:**
   ```powershell
   npm install pg
   ```

3. **Connection String Yapılandırması:**

   **`config/database.js` veya `db.js`:**
   ```javascript
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
   
   // Bağlantı testi
   pool.query('SELECT COUNT(*) FROM "Users"', (err, res) => {
     if (err) {
       console.error('✗ Bağlantı hatası:', err);
     } else {
       console.log(`✓ Bağlantı başarılı! Users tablosunda ${res.rows[0].count} kayıt var.`);
     }
   });
   
   module.exports = pool;
   ```

4. **API Endpoint Örnekleri:**

   ```javascript
   const express = require('express');
   const pool = require('./config/database');
   const app = express();
   
   // İlçe Listesi Getir
   app.get('/api/districts', async (req, res) => {
     try {
       const result = await pool.query('SELECT * FROM "Districts" ORDER BY "Name"');
       res.json(result.rows);
     } catch (error) {
       res.status(500).json({ error: error.message });
     }
   });
   
   // View Kullanarak İlçe Ortalama Fiyatları
   app.get('/api/districts/avg-prices', async (req, res) => {
     try {
       const result = await pool.query('SELECT * FROM vw_district_avg_prices');
       res.json(result.rows);
     } catch (error) {
       res.status(500).json({ error: error.message });
     }
   });
   
   // Stored Procedure Kullanarak İlanları Getir
   app.get('/api/listings', async (req, res) => {
     try {
       const { districtId, roomCount, minPrice, maxPrice } = req.query;
       const result = await pool.query(
         'SELECT * FROM sp_get_listings_by_criteria($1, $2, $3, $4, NULL, NULL)',
         [districtId || null, roomCount || null, minPrice || null, maxPrice || null]
       );
       res.json(result.rows);
     } catch (error) {
       res.status(500).json({ error: error.message });
     }
   });
   
   app.listen(3000, () => {
     console.log('API server çalışıyor: http://localhost:3000');
   });
   ```

5. **Bağlantı Testi:**
   ```powershell
   node test_connection.js
   ```

### Önemli Notlar

- **View'leri kullan:** 5 hazır view var (örn: `vw_district_avg_prices`)
- **Stored Procedure'leri kullan:** 2 hazır procedure var (örn: `sp_get_listings_by_criteria`)
- **Function'ları kullan:** 2 hazır function var
- **ML kişisinin verilerini kullan:** Üye 2 verileri ekledikten sonra API'den sunabilirsin

---

## 👨‍💻 Üye 4: Web Backend - Full-Stack Backend

### Görevin
- **Proje zaten web uygulaması!** ASP.NET MVC 5 olarak hazır ✅
- Controller ve Action'ların yazılması (Min 5 Controller, 3 Action)
- Kullanıcı Giriş/Kayıt sistemi (Identity) ve 2 farklı Rol (Admin/User) yönetimi
- ViewBag, TempData ile sayfalar arası veri taşıma
- Üye 3'ün yazdığı API'den verileri çekip ön yüze göndermek
- **Entity Framework ile veritabanına bağlanma** ← Veritabanına bağlanacaksın

### ⚠️ ÖNEMLİ: Proje Durumu

**Proje zaten web uygulaması olarak hazır!**
- ✅ ASP.NET MVC 5 yapılandırıldı
- ✅ HomeController örnek olarak eklendi
- ✅ Temel View'ler oluşturuldu
- ✅ Web.config hazır

**Yapman gerekenler:**
1. NuGet paketlerini yükle (MVC paketleri)
2. Controller'ları ekle
3. View'leri oluştur
4. Identity sistemi kur

### Veritabanı Kurulumu

1. **Ortak Kurulum Adımlarını Tamamla:**
   - [Ortak Kurulum](#ortak-kurulum-herkes-için) bölümündeki tüm adımları yap
   - **ÖNEMLİ:** Veritabanını oluştur ve SQL scriptlerini çalıştır

2. **NuGet Paketlerini Yükle:**

   **Visual Studio'da:**
   - Solution Explorer'da projeye sağ tık → **Manage NuGet Packages**
   - **Browse** sekmesinde şu paketleri ara ve yükle:
     - `Microsoft.AspNet.Mvc` (5.2.9)
     - `Microsoft.AspNet.WebPages` (3.2.9)
     - `Microsoft.AspNet.Razor` (3.2.9)

   **Veya Package Manager Console'da:**
   ```powershell
   Install-Package Microsoft.AspNet.Mvc -Version 5.2.9
   Install-Package Microsoft.AspNet.WebPages -Version 3.2.9
   Install-Package Microsoft.AspNet.Razor -Version 3.2.9
   ```

3. **Connection String Kontrolü:**

   **`Web.config` dosyasında zaten var:**
   ```xml
   <connectionStrings>
     <add name="HomeRadarConnection" 
          connectionString="Host=localhost;Port=5432;Database=HomeRadar_db;Username=homeradar_app_user;Password=HomeRadar2024!SecurePass" 
          providerName="Npgsql.EntityFrameworkCore.PostgreSQL" />
   </connectionStrings>
   ```

4. **Controller Örneği (Zaten HomeController var, yeni ekle):**

   **`Controllers/ListingController.cs` oluştur:**
   ```csharp
   using System.Linq;
   using System.Web.Mvc;
   using HomeRadar.Data;
   using HomeRadar.Models;
   using System.Data.Entity;

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

5. **Bağlantı Testi:**
   - Visual Studio'da **F5** tuşuna bas
   - Tarayıcıda `http://localhost:xxxxx` açılmalı
   - Ana sayfa görünmeli

### Önemli Notlar

- **Proje zaten web uygulaması:** Sadece Controller'ları eklemen yeterli
- **Entity Framework kullan:** Mevcut `EmlakContext` sınıfını kullanabilirsin
- **ML kişisinin verilerini kullan:** Üye 2 verileri kendi veritabanına ekledikten sonra, sen de kendi veritabanında çalışırsın
- **API'den veri çek:** Üye 3'ün API'sini kullanarak ML tahminlerini alabilirsin
- **Detaylı rehber:** `README_MEMBER4.md` dosyasına bak

---

## 👨‍💻 Üye 5: Frontend - UI/UX Tasarımcı

### Görevin
- Özgün bir Layout tasarımı ve bunun 3 view'de uygulanması
- PartialView ve ViewComponent yapılarını kullanarak sayfa parçalarının oluşturulması
- Esnek (Responsive) tasarımların yapılması
- Form tasarımları ve kullanıcı deneyimi
- **Veritabanı bağlantısı gerekmez** ← Backend üzerinden çalışacaksın

### Veritabanı Kurulumu

**Frontend için veritabanı kurulumu gerekmez!**

- Backend (Üye 4) veritabanına bağlanır
- Frontend sadece HTML/CSS/JavaScript ile çalışır
- Veriler backend'den ViewBag/TempData ile gelir

### Yapman Gerekenler

1. **Üye 4'ün hazırladığı Controller'ları kullan**
2. **ViewBag ve TempData ile veri alışverişi yap**
3. **Form alanlarını tasarla:**

   ```html
   <!-- Tahmin Formu Örneği -->
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
           <label>Metrekare:</label>
           <input asp-for="SquareMeters" type="number" min="1" step="0.01" />
       </div>
       <button type="submit">Hesapla</button>
   </form>
   ```

### Önemli Notlar

- **Veritabanı kurulumu gerekmez:** Backend hallediyor
- **Sadece tasarım yap:** HTML, CSS, JavaScript
- **Backend ile entegre çalış:** Üye 4'ün Controller'larını kullan

---

## 🔧 Sorun Giderme

### Problem 1: "Connection refused" veya "No connection could be made"

**Sebep:** PostgreSQL servisi çalışmıyor.

**Çözüm:**
- `Win + R` → `services.msc` → PostgreSQL servisini başlat

---

### Problem 2: "Password authentication failed for user 'homeradar_app_user'"

**Sebep:** Kullanıcı oluşturulmamış.

**Çözüm:**
```powershell
# 05_UserPermissions.sql dosyasını tekrar çalıştır
psql -U postgres -d HomeRadar_db -f SQL\05_UserPermissions.sql
```

---

### Problem 3: "Database does not exist"

**Sebep:** Veritabanı oluşturulmamış.

**Çözüm:**
```sql
CREATE DATABASE "HomeRadar_db" WITH ENCODING = 'UTF8';
```

---

### Problem 4: "Table does not exist"

**Sebep:** SQL scriptleri çalıştırılmamış.

**Çözüm:**
```powershell
# Scriptleri sırayla çalıştır
psql -U postgres -d HomeRadar_db -f SQL\01_Database_Schema.sql
# ... diğer scriptler
```

---

### Problem 5: "Connection string 'HomeRadarConnection' bulunamadı!"

**Sebep:** `App.config` veya `appsettings.json` yanlış yapılandırılmış.

**Çözüm:**
- Connection string'i kontrol et
- Şifrenin doğru olduğundan emin ol

---

## 📋 Kontrol Listesi

Kurulumu tamamladıktan sonra şunları kontrol et:

- [ ] PostgreSQL kurulu ve çalışıyor
- [ ] `HomeRadar_db` veritabanı oluşturuldu (UTF-8)
- [ ] SQL scriptleri çalıştırıldı (5 dosya sırayla)
- [ ] `homeradar_app_user` kullanıcısı oluşturuldu
- [ ] Connection string yapılandırıldı (görevine göre)
- [ ] Bağlantı test edildi ve başarılı

---

## 📞 Yardım

Sorun yaşarsan:
1. Bu dokümantasyonu tekrar oku
2. "Sorun Giderme" bölümüne bak
3. Takım lideri (Veritabanı Yöneticisi) ile iletişime geç

---

## 🎯 Hızlı Başlangıç Özeti

### Herkes İçin (5 Dakika)
1. PostgreSQL kur (https://www.postgresql.org/download/windows/)
2. Veritabanı oluştur: `CREATE DATABASE "HomeRadar_db" WITH ENCODING = 'UTF8';`
3. SQL scriptlerini çalıştır (01-05 arası)
4. Connection string'i yapılandır

### Üye 2 (ML) - Veri Ekleme
1. Python ile scraping yap (2000+ veri)
2. Verileri temizle (EDA)
3. PostgreSQL'e ekle (`Listings` tablosuna)
4. **Veriler SENİN veritabanında olacak!**

### Üye 3 (SOA/API) - API Geliştirme
1. Node.js kur
2. `pg` paketini yükle
3. API endpoint'leri yaz
4. Veritabanından veri çek ve API'den sun

### Üye 4 (Web Backend) - MVC Controller
1. Proje zaten web uygulaması ✅
2. NuGet paketlerini yükle (MVC)
3. Controller'ları ekle (Min 5 Controller)
4. Identity sistemi kur
5. ViewBag, TempData kullan

### Üye 5 (Frontend) - UI/UX
1. Layout tasarla
2. View'leri oluştur (3+ view)
3. PartialView ve ViewComponent kullan
4. Responsive tasarım yap

---

## 📊 ML Verilerinin Paylaşımı

### Senaryo 1: Herkes Kendi Bilgisayarında Çalışıyor
- ML kişisi verileri **sadece kendi veritabanına** ekler
- Diğerleri kendi veritabanlarında çalışır (ML verileri olmaz)
- **Çözüm:** ML kişisi verileri export edip, diğerleri import edebilir

### Senaryo 2: Ortak Sunucu Kullanılıyor
- Herkes aynı PostgreSQL sunucusuna bağlanır
- ML kişisi verileri eklediğinde, **herkes görebilir** ✅
- En pratik çözüm!

### ML Verilerini Export/Import Etme

**ML Kişisi (Export):**
```powershell
# Tüm veritabanını export et
pg_dump -U postgres -d HomeRadar_db -f backup.sql

# Sadece Listings tablosunu export et
pg_dump -U postgres -d HomeRadar_db -t "Listings" -f listings_backup.sql
```

**Diğer Üyeler (Import):**
```powershell
# Tüm veritabanını import et
psql -U postgres -d HomeRadar_db -f backup.sql

# Sadece Listings tablosunu import et
psql -U postgres -d HomeRadar_db -f listings_backup.sql
```

---

## 🎯 Sonuç

Tüm adımları tamamladıktan sonra:
- ✅ Veritabanı hazır
- ✅ Bağlantı çalışıyor
- ✅ Görevine göre projeyi geliştirmeye başlayabilirsin

**İyi çalışmalar! 🚀**
