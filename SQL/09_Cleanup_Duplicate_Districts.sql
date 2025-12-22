-- =============================================
-- DUPLICATE İLÇELERİ TEMİZLEME SCRIPTİ
-- Aynı ilçenin farklı yazımlarını birleştirir ve duplicate kayıtları temizler
-- =============================================

-- Önce mevcut duplicate'leri görelim
SELECT 
    "Id", 
    "Name", 
    "City",
    COUNT(*) OVER (PARTITION BY LOWER(TRIM("Name"))) as duplicatecount
FROM "Districts"
ORDER BY LOWER(TRIM("Name")), "Id";

-- =============================================
-- ADIM 1: Listings tablosundaki referansları en düşük ID'li ilçeye yönlendir
-- =============================================

-- Duplicate ilçeler için mapping tablosu oluştur (en düşük ID'yi tut)
WITH DuplicateMapping AS (
    SELECT 
        "Id",
        LOWER(TRIM("Name")) as normalizedname,
        MIN("Id") OVER (PARTITION BY LOWER(TRIM("Name"))) as minid
    FROM "Districts"
    WHERE "City" = 'Manisa'
),
DistrictsToUpdate AS (
    SELECT 
        dm."Id" as olddistrictid,
        dm.minid as newdistrictid
    FROM DuplicateMapping dm
    WHERE dm."Id" != dm.minid
)
UPDATE "Listings" l
SET "DistrictId" = dtu.newdistrictid
FROM DistrictsToUpdate dtu
WHERE l."DistrictId" = dtu.olddistrictid;

-- =============================================
-- ADIM 2: Predictions tablosundaki referansları güncelle
-- =============================================

WITH DuplicateMapping AS (
    SELECT 
        "Id",
        LOWER(TRIM("Name")) as normalizedname,
        MIN("Id") OVER (PARTITION BY LOWER(TRIM("Name"))) as minid
    FROM "Districts"
    WHERE "City" = 'Manisa'
),
DistrictsToUpdate AS (
    SELECT 
        dm."Id" as olddistrictid,
        dm.minid as newdistrictid
    FROM DuplicateMapping dm
    WHERE dm."Id" != dm.minid
)
UPDATE "Predictions" p
SET "DistrictId" = dtu.newdistrictid
FROM DistrictsToUpdate dtu
WHERE p."DistrictId" = dtu.olddistrictid;

-- =============================================
-- ADIM 3: Duplicate ilçeleri sil (en düşük ID'li olanı koru)
-- =============================================

DELETE FROM "Districts"
WHERE "Id" IN (
    SELECT "Id"
    FROM (
        SELECT 
            "Id",
            ROW_NUMBER() OVER (PARTITION BY LOWER(TRIM("Name")) ORDER BY "Id") as rn
        FROM "Districts"
        WHERE "City" = 'Manisa'
    ) sub
    WHERE sub.rn > 1
);

-- =============================================
-- ADIM 4: Temizlik sonrası kontrol
-- =============================================

SELECT 
    "Id", 
    "Name", 
    "City"
FROM "Districts"
WHERE "City" = 'Manisa'
ORDER BY "Name";

-- Duplicate kontrolü
SELECT 
    LOWER(TRIM("Name")) as normalizedname,
    COUNT(*) as count
FROM "Districts"
WHERE "City" = 'Manisa'
GROUP BY LOWER(TRIM("Name"))
HAVING COUNT(*) > 1;

