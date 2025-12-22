-- =============================================
-- DISTRICTS TABLOSUNDAKİ ENCODING HATALARINI DÜZELTME SCRIPTİ
-- Veritabanı yönetim aracınızda (pgAdmin, DBeaver, vb.) bu komutları çalıştırın
-- =============================================

-- Direkt UPDATE komutları (basit ve çalışır)
UPDATE "Districts" SET "Name" = 'Şehzadeler' WHERE "Id" = 2;
UPDATE "Districts" SET "Name" = 'Alaşehir' WHERE "Id" = 7;
UPDATE "Districts" SET "Name" = 'Saruhanlı' WHERE "Id" = 8;

-- Kontrol: Düzeltilmiş kayıtları göster
SELECT "Id", "Name", "City" 
FROM "Districts" 
WHERE "Id" IN (2, 7, 8)
ORDER BY "Id";

-- Tüm Districts kayıtlarını göster (final kontrol)
SELECT "Id", "Name", "City" 
FROM "Districts" 
ORDER BY "Name";
