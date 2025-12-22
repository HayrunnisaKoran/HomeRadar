-- =============================================
-- İLÇE İSİMLERİNİ DÜZELTME SCRIPTİ
-- View'da görünen encoding sorunlarını düzeltmek için
-- =============================================

-- Mevcut ilçe isimlerini kontrol et
SELECT "Id", "Name", "City" 
FROM "Districts" 
ORDER BY "Id";

-- İlçe isimlerini doğru Türkçe karakterlerle güncelle
-- (Görselde görülen hatalı isimleri düzelt)

-- Id 2: Sehzadeler -> Şehzadeler
UPDATE "Districts" 
SET "Name" = 'Şehzadeler' 
WHERE "Id" = 2 AND "Name" != 'Şehzadeler';

-- Id 7: Alasehir -> Alaşehir
UPDATE "Districts" 
SET "Name" = 'Alaşehir' 
WHERE "Id" = 7 AND "Name" != 'Alaşehir';

-- Id 8: Saruhanli -> Saruhanlı
UPDATE "Districts" 
SET "Name" = 'Saruhanlı' 
WHERE "Id" = 8 AND "Name" != 'Saruhanlı';

-- Id 22: Gordes -> Gördes
UPDATE "Districts" 
SET "Name" = 'Gördes' 
WHERE "Id" = 22 AND "Name" != 'Gördes';

-- Id 23: Kırkagac -> Kırkağaç
UPDATE "Districts" 
SET "Name" = 'Kırkağaç' 
WHERE "Id" = 23 AND "Name" != 'Kırkağaç';

-- Id 24: Koprubasi -> Köprübaşı
UPDATE "Districts" 
SET "Name" = 'Köprübaşı' 
WHERE "Id" = 24 AND "Name" != 'Köprübaşı';

-- Id 25: Sarıgol -> Sarıgöl
UPDATE "Districts" 
SET "Name" = 'Sarıgöl' 
WHERE "Id" = 25 AND "Name" != 'Sarıgöl';

-- Kontrol: Düzeltilmiş kayıtları göster
SELECT "Id", "Name", "City" 
FROM "Districts" 
ORDER BY "Name";

