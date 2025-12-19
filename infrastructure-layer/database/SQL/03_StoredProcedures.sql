-- =============================================
-- STORED PROCEDURES (En az 2 adet)
-- =============================================

-- 1. Stored Procedure: İlçe ve Oda Sayısına Göre İlanları Getir
CREATE OR REPLACE FUNCTION sp_get_listings_by_criteria(
    p_district_id INTEGER DEFAULT NULL,
    p_room_count INTEGER DEFAULT NULL,
    p_min_price DECIMAL DEFAULT NULL,
    p_max_price DECIMAL DEFAULT NULL,
    p_min_square_meters DECIMAL DEFAULT NULL,
    p_max_square_meters DECIMAL DEFAULT NULL
)
RETURNS TABLE (
    ListingId INTEGER,
    Price DECIMAL,
    SquareMeters DECIMAL,
    RoomCount INTEGER,
    SalonCount INTEGER,
    BuildingAge INTEGER,
    DistrictName VARCHAR,
    BuildingTypeName VARCHAR,
    ListingDate TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        l."Id" AS ListingId,
        l."Price",
        l."SquareMeters",
        l."RoomCount",
        l."SalonCount",
        l."BuildingAge",
        d."Name" AS DistrictName,
        bt."Name" AS BuildingTypeName,
        l."ListingDate"
    FROM "Listings" l
    INNER JOIN "Districts" d ON l."DistrictId" = d."Id"
    INNER JOIN "BuildingTypes" bt ON l."BuildingTypeId" = bt."Id"
    WHERE l."IsActive" = true
        AND (p_district_id IS NULL OR l."DistrictId" = p_district_id)
        AND (p_room_count IS NULL OR l."RoomCount" = p_room_count)
        AND (p_min_price IS NULL OR l."Price" >= p_min_price)
        AND (p_max_price IS NULL OR l."Price" <= p_max_price)
        AND (p_min_square_meters IS NULL OR l."SquareMeters" >= p_min_square_meters)
        AND (p_max_square_meters IS NULL OR l."SquareMeters" <= p_max_square_meters)
    ORDER BY l."ListingDate" DESC;
END;
$$ LANGUAGE plpgsql;

-- 2. Stored Procedure: Yeni Tahmin Kaydı Ekle ve İstatistikleri Güncelle
CREATE OR REPLACE FUNCTION sp_insert_prediction(
    p_user_id INTEGER,
    p_district_id INTEGER,
    p_room_count INTEGER,
    p_square_meters DECIMAL,
    p_building_age INTEGER,
    p_building_type_id INTEGER,
    p_predicted_price_min DECIMAL,
    p_predicted_price_max DECIMAL,
    p_model_name VARCHAR,
    p_confidence_score DECIMAL
)
RETURNS INTEGER AS $$
DECLARE
    v_prediction_id INTEGER;
    v_predicted_price_avg DECIMAL;
BEGIN
    -- Ortalama fiyatı hesapla
    v_predicted_price_avg := (p_predicted_price_min + p_predicted_price_max) / 2;
    
    -- Tahmin kaydını ekle
    INSERT INTO "Predictions" (
        "UserId",
        "DistrictId",
        "RoomCount",
        "SquareMeters",
        "BuildingAge",
        "BuildingTypeId",
        "PredictedPriceMin",
        "PredictedPriceMax",
        "PredictedPriceAvg",
        "ModelName",
        "ConfidenceScore",
        "CreatedAt"
    ) VALUES (
        p_user_id,
        p_district_id,
        p_room_count,
        p_square_meters,
        p_building_age,
        p_building_type_id,
        p_predicted_price_min,
        p_predicted_price_max,
        v_predicted_price_avg,
        p_model_name,
        p_confidence_score,
        NOW()
    )
    RETURNING "Id" INTO v_prediction_id;
    
    RETURN v_prediction_id;
END;
$$ LANGUAGE plpgsql;

