-- =============================================
-- SmartValue - Emlak Değerleme Sistemi
-- TÜM SQL SCRİPTLERİNİ ÇALIŞTIRAN ANA DOSYA
-- =============================================
-- 
-- KULLANIM:
-- 1. Önce Entity Framework Migration ile tabloları oluşturun
-- 2. Sonra bu dosyayı çalıştırın veya alt dosyaları sırayla çalıştırın
--
-- Sıralama:
-- 01_Database_Schema.sql (Constraints ve Indexes)
-- 02_Views.sql
-- 03_StoredProcedures.sql
-- 04_UserDefinedFunctions.sql
-- 05_UserPermissions.sql
-- 06_SeedData.sql (Test Verileri - Opsiyonel)
-- 07_Performance_Optimizations.sql (Performans İyileştirmeleri - Opsiyonel)
-- =============================================

\echo 'SmartValue Veritabanı Kurulumu Başlatılıyor...'

-- Constraints ve Indexes
\i SQL/01_Database_Schema.sql

-- Views
\i SQL/02_Views.sql

-- Stored Procedures
\i SQL/03_StoredProcedures.sql

-- User Defined Functions
\i SQL/04_UserDefinedFunctions.sql

-- User Permissions
\i SQL/05_UserPermissions.sql

-- Test Verileri (Opsiyonel - Geliştirme ve test için)
-- NOT: Production ortamında kullanmayın!
\i SQL/06_SeedData.sql

-- Performance Optimizations (Opsiyonel - İsterler zaten karşılanmış)
-- \i SQL/07_Performance_Optimizations.sql

\echo 'Kurulum Tamamlandı!'

