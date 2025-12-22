-- =============================================
-- VIEW'LER (En az 5 adet)
-- =============================================

-- 1. View: İlçe Bazında Ortalama Fiyatlar
CREATE OR REPLACE VIEW vw_district_avg_prices AS
SELECT 
    d."Id" AS DistrictId,
    d."Name" AS DistrictName,
    d."City",
    COUNT(l."Id") AS ListingCount,
    AVG(l."Price") AS AvgPrice,
    MIN(l."Price") AS MinPrice,
    MAX(l."Price") AS MaxPrice,
    AVG(l."SquareMeters") AS AvgSquareMeters,
    AVG(l."RoomCount") AS AvgRoomCount
FROM "Districts" d
LEFT JOIN "Listings" l ON d."Id" = l."DistrictId" AND l."IsActive" = true
GROUP BY d."Id", d."Name", d."City";

-- 2. View: Oda Sayısına Göre Fiyat İstatistikleri
CREATE OR REPLACE VIEW vw_room_count_statistics AS
SELECT 
    l."RoomCount",
    l."SalonCount",
    COUNT(*) AS TotalListings,
    AVG(l."Price") AS AvgPrice,
    AVG(l."SquareMeters") AS AvgSquareMeters,
    AVG(l."BuildingAge") AS AvgBuildingAge,
    MIN(l."Price") AS MinPrice,
    MAX(l."Price") AS MaxPrice
FROM "Listings" l
WHERE l."IsActive" = true
GROUP BY l."RoomCount", l."SalonCount"
ORDER BY l."RoomCount", l."SalonCount";

-- 3. View: Kullanıcı Tahmin Geçmişi (Detaylı)
CREATE OR REPLACE VIEW vw_user_predictions AS
SELECT 
    p."Id" AS PredictionId,
    p."CreatedAt" AS PredictionDate,
    u."Id" AS UserId,
    u."Email" AS UserEmail,
    u."FirstName" || ' ' || u."LastName" AS UserFullName,
    d."Name" AS DistrictName,
    p."RoomCount",
    p."SquareMeters",
    p."BuildingAge",
    p."PredictedPriceMin",
    p."PredictedPriceMax",
    p."PredictedPriceAvg",
    p."ModelName",
    p."ConfidenceScore"
FROM "Predictions" p
LEFT JOIN "Users" u ON p."UserId" = u."Id"
LEFT JOIN "Districts" d ON p."DistrictId" = d."Id"
ORDER BY p."CreatedAt" DESC;

-- 4. View: Aktif İlanlar Detaylı Bilgi
CREATE OR REPLACE VIEW vw_active_listings_detail AS
SELECT 
    l."Id" AS ListingId,
    l."Price",
    l."SquareMeters",
    l."RoomCount" || '+' || l."SalonCount" AS RoomInfo,
    l."BuildingAge",
    l."Floor",
    l."HeatingType",
    l."Direction",
    l."BuildingStatus",
    l."UsageStatus",
    l."DeedStatus",
    l."FurnitureStatus",
    l."HasBalcony",
    l."HasElevator",
    l."HasGarage",
    l."IsInComplex",
    l."HasSecurity",
    l."Aidat",
    l."ListingDate",
    d."Name" AS DistrictName,
    d."City",
    bt."Name" AS BuildingTypeName,
    l."Neighborhood"
FROM "Listings" l
INNER JOIN "Districts" d ON l."DistrictId" = d."Id"
INNER JOIN "BuildingTypes" bt ON l."BuildingTypeId" = bt."Id"
WHERE l."IsActive" = true
ORDER BY l."ListingDate" DESC;

-- 5. View: Bina Yaşına Göre Fiyat Analizi
CREATE OR REPLACE VIEW vw_building_age_price_analysis AS
SELECT 
    CASE 
        WHEN l."BuildingAge" = 0 THEN 'Sıfır'
        WHEN l."BuildingAge" BETWEEN 1 AND 5 THEN '1-5 Yıl'
        WHEN l."BuildingAge" BETWEEN 6 AND 10 THEN '6-10 Yıl'
        WHEN l."BuildingAge" BETWEEN 11 AND 20 THEN '11-20 Yıl'
        WHEN l."BuildingAge" BETWEEN 21 AND 30 THEN '21-30 Yıl'
        ELSE '30+ Yıl'
    END AS AgeGroup,
    COUNT(*) AS ListingCount,
    AVG(l."Price") AS AvgPrice,
    AVG(l."Price" / NULLIF(l."SquareMeters", 0)) AS AvgPricePerSquareMeter,
    MIN(l."Price") AS MinPrice,
    MAX(l."Price") AS MaxPrice,
    AVG(l."SquareMeters") AS AvgSquareMeters
FROM "Listings" l
WHERE l."IsActive" = true
GROUP BY 
    CASE 
        WHEN l."BuildingAge" = 0 THEN 'Sıfır'
        WHEN l."BuildingAge" BETWEEN 1 AND 5 THEN '1-5 Yıl'
        WHEN l."BuildingAge" BETWEEN 6 AND 10 THEN '6-10 Yıl'
        WHEN l."BuildingAge" BETWEEN 11 AND 20 THEN '11-20 Yıl'
        WHEN l."BuildingAge" BETWEEN 21 AND 30 THEN '21-30 Yıl'
        ELSE '30+ Yıl'
    END;
