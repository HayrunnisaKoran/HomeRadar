# SmartValue - Connection String Rehberi

## 🔐 Connection String Formatı

### PostgreSQL Connection String (Npgsql)

```
Host=localhost;Port=5432;Database=HomeRadar_db;Username=homeradar_app_user;Password=HomeRadar2024!SecurePass
```

### Parametreler

| Parametre | Açıklama | Örnek Değer |
|-----------|----------|-------------|
| `Host` | PostgreSQL sunucu adresi | `localhost` veya `192.168.1.100` |
| `Port` | PostgreSQL port numarası | `5432` (varsayılan) |
| `Database` | Veritabanı adı | `HomeRadar_db` |
| `Username` | Kullanıcı adı | `homeradar_app_user` |
| `Password` | Kullanıcı şifresi | `HomeRadar2024!SecurePass` |

---

## 📝 Farklı Ortamlar İçin Connection String'ler

### 1. Development (Geliştirme)

**App.config:**
```xml
<connectionStrings>
  <add name="HomeRadarConnection" 
       connectionString="Host=localhost;Port=5432;Database=HomeRadar_db;Username=homeradar_app_user;Password=HomeRadar2024!SecurePass" 
       providerName="Npgsql.EntityFrameworkCore.PostgreSQL" />
</connectionStrings>
```

### 2. Migration (Entity Framework)

**EmlakContextFactory.cs:**
```csharp
// Migration için postgres kullanıcısı kullanılır (superuser yetkisi gerekli)
connectionString = "Host=localhost;Port=5432;Database=HomeRadar_db;Username=postgres;Password=250400";
```

**Not**: Migration sırasında `postgres` kullanıcısı kullanılır çünkü:
- Tablo oluşturma yetkisi gerekir
- Kullanıcı oluşturma yetkisi gerekir
- Schema değişiklikleri yapma yetkisi gerekir

### 3. Production (Üretim)

**Environment Variables (Önerilen):**
```bash
# .env dosyası
DB_HOST=production-server.example.com
DB_PORT=5432
DB_NAME=HomeRadar_db
DB_USER=homeradar_app_user
DB_PASSWORD=GüvenliŞifre123!
```

**appsettings.json (ASP.NET Core):**
```json
{
  "ConnectionStrings": {
    "HomeRadarConnection": "Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"
  }
}
```

---

## 🔧 Farklı Diller İçin Connection String Örnekleri

### C# (.NET Framework - App.config)

```xml
<connectionStrings>
  <add name="HomeRadarConnection" 
       connectionString="Host=localhost;Port=5432;Database=HomeRadar_db;Username=homeradar_app_user;Password=HomeRadar2024!SecurePass" 
       providerName="Npgsql.EntityFrameworkCore.PostgreSQL" />
</connectionStrings>
```

**Kod İçinde Kullanım:**
```csharp
using System.Configuration;

string connectionString = ConfigurationManager.ConnectionStrings["HomeRadarConnection"]?.ConnectionString;
```

### C# (ASP.NET Core - appsettings.json)

```json
{
  "ConnectionStrings": {
    "HomeRadarConnection": "Host=localhost;Port=5432;Database=HomeRadar_db;Username=homeradar_app_user;Password=HomeRadar2024!SecurePass"
  }
}
```

**Kod İçinde Kullanım:**
```csharp
using Microsoft.Extensions.Configuration;

string connectionString = Configuration.GetConnectionString("HomeRadarConnection");
```

### Python (psycopg2)

```python
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

**veya Connection String Formatında:**
```python
connection_string = "host=localhost port=5432 dbname=HomeRadar_db user=homeradar_app_user password=HomeRadar2024!SecurePass"
conn = psycopg2.connect(connection_string)
```

### Node.js (pg)

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
```

**veya Connection String Formatında:**
```javascript
const connectionString = 'postgresql://homeradar_app_user:HomeRadar2024!SecurePass@localhost:5432/HomeRadar_db';
const pool = new Pool({ connectionString });
```

### Java (JDBC)

```java
String connectionString = "jdbc:postgresql://localhost:5432/HomeRadar_db?user=homeradar_app_user&password=HomeRadar2024!SecurePass";
Connection conn = DriverManager.getConnection(connectionString);
```

---

## 🔒 Güvenlik Best Practices

### 1. Şifreleri Kod İçinde Saklamayın

❌ **YANLIŞ:**
```csharp
string connectionString = "Host=localhost;Password=MyPassword123!";
```

✅ **DOĞRU:**
```csharp
string connectionString = Configuration.GetConnectionString("HomeRadarConnection");
```

### 2. Environment Variables Kullanın

**Windows:**
```powershell
# PowerShell
$env:DB_PASSWORD = "GüvenliŞifre123!"
```

**Linux/macOS:**
```bash
# .env dosyası
export DB_PASSWORD="GüvenliŞifre123!"
```

### 3. Azure Key Vault (Production)

```csharp
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var client = new SecretClient(
    new Uri("https://your-keyvault.vault.azure.net/"),
    new DefaultAzureCredential());

KeyVaultSecret secret = await client.GetSecretAsync("HomeRadarConnectionString");
string connectionString = secret.Value;
```

