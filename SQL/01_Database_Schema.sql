-- =============================================
-- SmartValue - Emlak Değerleme Sistemi
-- Veritabanı Şema Oluşturma Scripti
-- =============================================

-- Veritabanı oluşturma (Eğer yoksa)
-- CREATE DATABASE HomeRadar_db;

-- =============================================
-- 1. TABLOLAR (CREATE TABLE)
-- =============================================
-- NOT: Tablolar zaten varsa hata vermez (IF NOT EXISTS kullanılmıştır)
-- Foreign key bağımlılıklarına göre sıralanmıştır

-- BuildingTypes tablosu (Foreign key bağımlılığı yok)
CREATE TABLE IF NOT EXISTS "BuildingTypes" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(50) NOT NULL,
    "Description" VARCHAR(200) NULL
);

-- Districts tablosu (Foreign key bağımlılığı yok)
CREATE TABLE IF NOT EXISTS "Districts" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "City" VARCHAR(50) NULL
);

-- Features tablosu (Foreign key bağımlılığı yok)
CREATE TABLE IF NOT EXISTS "Features" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(50) NOT NULL,
    "Description" VARCHAR(200) NULL
);

-- Users tablosu (Foreign key bağımlılığı yok)
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Email" VARCHAR(100) NOT NULL,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "FirstName" VARCHAR(50) NOT NULL,
    "LastName" VARCHAR(50) NOT NULL,
    "Role" VARCHAR(20) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "IsActive" BOOLEAN NOT NULL
);

-- Listings tablosu (Districts ve BuildingTypes'a bağımlı)
CREATE TABLE IF NOT EXISTS "Listings" (
    "Id" SERIAL PRIMARY KEY,
    "DistrictId" INTEGER NOT NULL,
    "BuildingTypeId" INTEGER NOT NULL,
    "Price" DECIMAL(18,2) NOT NULL,
    "SquareMeters" DECIMAL(10,2) NOT NULL,
    "RoomCount" INTEGER NOT NULL,
    "SalonCount" INTEGER NOT NULL,
    "BathroomCount" INTEGER NULL,
    "Floor" VARCHAR(50) NULL,
    "BuildingAge" INTEGER NOT NULL,
    "Aidat" DECIMAL(10,2) NULL,
    "HeatingType" VARCHAR(50) NULL,
    "Direction" VARCHAR(50) NULL,
    "BuildingStatus" VARCHAR(50) NULL,
    "UsageStatus" VARCHAR(50) NULL,
    "DeedStatus" VARCHAR(50) NULL,
    "FurnitureStatus" VARCHAR(50) NULL,
    "HasBalcony" BOOLEAN NULL,
    "HasElevator" BOOLEAN NULL,
    "HasGarage" BOOLEAN NULL,
    "IsInComplex" BOOLEAN NULL,
    "HasSecurity" BOOLEAN NULL,
    "IsCreditSuitable" BOOLEAN NULL,
    "IsExchangeable" BOOLEAN NULL,
    "Neighborhood" VARCHAR(200) NULL,
    "ListingDate" TIMESTAMP NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "IsActive" BOOLEAN NOT NULL,
    CONSTRAINT "FK_Listings_Districts_DistrictId" 
        FOREIGN KEY ("DistrictId") REFERENCES "Districts"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Listings_BuildingTypes_BuildingTypeId" 
        FOREIGN KEY ("BuildingTypeId") REFERENCES "BuildingTypes"("Id") ON DELETE RESTRICT
);

-- ListingFeatures tablosu (Listings ve Features'a bağımlı)
CREATE TABLE IF NOT EXISTS "ListingFeatures" (
    "Id" SERIAL PRIMARY KEY,
    "ListingId" INTEGER NOT NULL,
    "FeatureId" INTEGER NOT NULL,
    CONSTRAINT "FK_ListingFeatures_Listings_ListingId" 
        FOREIGN KEY ("ListingId") REFERENCES "Listings"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ListingFeatures_Features_FeatureId" 
        FOREIGN KEY ("FeatureId") REFERENCES "Features"("Id") ON DELETE RESTRICT
);

-- Predictions tablosu (Users, Listings, Districts, BuildingTypes'a bağımlı)
CREATE TABLE IF NOT EXISTS "Predictions" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NULL,
    "ListingId" INTEGER NULL,
    "DistrictId" INTEGER NOT NULL,
    "RoomCount" INTEGER NOT NULL,
    "SquareMeters" DECIMAL(10,2) NOT NULL,
    "BuildingAge" INTEGER NOT NULL,
    "BuildingTypeId" INTEGER NULL,
    "PredictedPriceMin" DECIMAL(18,2) NOT NULL,
    "PredictedPriceMax" DECIMAL(18,2) NOT NULL,
    "PredictedPriceAvg" DECIMAL(18,2) NOT NULL,
    "ModelName" VARCHAR(50) NULL,
    "ConfidenceScore" DECIMAL(5,2) NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    CONSTRAINT "FK_Predictions_Users_UserId" 
        FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Predictions_Listings_ListingId" 
        FOREIGN KEY ("ListingId") REFERENCES "Listings"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Predictions_Districts_DistrictId" 
        FOREIGN KEY ("DistrictId") REFERENCES "Districts"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Predictions_BuildingTypes_BuildingTypeId" 
        FOREIGN KEY ("BuildingTypeId") REFERENCES "BuildingTypes"("Id") ON DELETE SET NULL
);

-- =============================================
-- 1.1. UNIQUE INDEXES (Migration'dan)
-- =============================================

-- BuildingTypes.Name unique index
CREATE UNIQUE INDEX IF NOT EXISTS "IX_BuildingTypes_Name" ON "BuildingTypes"("Name");

-- Districts.Name index (unique değil)
CREATE INDEX IF NOT EXISTS "IX_Districts_Name" ON "Districts"("Name");

-- Features.Name unique index
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Features_Name" ON "Features"("Name");

-- Users.Email unique index
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users"("Email");

-- ListingFeatures composite unique index
CREATE UNIQUE INDEX IF NOT EXISTS "IX_ListingFeatures_ListingId_FeatureId" 
    ON "ListingFeatures"("ListingId", "FeatureId");

-- ListingFeatures.FeatureId index
CREATE INDEX IF NOT EXISTS "IX_ListingFeatures_FeatureId" ON "ListingFeatures"("FeatureId");

-- Listings foreign key indexes
CREATE INDEX IF NOT EXISTS "IX_Listings_BuildingTypeId" ON "Listings"("BuildingTypeId");
CREATE INDEX IF NOT EXISTS "IX_Listings_DistrictId" ON "Listings"("DistrictId");

-- Predictions foreign key indexes
CREATE INDEX IF NOT EXISTS "IX_Predictions_BuildingTypeId" ON "Predictions"("BuildingTypeId");
CREATE INDEX IF NOT EXISTS "IX_Predictions_DistrictId" ON "Predictions"("DistrictId");
CREATE INDEX IF NOT EXISTS "IX_Predictions_ListingId" ON "Predictions"("ListingId");
CREATE INDEX IF NOT EXISTS "IX_Predictions_UserId" ON "Predictions"("UserId");

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

