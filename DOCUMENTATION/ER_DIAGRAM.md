# SmartValue - Veritabanı ER Diyagramı

## 📊 Varlık İlişki Modeli (ER Model)

### Tablolar ve İlişkiler

```
┌─────────────────┐
│     Users       │
├─────────────────┤
│ PK Id           │
│    Email (UK)   │
│    PasswordHash │
│    FirstName    │
│    LastName     │
│    Role         │
│    CreatedAt    │
│    IsActive     │
└────────┬────────┘
         │
         │ 1:N
         │
         ▼
┌─────────────────┐
│  Predictions    │
├─────────────────┤
│ PK Id           │
│ FK UserId (N)   │◄──┐
│ FK ListingId(N) │   │
│ FK DistrictId   │───┼──┐
│ FK BuildingTypeId(N)│  │
│    RoomCount    │   │  │
│    SquareMeters │   │  │
│    BuildingAge  │   │  │
│    PredictedPriceMin│ │
│    PredictedPriceMax│ │
│    PredictedPriceAvg│ │
│    ModelName    │   │  │
│    ConfidenceScore│ │
│    CreatedAt    │   │  │
└─────────────────┘   │  │
                       │  │
┌─────────────────┐    │  │
│   Districts     │    │  │
├─────────────────┤    │  │
│ PK Id           │────┘  │
│    Name         │       │
│    City         │       │
└────────┬────────┘       │
         │                │
         │ 1:N            │
         │                │
         ▼                │
┌─────────────────┐       │
│    Listings     │       │
├─────────────────┤       │
│ PK Id           │       │
│ FK DistrictId   │───────┘
│ FK BuildingTypeId│──┐
│    Price        │   │
│    SquareMeters │   │
│    RoomCount    │   │
│    SalonCount   │   │
│    BathroomCount│   │
│    Floor        │   │
│    BuildingAge  │   │
│    Aidat        │   │
│    HeatingType  │   │
│    Direction    │   │
│    BuildingStatus│   │
│    UsageStatus  │   │
│    DeedStatus   │   │
│    FurnitureStatus│   │
│    HasBalcony   │   │
│    HasElevator  │   │
│    HasGarage    │   │
│    IsInComplex  │   │
│    HasSecurity  │   │
│    IsCreditSuitable│ │
│    IsExchangeable│   │
│    Neighborhood │   │
│    ListingDate  │   │
│    CreatedAt    │   │
│    IsActive     │   │
└────────┬────────┘   │
         │            │
         │ 1:N        │
         │            │
         ▼            │
┌─────────────────┐   │
│ ListingFeatures │   │
├─────────────────┤   │
│ PK Id           │   │
│ FK ListingId    │───┘
│ FK FeatureId    │──┐
└─────────────────┘   │
                      │
┌─────────────────┐   │
│   BuildingTypes │   │
├─────────────────┤   │
│ PK Id           │───┘
│    Name (UK)    │
│    Description  │
└─────────────────┘

┌─────────────────┐
│    Features      │
├─────────────────┤
│ PK Id           │
│    Name (UK)    │
│    Description  │
└─────────────────┘
```

## 📋 Tablo Detayları

### 1. Users (Kullanıcılar)
- **Amaç**: Sistem kullanıcıları (Admin ve User rolleri)
- **Primary Key**: Id
- **Unique Constraint**: Email
- **Check Constraint**: Role IN ('Admin', 'User')
- **İlişkiler**: 
  - 1:N → Predictions

### 2. Districts (İlçeler)
- **Amaç**: Manisa ilçeleri (Normalizasyon)
- **Primary Key**: Id
- **Index**: Name
- **İlişkiler**: 
  - 1:N → Listings
  - 1:N → Predictions

### 3. BuildingTypes (Bina Tipleri)
- **Amaç**: Daire, Villa, Müstakil vb.
- **Primary Key**: Id
- **Unique Constraint**: Name
- **İlişkiler**: 
  - 1:N → Listings
  - 1:N → Predictions

