-- =============================================
-- DISTRICTS TABLOSUNA TEMİZ VERİ EKLEME
-- Districts tablosu boş olduğunda bu script'i çalıştırın
-- =============================================

-- Doğru encoding ile Districts kayıtlarını ekle (UTF-8)
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
    ('Selendi', 'Manisa');

-- Kontrol: Tüm Districts kayıtlarını göster
SELECT "Id", "Name", "City" 
FROM "Districts" 
ORDER BY "Name";

