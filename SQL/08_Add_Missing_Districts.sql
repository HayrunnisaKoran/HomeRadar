-- =============================================
-- EKSİK İLÇELERİ EKLEME SCRIPTİ
-- CSV import'tan sonra eksik kalan ilçeler için
-- =============================================

-- CSV'de bulunan ama veritabanında olmayan ilçeler:
-- Ahmetli, Gördes (Gordes), Kırkağaç (Kirkagac), Köprübaşı (Koprubasi), Sarıgöl (Sarigol), Selendi

-- Normalize edilmiş isim kontrolü ile ekle (duplicate önleme)
INSERT INTO "Districts" ("Name", "City")
SELECT "Name", "City"
FROM (VALUES 
    ('Ahmetli', 'Manisa'),
    ('Gördes', 'Manisa'),
    ('Kırkağaç', 'Manisa'),
    ('Köprübaşı', 'Manisa'),
    ('Sarıgöl', 'Manisa'),
    ('Selendi', 'Manisa')
) AS v("Name", "City")
WHERE NOT EXISTS (
    SELECT 1 FROM "Districts" d 
    WHERE LOWER(TRIM(d."Name")) = LOWER(TRIM(v."Name")) 
    AND d."City" = v."City"
);

-- Kontrol
SELECT "Id", "Name", "City" FROM "Districts" ORDER BY "Name";

