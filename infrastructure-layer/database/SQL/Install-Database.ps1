# =============================================
# SmartValue - Veritabanı Kurulum Scripti
# PowerShell ile Otomatik Kurulum
# =============================================
# 
# KULLANIM:
# 1. PowerShell'i Administrator olarak açın
# 2. Script çalıştırma izni verin (gerekirse):
#    Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
# 3. Script'i çalıştırın:
#    .\SQL\Install-Database.ps1
# =============================================

param(
    [string]$DatabaseName = "HomeRadar_db",
    [string]$DatabaseHost = "localhost",
    [int]$Port = 5432,
    [string]$Username = "postgres",
    [string]$Password = "250400",
    [switch]$SkipMigration = $false,
    [switch]$SkipSeedData = $false
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SmartValue - Veritabanı Kurulum Scripti" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# PostgreSQL bağlantı bilgilerini al
if ([string]::IsNullOrEmpty($Password)) {
    $securePassword = Read-Host "PostgreSQL şifresini girin" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    $Password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
}

# PostgreSQL psql yolunu bul
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psqlPath) {
    Write-Host "HATA: psql komutu bulunamadı!" -ForegroundColor Red
    Write-Host "PostgreSQL'in PATH'e eklendiğinden emin olun." -ForegroundColor Yellow
    Write-Host "Veya psql'in tam yolunu belirtin: C:\Program Files\PostgreSQL\15\bin\psql.exe" -ForegroundColor Yellow
    exit 1
}

$psqlExe = $psqlPath.Source
Write-Host "PostgreSQL bulundu: $psqlExe" -ForegroundColor Green

# Connection string oluştur
$env:PGPASSWORD = $Password
# Encoding sorununu önlemek için UTF8 ayarla
$env:PGCLIENTENCODING = "UTF8"

# Script dizinini belirle
$scriptDirectory = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
Write-Host "Script dizini: $scriptDirectory" -ForegroundColor Gray

# Veritabanının var olup olmadığını kontrol et
Write-Host "`nVeritabanı kontrol ediliyor..." -ForegroundColor Yellow
try {
    $dbCheck = & $psqlExe -h $DatabaseHost -p $Port -U $Username -d "postgres" -tAc "SELECT 1 FROM pg_database WHERE datname='$DatabaseName'" 2>&1
    $dbExists = ($dbCheck -match "1")
} catch {
    $dbExists = $false
}

if (-not $dbExists) {
    Write-Host "UYARI: $DatabaseName veritabanı bulunamadı!" -ForegroundColor Yellow
    $createDb = Read-Host "Veritabanını oluşturmak ister misiniz? (E/H)"
    if ($createDb -eq "E" -or $createDb -eq "e") {
        Write-Host "Veritabanı oluşturuluyor..." -ForegroundColor Yellow
        & $psqlExe -h $DatabaseHost -p $Port -U $Username -d "postgres" -c "CREATE DATABASE `"$DatabaseName`";" 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "HATA: Veritabanı oluşturulamadı!" -ForegroundColor Red
            exit 1
        }
        Write-Host "Veritabanı oluşturuldu!" -ForegroundColor Green
    } else {
        Write-Host "Kurulum iptal edildi." -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Veritabanı bulundu: $DatabaseName" -ForegroundColor Green
}

# ADIM 1: Entity Framework Migration Kontrolü
if (-not $SkipMigration) {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "ADIM 1: Entity Framework Migration Kontrolü" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    # Tabloların var olup olmadığını kontrol et
    Write-Host "Migration durumu kontrol ediliyor..." -ForegroundColor Yellow
    try {
        $tableCheck = & $psqlExe -h $DatabaseHost -p $Port -U $Username -d $DatabaseName -tAc `
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_name IN ('Users', 'Listings', 'Districts', 'BuildingTypes', 'Features');" 2>&1
        
        $tableCount = [int]($tableCheck -replace '\s+', '')
        
        if ($tableCount -ge 5) {
            Write-Host "✓ Migration tamamlanmış! ($tableCount tablo bulundu)" -ForegroundColor Green
            Write-Host "Tablolar: Users, Listings, Districts, BuildingTypes, Features ve diğerleri mevcut." -ForegroundColor Gray
        } else {
            Write-Host "✗ Migration henüz yapılmamış! (Sadece $tableCount tablo bulundu, en az 5 tablo bekleniyor)" -ForegroundColor Red
            Write-Host ""
            Write-Host "Migration yapmak için Visual Studio'da Package Manager Console'dan şu komutları çalıştırın:" -ForegroundColor Yellow
            Write-Host "  1. Add-Migration InitialCreate" -ForegroundColor White
            Write-Host "  2. Update-Database" -ForegroundColor White
            Write-Host ""
            $continue = Read-Host "Migration'ı tamamladıktan sonra devam etmek için E yazın, çıkmak için H yazın (E/H)"
            if ($continue -ne "E" -and $continue -ne "e") {
                Write-Host "Kurulum iptal edildi. Önce Migration'ı tamamlayın." -ForegroundColor Red
                exit 1
            }
            Write-Host "Migration tamamlandı, devam ediliyor..." -ForegroundColor Green
        }
    } catch {
        Write-Host "⚠ Migration kontrolü sırasında hata oluştu: $_" -ForegroundColor Yellow
        Write-Host "Manuel kontrol yapılıyor..." -ForegroundColor Yellow
        $continue = Read-Host "Migration'ı tamamladınız mı? (E/H)"
        if ($continue -ne "E" -and $continue -ne "e") {
            Write-Host "Kurulum iptal edildi. Önce Migration'ı tamamlayın." -ForegroundColor Red
            exit 1
        }
    }
} else {
    Write-Host "`nMigration adımı atlandı." -ForegroundColor Yellow
}