### 4. Features (Özellikler)
- **Amaç**: Balkon, Asansör, Garaj vb.
- **Primary Key**: Id
- **Unique Constraint**: Name
- **İlişkiler**: 
  - N:M → Listings (ListingFeatures üzerinden)

### 5. Listings (İlanlar)
- **Amaç**: Ana emlak verileri
- **Primary Key**: Id
- **Foreign Keys**: 
  - DistrictId → Districts
  - BuildingTypeId → BuildingTypes
- **Check Constraints**: 
  - Price > 0
  - SquareMeters > 0
  - RoomCount > 0
  - BuildingAge >= 0
- **Indexes**: 
  - DistrictId, BuildingTypeId, Price, SquareMeters, RoomCount, ListingDate
- **İlişkiler**: 
  - N:1 → Districts
  - N:1 → BuildingTypes
  - N:M → Features (ListingFeatures üzerinden)
  - 1:N → Predictions

### 6. ListingFeatures (İlan-Özellik İlişkisi)
- **Amaç**: Many-to-Many ilişki tablosu
- **Primary Key**: Id
- **Foreign Keys**: 
  - ListingId → Listings (CASCADE DELETE)
  - FeatureId → Features (RESTRICT DELETE)
- **Unique Constraint**: (ListingId, FeatureId)

### 7. Predictions (Tahminler)
- **Amaç**: ML model tahmin sonuçları
- **Primary Key**: Id
- **Foreign Keys**: 
  - UserId → Users (NULLABLE, SET NULL)
  - ListingId → Listings (NULLABLE, SET NULL)
  - DistrictId → Districts (RESTRICT)
  - BuildingTypeId → BuildingTypes (NULLABLE, SET NULL)
- **Check Constraints**: 
  - PredictedPriceMin > 0
  - PredictedPriceMax >= PredictedPriceMin
  - ConfidenceScore BETWEEN 0 AND 100 (NULLABLE)
- **Indexes**: 
  - UserId, DistrictId, CreatedAt

## 🔗 İlişki Tipleri

| İlişki | Tip | Açıklama |
|--------|-----|----------|
| Users → Predictions | 1:N | Bir kullanıcı birden fazla tahmin yapabilir |
| Districts → Listings | 1:N | Bir ilçede birden fazla ilan olabilir |
| Districts → Predictions | 1:N | Bir ilçe için birden fazla tahmin yapılabilir |
| BuildingTypes → Listings | 1:N | Bir bina tipinde birden fazla ilan olabilir |
| BuildingTypes → Predictions | 1:N | Bir bina tipi için birden fazla tahmin yapılabilir |
| Listings ↔ Features | N:M | Bir ilan birden fazla özelliğe sahip olabilir |
| Listings → Predictions | 1:N | Bir ilan için birden fazla tahmin yapılabilir |

## 📊 Normalizasyon

### 1NF (First Normal Form)
✅ Tüm tablolar atomik değerlere sahip
✅ Her kolon tek bir değer tutuyor

### 2NF (Second Normal Form)
✅ Tüm tablolar 1NF'de
✅ Tüm non-key kolonlar primary key'e tam bağımlı
✅ Districts tablosu ile ilçe bilgisi normalize edilmiş
✅ BuildingTypes tablosu ile bina tipi bilgisi normalize edilmiş

### 3NF (Third Normal Form)
✅ Tüm tablolar 2NF'de
✅ Transitive dependencies yok
✅ Districts ve BuildingTypes ayrı tablolarda

## 🔐 Veri Bütünlüğü Stratejisi

### Primary Keys
- Tüm tablolarda `Id` (INTEGER, AUTO_INCREMENT)

### Foreign Keys
- **CASCADE DELETE**: ListingFeatures → Listings (İlan silinince özellikleri de silinir)
- **RESTRICT DELETE**: 
  - Listings → Districts (İlçe silinemez, ilanlar varsa)
  - Listings → BuildingTypes (Bina tipi silinemez, ilanlar varsa)
  - ListingFeatures → Features (Özellik silinemez, kullanılıyorsa)
  - Predictions → Districts (İlçe silinemez, tahminler varsa)
