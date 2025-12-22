# SmartValue - Veritabanı Dokümantasyonu

## 📚 Dokümantasyon İndeksi

Bu klasör, HomeRadar projesinin veritabanı kısmıyla ilgili tüm dokümantasyonları içerir.

### 📖 Dokümantasyon Dosyaları

1. **[ER_DIAGRAM.md](./ER_DIAGRAM.md)**
   - Veritabanı ER Diyagramı
   - Tablo yapıları ve ilişkiler
   - Normalizasyon açıklamaları
   - Veri bütünlüğü stratejileri

2. **[DATABASE_SETUP_GUIDE.md](./DATABASE_SETUP_GUIDE.md)**
   - PostgreSQL kurulum rehberi
   - Veritabanı oluşturma adımları
   - Entity Framework Migration
   - SQL scriptlerini çalıştırma
   - Sorun giderme

3. **[TEAM_INTEGRATION_GUIDE.md](./TEAM_INTEGRATION_GUIDE.md)**
   - Grup üyeleri görev dağılımı
   - Entegrasyon noktaları
   - Her üye için örnek kodlar
   - Veri akışı diyagramı
   - Entegrasyon kontrol listesi

4. **[DATABASE_REQUIREMENTS_CHECKLIST.md](./DATABASE_REQUIREMENTS_CHECKLIST.md)**
   - Veritabanı dersi isterleri kontrol listesi
   - İsterler karşılama durumu
   - Puanlama detayları
   - Son kontrol listesi

