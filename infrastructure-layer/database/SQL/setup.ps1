Write-Host "=== SmartValue Database Setup ===" -ForegroundColor Cyan

# PostgreSQL path'i ekle
$env:Path += ";C:\Program Files\PostgreSQL\15\bin"
$env:PGPASSWORD = "250400"

# SQL dosyalarını sırayla çalıştır
Write-Host "`n1. Constraints ve Indexes..." -ForegroundColor Yellow
psql -U postgres -d homeradar_db -f "01_Database_Schema.sql"

Write-Host "`n2. Views..." -ForegroundColor Yellow
psql -U postgres -d homeradar_db -f "02_Views.sql"

Write-Host "`n3. Stored Procedures..." -ForegroundColor Yellow
psql -U postgres -d homeradar_db -f "03_StoredProcedures.sql"

Write-Host "`n4. User Defined Functions..." -ForegroundColor Yellow
psql -U postgres -d homeradar_db -f "04_UserDefinedFunctions.sql"

Write-Host "`n5. User Permissions..." -ForegroundColor Yellow
psql -U postgres -d homeradar_db -f "05_UserPermissions.sql"

Write-Host "`n6. Test Verileri..." -ForegroundColor Yellow
psql -U postgres -d homeradar_db -f "06_SeedData.sql"

Write-Host "`n✅ TAMAMLANDI!" -ForegroundColor Green

# Şifreyi temizle
Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