- **SET NULL**: 
  - Predictions → Users (Kullanıcı silinince tahminler kalır, UserId NULL olur)
  - Predictions → Listings (İlan silinince tahminler kalır, ListingId NULL olur)
  - Predictions → BuildingTypes (Bina tipi silinince tahminler kalır, BuildingTypeId NULL olur)

### Constraints
1. **Check Constraints** (5 adet):
   - Users.Role IN ('Admin', 'User') - Trigger ile
   - Listings.Price > 0
   - Listings.SquareMeters > 0
   - Listings.RoomCount > 0
   - Listings.BuildingAge >= 0
   - Predictions.PredictedPriceMin > 0 AND PredictedPriceMax >= PredictedPriceMin
   - Predictions.ConfidenceScore BETWEEN 0 AND 100 (NULLABLE)

2. **Unique Constraints**:
   - Users.Email
   - BuildingTypes.Name
   - Features.Name
   - ListingFeatures(ListingId, FeatureId)

3. **Not Null Constraints**:
   - Tüm Primary Key'ler
   - Tüm Foreign Key'ler (NULLABLE olanlar hariç)
   - Users: Email, PasswordHash, FirstName, LastName, Role
   - Districts: Name
   - BuildingTypes: Name
   - Features: Name
   - Listings: DistrictId, BuildingTypeId, Price, SquareMeters, RoomCount, SalonCount, BuildingAge
   - Predictions: DistrictId, RoomCount, SquareMeters, BuildingAge, PredictedPriceMin, PredictedPriceMax

## 📈 Performans Stratejileri

### Indexes
1. **Primary Key Indexes**: Tüm tablolarda otomatik
2. **Foreign Key Indexes**: Tüm foreign key'lerde otomatik
3. **Unique Indexes**: Email, BuildingTypes.Name, Features.Name
4. **Performance Indexes**:
   - Listings: DistrictId, BuildingTypeId, Price, SquareMeters, RoomCount, ListingDate
   - Predictions: UserId, DistrictId, CreatedAt
   - Districts: Name
5. **Composite Indexes** (Performance Optimizations):
   - Listings(DistrictId, Price)
   - Listings(DistrictId, RoomCount, IsActive)
   - Listings(Price, SquareMeters)
   - Predictions(DistrictId, RoomCount, SquareMeters)

### Views (5 adet)
1. `vw_district_avg_prices` - İlçe bazında ortalama fiyatlar
2. `vw_room_count_statistics` - Oda sayısına göre istatistikler
3. `vw_user_predictions` - Kullanıcı tahmin geçmişi
4. `vw_active_listings_detail` - Aktif ilanlar detaylı bilgi
5. `vw_building_age_price_analysis` - Bina yaşına göre fiyat analizi

### Stored Procedures (2 adet)
1. `sp_get_listings_by_criteria` - Kriterlere göre ilan getir
2. `sp_insert_prediction` - Tahmin kaydı ekle

### User Defined Functions (2 adet)
1. `fn_calculate_price_per_square_meter` - Metrekare başına fiyat hesapla
2. `fn_estimate_price_by_district` - İlçe için ortalama fiyat tahmini

## 🔒 Güvenlik ve Yetkilendirme

### Kullanıcı Rolleri
- **homeradar_app_user**: Uygulama kullanıcısı (SELECT, INSERT, UPDATE, DELETE)
- **homeradar_readonly**: Raporlama kullanıcısı (sadece SELECT)

### Row Level Security (RLS)
- Predictions tablosunda kullanıcı izolasyonu (kullanıcılar sadece kendi tahminlerini görebilir)

## 📝 Notlar

- Tüm tablo ve kolon isimleri PostgreSQL naming convention'ına uygun (tırnak içinde)
- DateTime kolonları TIMESTAMP tipinde
- Decimal kolonlar için precision belirtilmiş (Price: 18,2; SquareMeters: 10,2)
- Boolean kolonlar BOOLEAN tipinde (NULLABLE olanlar var)
- String kolonlar için MaxLength belirtilmiş