### 4. .gitignore Dosyasına Ekleyin

```
# Connection strings
App.config
appsettings.json
appsettings.Development.json
.env
*.env
```

---

## 🧪 Connection String Testi

### C# ile Test

```csharp
using Npgsql;

string connectionString = "Host=localhost;Port=5432;Database=HomeRadar_db;Username=homeradar_app_user;Password=HomeRadar2024!SecurePass";

try
{
    using (var conn = new NpgsqlConnection(connectionString))
    {
        conn.Open();
        Console.WriteLine("✓ Bağlantı başarılı!");
        
        // Basit sorgu testi
        using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\"", conn))
        {
            var count = cmd.ExecuteScalar();
            Console.WriteLine($"Kullanıcı sayısı: {count}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Bağlantı hatası: {ex.Message}");
}
```

### Python ile Test

```python
import psycopg2

try:
    conn = psycopg2.connect(
        host='localhost',
        port=5432,
        database='HomeRadar_db',
        user='homeradar_app_user',
        password='HomeRadar2024!SecurePass'
    )
    print("✓ Bağlantı başarılı!")
    
    cur = conn.cursor()
    cur.execute('SELECT COUNT(*) FROM "Users"')
    count = cur.fetchone()[0]
    print(f"Kullanıcı sayısı: {count}")
    
    cur.close()
    conn.close()
except Exception as e:
    print(f"✗ Bağlantı hatası: {e}")
```

### Node.js ile Test

```javascript
const { Pool } = require('pg');

const pool = new Pool({
  host: 'localhost',
  port: 5432,
  database: 'HomeRadar_db',
  user: 'homeradar_app_user',
  password: 'HomeRadar2024!SecurePass',
});

pool.query('SELECT COUNT(*) FROM "Users"')
  .then(result => {
    console.log('✓ Bağlantı başarılı!');
    console.log(`Kullanıcı sayısı: ${result.rows[0].count}`);
    pool.end();
  })
  .catch(err => {
    console.error('✗ Bağlantı hatası:', err);
    pool.end();
  });
```

---

## 🔄 Connection String Değiştirme

### Kullanıcı Şifresini Değiştirme

```sql
-- PostgreSQL'de
ALTER USER homeradar_app_user WITH PASSWORD 'YeniGüvenliŞifre123!';
```

**Sonra App.config'i güncelleyin:**
```xml
<connectionStrings>
  <add name="HomeRadarConnection" 
       connectionString="Host=localhost;Port=5432;Database=HomeRadar_db;Username=homeradar_app_user;Password=YeniGüvenliŞifre123!" 
       providerName="Npgsql.EntityFrameworkCore.PostgreSQL" />
</connectionStrings>
```

### Veritabanı Adını Değiştirme

```sql
-- Yeni veritabanı oluştur
CREATE DATABASE "HomeRadar_db_new";

-- Verileri kopyala (pg_dump kullanarak)
-- Windows PowerShell
pg_dump -U postgres -d HomeRadar_db | psql -U postgres -d HomeRadar_db_new

-- Linux/macOS
pg_dump -U postgres HomeRadar_db | psql -U postgres HomeRadar_db_new
```

---

## 📋 Connection String Kontrol Listesi

- [ ] Connection string doğru formatta
- [ ] Host adresi doğru (localhost veya IP)
- [ ] Port numarası doğru (5432)
- [ ] Veritabanı adı doğru (HomeRadar_db)
- [ ] Kullanıcı adı doğru (homeradar_app_user)
- [ ] Şifre doğru ve güvenli
- [ ] PostgreSQL servisi çalışıyor
- [ ] Firewall ayarları doğru
- [ ] Kullanıcı yetkileri doğru
- [ ] Connection string test edildi

---

## 🚨 Yaygın Hatalar ve Çözümleri

### Hata 1: "Connection refused"

**Sebep**: PostgreSQL servisi çalışmıyor veya yanlış port

**Çözüm:**
```bash
# Windows
services.msc → PostgreSQL servisini başlat

# Linux
sudo systemctl start postgresql

# Port kontrolü
netstat -an | grep 5432
```

### Hata 2: "Password authentication failed"

**Sebep**: Yanlış şifre veya kullanıcı adı

**Çözüm:**
```sql
-- Şifreyi sıfırla
ALTER USER homeradar_app_user WITH PASSWORD 'YeniŞifre123!';
```

### Hata 3: "Database does not exist"

**Sebep**: Veritabanı oluşturulmamış

**Çözüm:**
```sql
CREATE DATABASE "HomeRadar_db";
```

### Hata 4: "Permission denied"

**Sebep**: Kullanıcı yetkileri yetersiz

**Çözüm:**
```sql
GRANT ALL PRIVILEGES ON DATABASE "HomeRadar_db" TO homeradar_app_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO homeradar_app_user;
```

---

## 📞 Destek

Sorun yaşarsanız:
1. Connection string formatını kontrol edin
2. PostgreSQL log dosyalarını kontrol edin
3. Kullanıcı yetkilerini kontrol edin
4. Takım lideri ile iletişime geçin

---

**Son Güncelleme**: 2024-12-15

