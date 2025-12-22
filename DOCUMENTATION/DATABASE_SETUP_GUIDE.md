# SmartValue - Veritabanı Kurulum Rehberi

## 📋 İçindekiler
1. [Gereksinimler](#gereksinimler)
2. [PostgreSQL Kurulumu](#postgresql-kurulumu)
3. [Veritabanı Oluşturma](#veritabanı-oluşturma)
4. [Entity Framework Migration](#entity-framework-migration)
5. [SQL Scriptlerini Çalıştırma](#sql-scriptlerini-çalıştırma)
6. [Bağlantı Testi](#bağlantı-testi)
7. [ER Diyagramı Oluşturma (pgAdmin)](#er-diyagramı-oluşturma-pgadmin)
8. [Sorun Giderme](#sorun-giderme)

## 🔧 Gereksinimler

### Yazılım Gereksinimleri
- PostgreSQL 12 veya üzeri
- .NET Framework 4.8.1
- Entity Framework Core 3.1.32
- Npgsql.EntityFrameworkCore.PostgreSQL 3.1.11
- Visual Studio 2019 veya üzeri (veya Visual Studio Code)

### Sistem Gereksinimleri
- Windows 10/11 veya Linux
- En az 2 GB RAM
- En az 500 MB disk alanı

## 🗄️ PostgreSQL Kurulumu

### Windows
1. PostgreSQL'i [resmi siteden](https://www.postgresql.org/download/windows/) indirin
2. Kurulum sırasında:
   - Port: **5432** (varsayılan)
   - Superuser şifresi: **Güvenli bir şifre belirleyin** (örn: `postgres123`)
   - Locale: **Turkish, Turkey** (opsiyonel)

### Linux (Ubuntu/Debian)
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

### macOS
```bash
brew install postgresql
brew services start postgresql
```

## 📦 Veritabanı Oluşturma

### 1. PostgreSQL'e Bağlanma

**Windows (pgAdmin veya psql):**
```sql
-- psql ile bağlanma
psql -U postgres
```

**Linux/macOS:**
```bash
sudo -u postgres psql
```

### 2. Veritabanı ve Kullanıcı Oluşturma

```sql
-- Veritabanı oluştur
CREATE DATABASE "HomeRadar_db"
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'Turkish_Turkey.1254'
    LC_CTYPE = 'Turkish_Turkey.1254'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

-- Veritabanına bağlan
\c "HomeRadar_db"

-- Uygulama kullanıcısı oluştur (şimdilik, SQL scripti bunu yapacak)
-- Bu adımı atlayabilirsiniz, 05_UserPermissions.sql bunu yapacak
```

## 🔄 Entity Framework Migration

### 1. Package Manager Console'da

Visual Studio'da **Tools → NuGet Package Manager → Package Manager Console** açın:

```powershell
# Migration oluştur
Add-Migration InitialCreate

# Veritabanını güncelle
Update-Database
```

### 2. Komut Satırından (dotnet CLI)

```bash
# Migration oluştur
dotnet ef migrations add InitialCreate

# Veritabanını güncelle
dotnet ef database update
```

### 3. Migration Başarısız Olursa

Eğer migration başarısız olursa:

```powershell
# Tüm migration'ları geri al
Update-Database 0

# Migration'ı sil
Remove-Migration

# Yeni migration oluştur
Add-Migration InitialCreate

# Tekrar güncelle
Update-Database
```

## 📜 SQL Scriptlerini Çalıştırma

### Sıralama (ÖNEMLİ!)

SQL scriptlerini **mutlaka bu sırayla** çalıştırın:

1. **01_Database_Schema.sql** - Constraints ve Indexes
2. **02_Views.sql** - View'ler
3. **03_StoredProcedures.sql** - Stored Procedures
4. **04_UserDefinedFunctions.sql** - User Defined Functions
5. **05_UserPermissions.sql** - Kullanıcı yetkilendirme
6. **06_SeedData.sql** - Test verileri (Opsiyonel)
7. **07_Performance_Optimizations.sql** - Performans iyileştirmeleri (Opsiyonel)

### Yöntem 1: psql ile (Önerilen)

```bash
# Windows PowerShell
cd SQL
psql -U postgres -d HomeRadar_db -f 01_Database_Schema.sql
psql -U postgres -d HomeRadar_db -f 02_Views.sql
psql -U postgres -d HomeRadar_db -f 03_StoredProcedures.sql
psql -U postgres -d HomeRadar_db -f 04_UserDefinedFunctions.sql
psql -U postgres -d HomeRadar_db -f 05_UserPermissions.sql
psql -U postgres -d HomeRadar_db -f 06_SeedData.sql
```

### Yöntem 2: pgAdmin ile

1. pgAdmin'i açın
2. HomeRadar_db veritabanına sağ tıklayın → **Query Tool**
3. Her SQL dosyasını sırayla açıp çalıştırın (F5)

### Yöntem 3: PowerShell Script ile

```powershell
# SQL klasöründe
.\Install-Database.ps1
```

### Yöntem 4: Tek Dosyadan (00_Install_All.sql)

```sql
-- psql içinde
\i SQL/00_Install_All.sql
```

## ✅ Bağlantı Testi

### 1. App.config Kontrolü

`App.config` dosyasında connection string'i kontrol edin:

```xml
<connectionStrings>
  <add name="HomeRadarConnection" 
       connectionString="Host=localhost;Port=5432;Database=HomeRadar_db;Username=homeradar_app_user;Password=HomeRadar2024!SecurePass" 
       providerName="Npgsql.EntityFrameworkCore.PostgreSQL" />
</connectionStrings>
```

**ÖNEMLİ:** 
- Migration için `postgres` kullanıcısı kullanılır (superuser yetkisi gerekli)
- Uygulama çalışırken `homeradar_app_user` kullanılır

### 2. Program.cs ile Test

```bash
# Projeyi çalıştır
dotnet run

# veya Visual Studio'da F5
```

Çıktı:
```
SmartValue - Emlak Değerleme Sistemi
=====================================

Veritabanı bağlantısı test ediliyor...
✓ Veritabanı bağlantısı başarılı!

Tablolar kontrol ediliyor...
  - Users: OK
  - Districts: OK
  - BuildingTypes: OK
  - Features: OK
  - Listings: OK
  - ListingFeatures: OK
  - Predictions: OK

Veri İstatistikleri:
  - Kullanıcılar: 3
  - İlçeler: 10
  - Bina Tipleri: 6
  - Özellikler: 10
  - İlanlar: 10
  - Tahminler: 4
```

### 3. SQL ile Test

```sql
-- Tabloları kontrol et
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;

-- View'leri kontrol et
SELECT table_name 
FROM information_schema.views 
WHERE table_schema = 'public';

-- Stored procedure'leri kontrol et
SELECT routine_name 
FROM information_schema.routines 
WHERE routine_schema = 'public' 
AND routine_type = 'FUNCTION';

-- Constraint'leri kontrol et
SELECT constraint_name, table_name 
FROM information_schema.table_constraints 
WHERE table_schema = 'public';
```

## 📊 ER Diyagramı Oluşturma (pgAdmin)

**ÖNEMLİ:** ER diyagramı pgAdmin'de **otomatik oluşmaz**. Veritabanı şemasından görsel ER diyagramı oluşturmak için aşağıdaki yöntemlerden birini kullanabilirsiniz.

### Yöntem 1: pgAdmin ERD Tool (pgAdmin 4.5+)

pgAdmin 4.5 ve üzeri versiyonlarda ERD (Entity Relationship Diagram) tool bulunur:

1. **pgAdmin'i açın** ve `HomeRadar_db` veritabanına bağlanın
2. **HomeRadar_db** veritabanına sağ tıklayın
3. **ERD For Database** seçeneğini seçin
4. ERD Tool penceresi açılacak
5. Tüm tabloları seçin (Ctrl+A) veya istediğiniz tabloları seçin
6. **Generate** butonuna tıklayın
7. ER diyagramı otomatik oluşturulacak
8. Diyagramı kaydetmek için: **File → Save** (PNG, SVG veya PDF formatında)

**Not:** Eğer "ERD For Database" seçeneği görünmüyorsa, pgAdmin versiyonunuz eski olabilir. Yöntem 2'yi kullanın.

### Yöntem 2: DBeaver (Ücretsiz Alternatif)

DBeaver, PostgreSQL için ücretsiz bir veritabanı yönetim aracıdır ve ER diyagramı oluşturma özelliği içerir:

1. [DBeaver Community Edition](https://dbeaver.io/download/) indirin ve kurun
2. PostgreSQL bağlantısı oluşturun:
   - Host: `localhost`
   - Port: `5432`
   - Database: `HomeRadar_db`
   - Username: `postgres` (veya `homeradar_app_user`)
3. **Database → View Diagram** seçeneğini seçin
4. Tüm tabloları seçin ve ER diyagramı otomatik oluşturulur
5. Diyagramı PNG, SVG veya PDF olarak kaydedebilirsiniz

### Yöntem 3: Online Araçlar

1. **dbdiagram.io** (Ücretsiz):
   - PostgreSQL veritabanınıza bağlanın
   - Otomatik ER diyagramı oluşturur
   - Export: PNG, PDF, SQL

2. **dbml.app**:
   - Veritabanı şemasından ER diyagramı oluşturur
   - Görsel düzenleme yapabilirsiniz

### Yöntem 4: SQL ile ER Diyagramı Bilgilerini Çıkarma

ER diyagramı oluşturmak için gerekli bilgileri SQL ile çıkarabilirsiniz:

```sql
-- Tablolar ve kolonlar
SELECT 
    t.table_name,
    c.column_name,
    c.data_type,
    c.is_nullable,
    CASE WHEN pk.column_name IS NOT NULL THEN 'PK' END AS primary_key,
    CASE WHEN fk.column_name IS NOT NULL THEN 'FK' END AS foreign_key
FROM information_schema.tables t
LEFT JOIN information_schema.columns c ON t.table_name = c.table_name
LEFT JOIN (
    SELECT ku.table_name, ku.column_name
    FROM information_schema.table_constraints tc
    JOIN information_schema.key_column_usage ku 
        ON tc.constraint_name = ku.constraint_name
    WHERE tc.constraint_type = 'PRIMARY KEY'
) pk ON c.table_name = pk.table_name AND c.column_name = pk.column_name
LEFT JOIN (
    SELECT ku.table_name, ku.column_name
    FROM information_schema.table_constraints tc
    JOIN information_schema.key_column_usage ku 
        ON tc.constraint_name = ku.constraint_name
    WHERE tc.constraint_type = 'FOREIGN KEY'
) fk ON c.table_name = fk.table_name AND c.column_name = fk.column_name
WHERE t.table_schema = 'public'
ORDER BY t.table_name, c.ordinal_position;

-- Foreign Key ilişkileri
SELECT
    tc.table_name AS child_table,
    kcu.column_name AS child_column,
    ccu.table_name AS parent_table,
    ccu.column_name AS parent_column
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY'
ORDER BY tc.table_name;
```

### Yöntem 5: ER_DIAGRAM.md Dosyasını Kullanma

Projenizde zaten bir ER diyagramı dokümantasyonu var: `DOCUMENTATION/ER_DIAGRAM.md`

Bu dosya:
- ✅ Tüm tabloları ve ilişkileri gösterir
- ✅ ASCII art formatında görsel diyagram içerir
- ✅ Tablo detaylarını açıklar
- ✅ Normalizasyon bilgilerini içerir

**Öneri:** 
- Dokümantasyon için: `ER_DIAGRAM.md` dosyasını kullanın
- Görsel sunum için: pgAdmin ERD Tool veya DBeaver kullanın
- Sunum/rapor için: DBeaver veya dbdiagram.io ile PNG/PDF export yapın

### ER Diyagramı Kontrol Listesi

- [ ] pgAdmin ERD Tool ile diyagram oluşturuldu (veya alternatif araç)
- [ ] Tüm 7 tablo diyagramda görünüyor
- [ ] Foreign key ilişkileri doğru gösteriliyor
- [ ] Primary key'ler işaretlenmiş
- [ ] Diyagram PNG/PDF olarak kaydedildi (opsiyonel)

## 🔧 Sorun Giderme

### Problem 1: "Connection refused" Hatası

**Çözüm:**
```bash
# PostgreSQL servisinin çalıştığını kontrol et
# Windows
services.msc → PostgreSQL servisini başlat

# Linux
sudo systemctl status postgresql
sudo systemctl start postgresql
```

### Problem 2: "Database does not exist" Hatası

**Çözüm:**
```sql
-- Veritabanını oluştur
CREATE DATABASE "HomeRadar_db";
```

### Problem 3: "Permission denied" Hatası

**Çözüm:**
```sql
-- Kullanıcıya yetki ver
GRANT ALL PRIVILEGES ON DATABASE "HomeRadar_db" TO homeradar_app_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO homeradar_app_user;
```

### Problem 4: "Migration failed" Hatası

**Çözüm:**
```powershell
# Migration'ı sıfırla
Update-Database 0

# Migration'ı sil
Remove-Migration

# Veritabanını sil (DİKKAT: Tüm veriler silinir!)
# psql içinde:
DROP DATABASE "HomeRadar_db";
CREATE DATABASE "HomeRadar_db";

# Yeni migration oluştur
Add-Migration InitialCreate
Update-Database
```

### Problem 5: "Constraint violation" Hatası

**Çözüm:**
- SQL scriptlerini sırayla çalıştırdığınızdan emin olun
- Önce migration'ı çalıştırın, sonra SQL scriptlerini

### Problem 6: Türkçe Karakter Sorunu

**Çözüm:**
```sql
-- Veritabanı encoding'ini kontrol et
SELECT datname, encoding, datcollate, datctype 
FROM pg_database 
WHERE datname = 'HomeRadar_db';

-- Gerekirse veritabanını yeniden oluştur
DROP DATABASE "HomeRadar_db";
CREATE DATABASE "HomeRadar_db" 
    WITH ENCODING = 'UTF8' 
    LC_COLLATE = 'Turkish_Turkey.1254' 
    LC_CTYPE = 'Turkish_Turkey.1254';
```

## 📝 Connection String Formatı

```
Host=localhost;Port=5432;Database=HomeRadar_db;Username=homeradar_app_user;Password=HomeRadar2024!SecurePass
```

### Parametreler:
- **Host**: PostgreSQL sunucu adresi (localhost veya IP)
- **Port**: PostgreSQL port numarası (varsayılan: 5432)
- **Database**: Veritabanı adı (HomeRadar_db)
- **Username**: Kullanıcı adı (homeradar_app_user)
- **Password**: Şifre (HomeRadar2024!SecurePass)

### Güvenlik Notu:
- Production ortamında şifreleri **asla** kod içinde saklamayın
- Environment variables veya Azure Key Vault kullanın
- `.gitignore` dosyasına `App.config` ekleyin (şifreler varsa)

## 🔐 Güvenlik Ayarları

### 1. Şifre Değiştirme

```sql
-- Kullanıcı şifresini değiştir
ALTER USER homeradar_app_user WITH PASSWORD 'YeniGüvenliŞifre123!';
```

### 2. Sadece Okuma Kullanıcısı (Raporlama)

```sql
-- Read-only kullanıcı oluştur
CREATE USER homeradar_readonly WITH PASSWORD 'ReadOnlyPass2024!';
GRANT CONNECT ON DATABASE "HomeRadar_db" TO homeradar_readonly;
GRANT USAGE ON SCHEMA public TO homeradar_readonly;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO homeradar_readonly;
```

### 3. Firewall Ayarları

Production ortamında:
- Sadece gerekli IP adreslerinden erişime izin verin
- PostgreSQL'in `pg_hba.conf` dosyasını yapılandırın

## 📊 Veritabanı İstatistikleri

Kurulum sonrası kontrol:

```sql
-- Tablo sayıları
SELECT 
    'Users' AS TableName, COUNT(*) AS RowCount FROM "Users"
UNION ALL
SELECT 'Districts', COUNT(*) FROM "Districts"
UNION ALL
SELECT 'BuildingTypes', COUNT(*) FROM "BuildingTypes"
UNION ALL
SELECT 'Features', COUNT(*) FROM "Features"
UNION ALL
SELECT 'Listings', COUNT(*) FROM "Listings"
UNION ALL
SELECT 'ListingFeatures', COUNT(*) FROM "ListingFeatures"
UNION ALL
SELECT 'Predictions', COUNT(*) FROM "Predictions";

-- View sayısı
SELECT COUNT(*) AS ViewCount 
FROM information_schema.views 
WHERE table_schema = 'public';

-- Stored procedure sayısı
SELECT COUNT(*) AS ProcedureCount 
FROM information_schema.routines 
WHERE routine_schema = 'public' 
AND routine_type = 'FUNCTION';
```

## ✅ Kurulum Kontrol Listesi

- [ ] PostgreSQL kuruldu ve çalışıyor
- [ ] Veritabanı oluşturuldu (HomeRadar_db)
- [ ] Entity Framework Migration çalıştırıldı
- [ ] SQL scriptleri sırayla çalıştırıldı
- [ ] Connection string doğru yapılandırıldı
- [ ] Program.cs ile bağlantı test edildi
- [ ] Tablolar oluşturuldu (7 tablo)
- [ ] View'ler oluşturuldu (5 view)
- [ ] Stored procedure'ler oluşturuldu (2 procedure)
- [ ] User defined function'lar oluşturuldu (2 function)
- [ ] Kullanıcı yetkilendirmeleri yapıldı
- [ ] Test verileri eklendi (opsiyonel)

## 📞 Destek

Sorun yaşarsanız:
1. Bu dokümantasyonu tekrar okuyun
2. PostgreSQL log dosyalarını kontrol edin
3. Entity Framework migration log'larını kontrol edin
4. Takım lideri ile iletişime geçin

