-- =============================================
-- DISTRICTS TABLOSUNDAKİ ENCODING HATALARINI DÜZELTME SCRIPTİ
-- Bu script, encoding hatası olan district kayıtlarını düzeltir
-- =============================================

-- Önce mevcut hatalı kayıtları göster
SELECT "Id", "Name", "City" 
FROM "Districts" 
ORDER BY "Id";

-- Encoding hatalı kayıtları ID'ye göre düzelt (görüntüden görüldüğü üzere)
-- Id 2: Å?ehzadeler -> Şehzadeler
UPDATE "Districts" 
SET "Name" = 'Şehzadeler' 
WHERE "Id" = 2;

-- Id 7: AlaÅYehir -> Alaşehir
UPDATE "Districts" 
SET "Name" = 'Alaşehir' 
WHERE "Id" = 7;

-- Id 8: SaruhanlÄ± -> Saruhanlı
UPDATE "Districts" 
SET "Name" = 'Saruhanlı' 
WHERE "Id" = 8;

-- Kontrol: Düzeltilmiş kayıtları göster
SELECT "Id", "Name", "City" 
FROM "Districts" 
ORDER BY "Name";
