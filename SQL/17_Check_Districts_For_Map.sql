-- =============================================
-- HARİTA İÇİN İLÇE İSİMLERİNİ KONTROL ETME
-- Veritabanındaki ilçe isimlerini ve ilan sayılarını gösterir
-- =============================================

-- 1. Tüm ilçeleri ve ilan sayılarını göster
SELECT 
    d."Id",
    d."Name",
    d."City",
    COUNT(l."Id") as "ListingCount",
    AVG(l."Price") as "AveragePrice"
FROM "Districts" d
LEFT JOIN "Listings" l ON d."Id" = l."DistrictId" AND l."IsActive" = true
GROUP BY d."Id", d."Name", d."City"
ORDER BY "ListingCount" DESC, d."Name";

-- 2. Sadece ilanı olan ilçeleri göster
SELECT 
    d."Name",
    COUNT(l."Id") as "ListingCount",
    AVG(l."Price") as "AveragePrice"
FROM "Districts" d
INNER JOIN "Listings" l ON d."Id" = l."DistrictId" AND l."IsActive" = true
GROUP BY d."Name"
ORDER BY "ListingCount" DESC;

-- 3. İlçe isimlerindeki encoding sorunlarını kontrol et
SELECT 
    "Id",
    "Name",
    LENGTH("Name") as "NameLength",
    "City"
FROM "Districts"
ORDER BY "Name";

