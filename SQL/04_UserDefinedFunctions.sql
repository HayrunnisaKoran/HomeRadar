-- =============================================
-- KULLANICI TANIMLI FONKSİYONLAR (En az 2 adet)
-- =============================================

-- 1. Function: Metrekare Başına Fiyat Hesapla
CREATE OR REPLACE FUNCTION fn_calculate_price_per_square_meter(
    p_price DECIMAL,
    p_square_meters DECIMAL
)
RETURNS DECIMAL AS $$
DECLARE
    v_result DECIMAL;
BEGIN
    IF p_square_meters IS NULL OR p_square_meters <= 0 THEN
        RETURN NULL;
    END IF;
    
    v_result := p_price / p_square_meters;
    RETURN ROUND(v_result, 2);
END;
$$ LANGUAGE plpgsql;

-- 2. Function: İlçe İçin Ortalama Fiyat Tahmini (Basit Formül)
CREATE OR REPLACE FUNCTION fn_estimate_price_by_district(
    p_district_id INTEGER,
    p_room_count INTEGER,
    p_square_meters DECIMAL,
    p_building_age INTEGER
)
RETURNS DECIMAL AS $$
DECLARE
    v_avg_price_per_sqm DECIMAL;
    v_age_factor DECIMAL;
    v_room_factor DECIMAL;
    v_estimated_price DECIMAL;
BEGIN
    -- İlçedeki ortalama metrekare başına fiyatı al
    SELECT AVG(fn_calculate_price_per_square_meter(l."Price", l."SquareMeters"))
    INTO v_avg_price_per_sqm
    FROM "Listings" l
    WHERE l."DistrictId" = p_district_id 
        AND l."IsActive" = true
        AND l."SquareMeters" > 0;
    
    -- Eğer veri yoksa NULL döndür
    IF v_avg_price_per_sqm IS NULL THEN
        RETURN NULL;
    END IF;
    
    -- Yaş faktörü (yaş arttıkça fiyat düşer)
    IF p_building_age = 0 THEN
        v_age_factor := 1.0;
    ELSIF p_building_age <= 5 THEN
        v_age_factor := 0.95;
    ELSIF p_building_age <= 10 THEN
        v_age_factor := 0.90;
    ELSIF p_building_age <= 20 THEN
        v_age_factor := 0.85;
    ELSE
        v_age_factor := 0.75;
    END IF;
    
    -- Oda sayısı faktörü (oda sayısı arttıkça metrekare başına fiyat düşer)
    IF p_room_count = 1 THEN
        v_room_factor := 1.1;
    ELSIF p_room_count = 2 THEN
        v_room_factor := 1.0;
    ELSIF p_room_count = 3 THEN
        v_room_factor := 0.95;
    ELSIF p_room_count >= 4 THEN
        v_room_factor := 0.90;
    ELSE
        v_room_factor := 1.0;
    END IF;
    
    -- Tahmini fiyatı hesapla
    v_estimated_price := v_avg_price_per_sqm * p_square_meters * v_age_factor * v_room_factor;
    
    RETURN ROUND(v_estimated_price, 2);
END;
$$ LANGUAGE plpgsql;