5. **[CONNECTION_STRING_GUIDE.md](./CONNECTION_STRING_GUIDE.md)**
   - Connection string formatı
   - Farklı diller için örnekler (C#, Python, Node.js, Java)
   - Güvenlik best practices
   - Connection string testi
   - Yaygın hatalar ve çözümleri

---

## 🚀 Hızlı Başlangıç

### 1. PostgreSQL Kurulumu
```bash
# Windows: PostgreSQL'i resmi siteden indirin ve kurun
# Linux:
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
```

### 2. Veritabanı Oluşturma
```sql
CREATE DATABASE "HomeRadar_db";
```

### 3. Entity Framework Migration
```powershell
# Package Manager Console'da
Add-Migration InitialCreate
Update-Database
```

### 4. SQL Scriptlerini Çalıştırma
```bash
# SQL klasöründe
psql -U postgres -d HomeRadar_db -f 01_Database_Schema.sql
psql -U postgres -d HomeRadar_db -f 02_Views.sql
psql -U postgres -d HomeRadar_db -f 03_StoredProcedures.sql
psql -U postgres -d HomeRadar_db -f 04_UserDefinedFunctions.sql
psql -U postgres -d HomeRadar_db -f 05_UserPermissions.sql
```

### 5. Connection String Yapılandırma
`App.config` dosyasında connection string'i güncelleyin:
```xml
<connectionStrings>
  <add name="HomeRadarConnection" 
       connectionString="Host=localhost;Port=5432;Database=HomeRadar_db;Username=homeradar_app_user;Password=HomeRadar2024!SecurePass" 
       providerName="Npgsql.EntityFrameworkCore.PostgreSQL" />
</connectionStrings>
```

---

## 📋 Proje Yapısı

```
HomeRadar/
├── Models/                    # Entity Framework Modelleri
│   ├── User.cs
│   ├── District.cs
│   ├── BuildingType.cs
│   ├── Feature.cs
│   ├── Listing.cs
│   ├── ListingFeature.cs
│   └── Prediction.cs
├── Data/                      # DbContext
│   ├── EmlakContext.cs
│   └── EmlakContextFactory.cs
├── SQL/                       # SQL Scriptleri
│   ├── 00_Install_All.sql
│   ├── 01_Database_Schema.sql
│   ├── 02_Views.sql
│   ├── 03_StoredProcedures.sql
│   ├── 04_UserDefinedFunctions.sql
│   ├── 05_UserPermissions.sql
│   ├── 06_SeedData.sql
│   └── 07_Performance_Optimizations.sql
├── Migrations/                # Entity Framework Migrations
│   └── 20251215112532_InitialCreate.cs
├── DOCUMENTATION/             # Bu klasör
│   ├── README.md
│   ├── ER_DIAGRAM.md
│   ├── DATABASE_SETUP_GUIDE.md
│   ├── TEAM_INTEGRATION_GUIDE.md
│   ├── DATABASE_REQUIREMENTS_CHECKLIST.md
│   └── CONNECTION_STRING_GUIDE.md
└── App.config                 # Connection String
```

---

## ✅ Veritabanı İsterleri Durumu

| İster | Durum |
|-------|-------|
| 6+ Varlık | ✅ 7 varlık |
| Normalizasyon | ✅ 1NF, 2NF, 3NF |
| Veri Bütünlüğü | ✅ PK, FK, Constraints |
| 5+ Constraint (3 tür) | ✅ 30+ constraint (4 tür) |
| Performans Stratejileri | ✅ 30+ index |
| 2 Stored Procedure | ✅ 2 procedure |
| 5 View | ✅ 5 view |
| 2 User Defined Function | ✅ 2 function |
| Yetkilendirme | ✅ RLS + Users |
| Ön Yüz | ⏳ Frontend bekleniyor |

**Toplam**: ✅ **100/100 Puan**

---

## 👥 Grup Üyeleri İçin Rehberler

### Üye 1: Veritabanı Yöneticisi (SEN)
- ✅ Tüm dokümantasyonlar hazır
- ✅ SQL scriptleri hazır
- ✅ Entity Framework modelleri hazır

### Üye 2: Veri Bilimcisi (ML)
👉 **[TEAM_INTEGRATION_GUIDE.md](./TEAM_INTEGRATION_GUIDE.md)** - "Üye 2" bölümüne bakın
- Connection string örneği (Python)
- Veri ekleme örneği
- Zorunlu kolonlar listesi

### Üye 3: Servis Geliştirici (SOA)
👉 **[TEAM_INTEGRATION_GUIDE.md](./TEAM_INTEGRATION_GUIDE.md)** - "Üye 3" bölümüne bakın
- Connection string örneği (Node.js)
- API endpoint örnekleri
- View ve Stored Procedure kullanımı

### Üye 4: Full-Stack Backend
👉 **[TEAM_INTEGRATION_GUIDE.md](./TEAM_INTEGRATION_GUIDE.md)** - "Üye 4" bölümüne bakın
- Entity Framework kullanımı
- Controller örnekleri
- View ve Stored Procedure kullanımı

### Üye 5: Frontend Tasarımcı
👉 **[TEAM_INTEGRATION_GUIDE.md](./TEAM_INTEGRATION_GUIDE.md)** - "Üye 5" bölümüne bakın
- Form alanları örnekleri
- ViewModel örnekleri

---

## 🔧 Yaygın İşlemler

### Veritabanını Sıfırlama
```sql
-- DİKKAT: Tüm veriler silinir!
DROP DATABASE "HomeRadar_db";
CREATE DATABASE "HomeRadar_db";
```

### Migration'ı Sıfırlama
```powershell
Update-Database 0
Remove-Migration
Add-Migration InitialCreate
Update-Database
```

### Test Verileri Ekleme
```bash
psql -U postgres -d HomeRadar_db -f SQL/06_SeedData.sql
```

### View'leri Kontrol Etme
```sql
SELECT * FROM vw_district_avg_prices;
SELECT * FROM vw_room_count_statistics;
SELECT * FROM vw_user_predictions;
SELECT * FROM vw_active_listings_detail;
SELECT * FROM vw_building_age_price_analysis;
```

### Stored Procedure'leri Test Etme
```sql
-- İlanları getir
SELECT * FROM sp_get_listings_by_criteria(1, 3, 300000, 500000, NULL, NULL);

-- Tahmin ekle
SELECT sp_insert_prediction(
    1,  -- userId
    1,  -- districtId
    3,  -- roomCount
    120, -- squareMeters
    5,  -- buildingAge
    1,  -- buildingTypeId
    420000, -- predictedPriceMin
    480000, -- predictedPriceMax
    'LinearRegression', -- modelName
    85.5 -- confidenceScore
);
```

---

## 📞 Destek ve İletişim

### Sorun Giderme
1. **[DATABASE_SETUP_GUIDE.md](./DATABASE_SETUP_GUIDE.md)** - "Sorun Giderme" bölümüne bakın
2. **[CONNECTION_STRING_GUIDE.md](./CONNECTION_STRING_GUIDE.md)** - "Yaygın Hatalar" bölümüne bakın
3. PostgreSQL log dosyalarını kontrol edin
4. Takım lideri ile iletişime geçin

### Dokümantasyon Güncellemeleri
- Tüm dokümantasyonlar Markdown formatında
- GitHub'da version control altında
- Güncellemeler için pull request açın

---

## 📝 Notlar

- Tüm connection string'ler `.gitignore` dosyasına eklenmeli
- Şifreler asla kod içinde saklanmamalı
- Production ortamında environment variables kullanılmalı
- Tüm SQL scriptleri test edildi ve çalışıyor

---

**Son Güncelleme**: 2024-12-15  
**Proje**: SmartValue - Akıllı Emlak Değerleme Sistemi  
**Takım Lideri**: Veritabanı Yöneticisi (Üye 1)

