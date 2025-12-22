-- =============================================
-- İLÇE İSİM UNIQUE CONSTRAINT EKLEME
-- Aynı isimli ilçelerin tekrar eklenmesini engeller
-- =============================================

-- NOT: Bu script, duplicate temizliği yapıldıktan SONRA çalıştırılmalıdır!

-- Önce mevcut duplicate'leri kontrol et
SELECT 
    LOWER(TRIM("Name")) as normalizedname,
    COUNT(*) as count,
    STRING_AGG("Id"::text, ', ') as districtids
FROM "Districts"
WHERE "City" = 'Manisa'
GROUP BY LOWER(TRIM("Name"))
HAVING COUNT(*) > 1;

-- Eğer yukarıdaki sorgu sonuç döndürüyorsa, önce SQL/09_Cleanup_Duplicate_Districts.sql scriptini çalıştırın!

-- Unique constraint ekle (case-insensitive ve trim'li)
-- PostgreSQL'de case-insensitive unique constraint için özel bir yöntem kullanıyoruz

-- Önce bir function oluştur (eğer yoksa)
CREATE OR REPLACE FUNCTION normalize_district_name(name_text TEXT)
RETURNS TEXT AS $$
BEGIN
    RETURN LOWER(TRIM(name_text));
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- Unique index oluştur (normalize edilmiş isim için)
-- Bu index, case-insensitive ve trim'li karşılaştırma yapar
CREATE OR REPLACE FUNCTION normalize_district_name(name_text TEXT)
RETURNS TEXT AS $$
BEGIN
    RETURN LOWER(TRIM(name_text));
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- Alternatif olarak, trigger ile kontrol edebiliriz (daha güvenli)
-- Önce mevcut trigger'ı kontrol et ve varsa sil
DROP TRIGGER IF EXISTS trg_check_duplicate_district ON "Districts";

-- Trigger function oluştur
CREATE OR REPLACE FUNCTION check_duplicate_district()
RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM "Districts" d
        WHERE d."Id" != NEW."Id"
        AND normalize_district_name(d."Name") = normalize_district_name(NEW."Name")
        AND d."City" = NEW."City"
    ) THEN
        RAISE EXCEPTION 'Duplicate district: % already exists in city %', NEW."Name", NEW."City";
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger oluştur (INSERT ve UPDATE için)
CREATE TRIGGER trg_check_duplicate_district
BEFORE INSERT OR UPDATE ON "Districts"
FOR EACH ROW
EXECUTE FUNCTION check_duplicate_district();

-- Test: Duplicate eklemeyi dene (hata vermeli)
-- INSERT INTO "Districts" ("Name", "City") VALUES ('Akhisar', 'Manisa'); -- Bu hata vermeli

-- Kontrol
SELECT 
    "Id", 
    "Name", 
    "City",
    normalize_district_name("Name") as normalizedname
FROM "Districts"
WHERE "City" = 'Manisa'
ORDER BY "Name";