# ADIM 2: Constraints ve Indexes
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "ADIM 2: Constraints ve Indexes" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
$scriptPath = Join-Path $scriptDirectory "01_Database_Schema.sql"
if (Test-Path $scriptPath) {
    Write-Host "Script çalıştırılıyor: 01_Database_Schema.sql" -ForegroundColor Yellow
    $result = & $psqlExe -h $DatabaseHost -p $Port -U $Username -d $DatabaseName -f $scriptPath 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Constraints ve Indexes eklendi!" -ForegroundColor Green
    } else {
        Write-Host "✗ Hata oluştu!" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
} else {
    Write-Host "✗ Script bulunamadı: $scriptPath" -ForegroundColor Red
    Write-Host "Mevcut dizin: $scriptDirectory" -ForegroundColor Yellow
}

# ADIM 3: Views
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "ADIM 3: Views (5 adet)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
$scriptPath = Join-Path $scriptDirectory "02_Views.sql"
if (Test-Path $scriptPath) {
    Write-Host "Script çalıştırılıyor: 02_Views.sql" -ForegroundColor Yellow
    $result = & $psqlExe -h $DatabaseHost -p $Port -U $Username -d $DatabaseName -f $scriptPath 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Views oluşturuldu!" -ForegroundColor Green
    } else {
        Write-Host "✗ Hata oluştu!" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
} else {
    Write-Host "✗ Script bulunamadı: $scriptPath" -ForegroundColor Red
    Write-Host "Mevcut dizin: $scriptDirectory" -ForegroundColor Yellow
}

# ADIM 4: Stored Procedures
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "ADIM 4: Stored Procedures (2 adet)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
$scriptPath = Join-Path $scriptDirectory "03_StoredProcedures.sql"
if (Test-Path $scriptPath) {
    Write-Host "Script çalıştırılıyor: 03_StoredProcedures.sql" -ForegroundColor Yellow
    $result = & $psqlExe -h $DatabaseHost -p $Port -U $Username -d $DatabaseName -f $scriptPath 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Stored Procedures oluşturuldu!" -ForegroundColor Green
    } else {
        Write-Host "✗ Hata oluştu!" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
} else {
    Write-Host "✗ Script bulunamadı: $scriptPath" -ForegroundColor Red
    Write-Host "Mevcut dizin: $scriptDirectory" -ForegroundColor Yellow
}

# ADIM 5: User Defined Functions
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "ADIM 5: User Defined Functions (2 adet)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
$scriptPath = Join-Path $scriptDirectory "04_UserDefinedFunctions.sql"
if (Test-Path $scriptPath) {
    Write-Host "Script çalıştırılıyor: 04_UserDefinedFunctions.sql" -ForegroundColor Yellow
    $result = & $psqlExe -h $DatabaseHost -p $Port -U $Username -d $DatabaseName -f $scriptPath 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ User Defined Functions oluşturuldu!" -ForegroundColor Green
    } else {
        Write-Host "✗ Hata oluştu!" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
} else {
    Write-Host "✗ Script bulunamadı: $scriptPath" -ForegroundColor Red
    Write-Host "Mevcut dizin: $scriptDirectory" -ForegroundColor Yellow
}

# ADIM 6: User Permissions
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "ADIM 6: User Permissions ve Maskeleme" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
$scriptPath = Join-Path $scriptDirectory "05_UserPermissions.sql"
if (Test-Path $scriptPath) {
    Write-Host "Script çalıştırılıyor: 05_UserPermissions.sql" -ForegroundColor Yellow
    $result = & $psqlExe -h $DatabaseHost -p $Port -U $Username -d $DatabaseName -f $scriptPath 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Kullanıcı yetkilendirme tamamlandı!" -ForegroundColor Green
    } else {
        Write-Host "✗ Hata oluştu!" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
} else {
    Write-Host "✗ Script bulunamadı: $scriptPath" -ForegroundColor Red
    Write-Host "Mevcut dizin: $scriptDirectory" -ForegroundColor Yellow
}

# ADIM 7: Test Verileri (Opsiyonel)
if (-not $SkipSeedData) {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "ADIM 7: Test Verileri (Opsiyonel)" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    $addSeedData = Read-Host "Test verileri eklemek ister misiniz? (E/H)"
    if ($addSeedData -eq "E" -or $addSeedData -eq "e") {
        $scriptPath = Join-Path $scriptDirectory "06_SeedData.sql"
        if (Test-Path $scriptPath) {
            Write-Host "Script çalıştırılıyor: 06_SeedData.sql" -ForegroundColor Yellow
            $result = & $psqlExe -h $DatabaseHost -p $Port -U $Username -d $DatabaseName -f $scriptPath 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Test verileri eklendi!" -ForegroundColor Green
            } else {
                Write-Host "✗ Hata oluştu!" -ForegroundColor Red
                Write-Host $result -ForegroundColor Red
            }
        } else {
            Write-Host "✗ Script bulunamadı: $scriptPath" -ForegroundColor Red
            Write-Host "Mevcut dizin: $scriptDirectory" -ForegroundColor Yellow
        }
    }
}

# Özet
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "KURULUM TAMAMLANDI!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Sonraki adımlar:" -ForegroundColor Yellow
Write-Host "1. Program.cs ile bağlantıyı test edin" -ForegroundColor White
Write-Host "2. View'leri test edin: SELECT * FROM vw_district_avg_prices;" -ForegroundColor White
Write-Host "3. Stored Procedure'leri test edin: SELECT * FROM sp_get_listings_by_criteria();" -ForegroundColor White
Write-Host ""

# Şifreyi temizle
Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue

