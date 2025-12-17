-- =============================================
-- SmartValue - Emlak Değerleme Sistemi
-- Veritabanı Şema Oluşturma Scripti
-- =============================================

-- Veritabanı oluşturma (Eğer yoksa)
-- CREATE DATABASE HomeRadar_db;

-- =============================================
-- 1. TABLOLAR (Entity Framework Migration ile oluşturulacak)
-- =============================================
-- Tablolar Entity Framework Code First yaklaşımı ile oluşturulacak
-- Bu dosya sadece referans amaçlıdır

-- =============================================
-- 2. CONSTRAINTS (Veri Bütünlüğü)
-- =============================================

-- User tablosu için Role constraint (Trigger ile)
CREATE OR REPLACE FUNCTION check_user_role()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW."Role" NOT IN ('Admin', 'User') THEN
        RAISE EXCEPTION 'Role must be either Admin or User';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_check_user_role
    BEFORE INSERT OR UPDATE ON "Users"
    FOR EACH ROW
    EXECUTE FUNCTION check_user_role();

-- Listing tablosu için Price constraint
ALTER TABLE "Listings" 
ADD CONSTRAINT CK_Listing_Price CHECK ("Price" > 0);

-- Listing tablosu için SquareMeters constraint
ALTER TABLE "Listings" 
ADD CONSTRAINT CK_Listing_SquareMeters CHECK ("SquareMeters" > 0);

-- Listing tablosu için RoomCount constraint
ALTER TABLE "Listings" 
ADD CONSTRAINT CK_Listing_RoomCount CHECK ("RoomCount" > 0);

-- Listing tablosu için BuildingAge constraint
ALTER TABLE "Listings" 
ADD CONSTRAINT CK_Listing_BuildingAge CHECK ("BuildingAge" >= 0);

-- Prediction tablosu için Price range constraint
ALTER TABLE "Predictions" 
ADD CONSTRAINT CK_Prediction_PriceRange CHECK ("PredictedPriceMin" > 0 AND "PredictedPriceMax" >= "PredictedPriceMin");

-- Prediction tablosu için ConfidenceScore constraint
ALTER TABLE "Predictions" 
ADD CONSTRAINT CK_Prediction_ConfidenceScore CHECK ("ConfidenceScore" IS NULL OR ("ConfidenceScore" >= 0 AND "ConfidenceScore" <= 100));

-- =============================================
-- 3. INDEXES (Performans Optimizasyonu)
-- =============================================

-- Listing tablosu için performans indexleri
CREATE INDEX IF NOT EXISTS idx_listings_district ON "Listings"("DistrictId");
CREATE INDEX IF NOT EXISTS idx_listings_buildingtype ON "Listings"("BuildingTypeId");
CREATE INDEX IF NOT EXISTS idx_listings_price ON "Listings"("Price");
CREATE INDEX IF NOT EXISTS idx_listings_squaremeters ON "Listings"("SquareMeters");
CREATE INDEX IF NOT EXISTS idx_listings_roomcount ON "Listings"("RoomCount");
CREATE INDEX IF NOT EXISTS idx_listings_listingdate ON "Listings"("ListingDate");

-- Prediction tablosu için performans indexleri
CREATE INDEX IF NOT EXISTS idx_predictions_user ON "Predictions"("UserId");
CREATE INDEX IF NOT EXISTS idx_predictions_district ON "Predictions"("DistrictId");
CREATE INDEX IF NOT EXISTS idx_predictions_createdat ON "Predictions"("CreatedAt");

-- Users tablosu için email index (zaten unique constraint var)
-- CREATE INDEX IF NOT EXISTS idx_users_email ON "Users"("Email");

-- =============================================
-- NOT: Ek performans iyileştirmeleri için
-- SQL/07_Performance_Optimizations.sql dosyasına bakın
-- (Composite index'ler, Materialized view, Full-text search)
-- =============================================

