-- =============================================
-- DISTRICTS TABLOSUNU TEMİZLEYİP YENİDEN OLUŞTURMA
-- Encoding sorunlarını çözmek için tüm Districts kayıtlarını silip doğru encoding ile yeniden ekler
-- =============================================

BEGIN;

-- 1. Önce Predictions tablosundaki DistrictId referanslarını geçici olarak 1'e güncelle (Yunusemre)
-- NOT: DistrictId NOT NULL olduğu için NULL yapamıyoruz
UPDATE "Predictions" 
SET "DistrictId" = 1 
WHERE "DistrictId" IS NOT NULL;

-- 2. Listings tablosundaki DistrictId referanslarını geçici olarak 1'e güncelle (Yunusemre)
UPDATE "Listings" 
SET "DistrictId" = 1 
WHERE "DistrictId" IS NOT NULL;

-- 3. Tüm Districts kayıtlarını sil
DELETE FROM "Districts";

-- 4. ID sequence'ini sıfırla
SELECT setval(pg_get_serial_sequence('"Districts"', 'Id'), 1, false);

-- 5. Doğru encoding ile yeniden ekle (UTF-8)
INSERT INTO "Districts" ("Name", "City")
VALUES 
    ('Yunusemre', 'Manisa'),
    ('Şehzadeler', 'Manisa'),
    ('Akhisar', 'Manisa'),
    ('Salihli', 'Manisa'),
    ('Turgutlu', 'Manisa'),
    ('Soma', 'Manisa'),
    ('Alaşehir', 'Manisa'),
    ('Saruhanlı', 'Manisa'),
    ('Kula', 'Manisa'),
    ('Demirci', 'Manisa'),
    ('Ahmetli', 'Manisa'),
    ('Gördes', 'Manisa'),
    ('Kırkağaç', 'Manisa'),
    ('Köprübaşı', 'Manisa'),
    ('Sarıgöl', 'Manisa'),
    ('Selendi', 'Manisa')
ON CONFLICT DO NOTHING;

-- 6. Kontrol: Tüm Districts kayıtlarını göster
SELECT "Id", "Name", "City" 
FROM "Districts" 
ORDER BY "Name";

COMMIT;
