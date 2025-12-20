-- =============================================
-- PERFORMANS İYİLEŞTİRMELERİ (Opsiyonel)
-- =============================================
-- Bu dosya, proje isterlerini aşan performans iyileştirmeleri içerir.
-- İsterler zaten karşılanmış durumda, bu dosya ek performans için.

-- =============================================
-- 1. COMPOSITE INDEX'LER (Sık Kullanılan Sorgular İçin)
-- =============================================

-- İlçe ve Fiyat kombinasyonu (Filtreleme sorguları için)
-- Örnek: "Yunusemre'de 300.000-500.000 TL arası evler"
CREATE INDEX IF NOT EXISTS idx_listings_district_price 
ON "Listings"("DistrictId", "Price") 
WHERE "IsActive" = true;

-- İlçe, Oda Sayısı ve Aktif Durumu (Arama sorguları için)
-- Örnek: "Yunusemre'de aktif 3+1 evler"
CREATE INDEX IF NOT EXISTS idx_listings_district_room_active 
ON "Listings"("DistrictId", "RoomCount", "IsActive") 
WHERE "IsActive" = true;

-- Fiyat ve Metrekare kombinasyonu (Sıralama sorguları için)
-- Örnek: "Fiyata göre sıralı, metrekareye göre filtreli"
CREATE INDEX IF NOT EXISTS idx_listings_price_squaremeters 
ON "Listings"("Price", "SquareMeters") 
WHERE "IsActive" = true;

-- Bina Tipi ve Bina Yaşı (Analiz sorguları için)
CREATE INDEX IF NOT EXISTS idx_listings_buildingtype_age 
ON "Listings"("BuildingTypeId", "BuildingAge") 
WHERE "IsActive" = true;

-- Tahmin sorguları için (District + Room + SquareMeters)
CREATE INDEX IF NOT EXISTS idx_predictions_district_room_sqm 
ON "Predictions"("DistrictId", "RoomCount", "SquareMeters");

-- =============================================
-- 2. MATERIALIZED VIEW (Sık Sorgulanan View'ler İçin)
-- =============================================

-- İlçe bazlı ortalama fiyatlar için materialized view
-- Bu view sık sorgulanacak ve güncelleme sıklığı düşük olduğu için materialized view ideal
CREATE MATERIALIZED VIEW IF NOT EXISTS mv_district_avg_prices AS
SELECT 
    d."Id" AS DistrictId,
    d."Name" AS DistrictName,
    d."City",
    COUNT(l."Id") AS ListingCount,
    AVG(l."Price") AS AvgPrice,
    MIN(l."Price") AS MinPrice,
    MAX(l."Price") AS MaxPrice,
    AVG(l."SquareMeters") AS AvgSquareMeters,
    AVG(l."RoomCount") AS AvgRoomCount,
    NOW() AS LastRefreshed
FROM "Districts" d
LEFT JOIN "Listings" l ON d."Id" = l."DistrictId" AND l."IsActive" = true
GROUP BY d."Id", d."Name", d."City";

-- Materialized view için index
CREATE UNIQUE INDEX IF NOT EXISTS idx_mv_district_avg_prices_district 
ON mv_district_avg_prices("DistrictId");

-- Materialized view'i yenileme fonksiyonu
CREATE OR REPLACE FUNCTION refresh_district_avg_prices()
RETURNS void AS $$
BEGIN
    REFRESH MATERIALIZED VIEW CONCURRENTLY mv_district_avg_prices;
END;
$$ LANGUAGE plpgsql;

-- NOT: Materialized view'i yenilemek için:
-- SELECT refresh_district_avg_prices();
-- Veya cron job ile otomatik yenileme yapılabilir

-- =============================================
-- 3. FULL-TEXT SEARCH (İlan Arama İçin)
-- =============================================

-- Neighborhood ve diğer metin alanları için full-text search index
-- Önce text search için kolon ekle (eğer yoksa)
-- NOT: Bu kolon zaten var, sadece index ekliyoruz

-- Neighborhood için GIN index (full-text search için)
CREATE INDEX IF NOT EXISTS idx_listings_neighborhood_gin 
ON "Listings" USING gin(to_tsvector('turkish', COALESCE("Neighborhood", '')));

-- Full-text search fonksiyonu
CREATE OR REPLACE FUNCTION search_listings_by_text(
    p_search_text TEXT,
    p_district_id INTEGER DEFAULT NULL,
    p_limit INTEGER DEFAULT 50
)
RETURNS TABLE (
    ListingId INTEGER,
    Price DECIMAL,
    SquareMeters DECIMAL,
    RoomCount INTEGER,
    DistrictName VARCHAR,
    Neighborhood TEXT,
    Relevance REAL
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        l."Id" AS ListingId,
        l."Price",
        l."SquareMeters",
        l."RoomCount",
        d."Name" AS DistrictName,
        l."Neighborhood",
        ts_rank(to_tsvector('turkish', COALESCE(l."Neighborhood", '')), 
                plainto_tsquery('turkish', p_search_text)) AS Relevance
    FROM "Listings" l
    INNER JOIN "Districts" d ON l."DistrictId" = d."Id"
    WHERE l."IsActive" = true
        AND (p_district_id IS NULL OR l."DistrictId" = p_district_id)
        AND (
            to_tsvector('turkish', COALESCE(l."Neighborhood", '')) 
            @@ plainto_tsquery('turkish', p_search_text)
        )
    ORDER BY Relevance DESC, l."ListingDate" DESC
    LIMIT p_limit;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- 4. EK PERFORMANS İYİLEŞTİRMELERİ
-- =============================================

-- Partial index: Sadece aktif ilanlar için (WHERE clause ile)
-- Bu index, IsActive = false olan kayıtları index'ten hariç tutar
CREATE INDEX IF NOT EXISTS idx_listings_active_only 
ON "Listings"("DistrictId", "Price", "RoomCount") 
WHERE "IsActive" = true;

-- Covering index: Sık kullanılan kolonları içeren index
-- Bu index, sorgunun sadece index'ten çalışmasını sağlar (table scan yapmaz)
CREATE INDEX IF NOT EXISTS idx_listings_covering 
ON "Listings"("DistrictId", "Price", "SquareMeters", "RoomCount", "BuildingAge") 
WHERE "IsActive" = true;

-- =============================================
-- 5. İSTATİSTİK GÜNCELLEME
-- =============================================

-- PostgreSQL'in query planner'ı için istatistikleri güncelle
-- Bu, daha iyi sorgu planları oluşturulmasını sağlar
ANALYZE "Listings";
ANALYZE "Predictions";
ANALYZE "Districts";
ANALYZE "Users";

-- =============================================
-- KULLANIM NOTLARI
-- =============================================

-- 1. Composite Index Kullanımı:
--    SELECT * FROM "Listings" 
--    WHERE "DistrictId" = 1 AND "Price" BETWEEN 300000 AND 500000;
--    (idx_listings_district_price kullanılır)

-- 2. Materialized View Yenileme:
--    SELECT refresh_district_avg_prices();
--    Veya: REFRESH MATERIALIZED VIEW mv_district_avg_prices;

-- 3. Full-Text Search Kullanımı:
--    SELECT * FROM search_listings_by_text('Merkez', 1, 20);
--    (Neighborhood'da "Merkez" geçen ilanları getirir)

-- 4. İstatistik Güncelleme (Periyodik):
--    ANALYZE "Listings";
--    (Haftalık veya aylık çalıştırılabilir)

