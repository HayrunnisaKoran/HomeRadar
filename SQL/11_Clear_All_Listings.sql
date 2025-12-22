-- =============================================
-- TÜM İLANLARI TEMİZLEME SCRIPTİ
-- DİKKAT: Bu script tüm ilanları kalıcı olarak siler!
-- =============================================

-- ÖNEMLİ: Bu script'i çalıştırmadan önce yedek alın!

-- Önce bağlı tablolardaki referansları kontrol et
SELECT COUNT(*) as ListingFeaturesCount 
FROM "ListingFeatures";

SELECT COUNT(*) as PredictionsWithListingCount 
FROM "Predictions" 
WHERE "ListingId" IS NOT NULL;

-- ListingFeatures tablosunu temizle (CASCADE DELETE ile otomatik silinir ama emin olmak için)
DELETE FROM "ListingFeatures";

-- Predictions tablosundaki ListingId referanslarını NULL yap
UPDATE "Predictions" 
SET "ListingId" = NULL 
WHERE "ListingId" IS NOT NULL;

-- Tüm ilanları sil
DELETE FROM "Listings";

-- Kontrol: Toplam ilan sayısı 0 olmalı
SELECT COUNT(*) as RemainingListingsCount 
FROM "Listings";

-- ID sequence'ı sıfırla (opsiyonel - yeni ID'ler 1'den başlasın)
ALTER SEQUENCE "Listings_Id_seq" RESTART WITH 1;

-- Sonuç mesajı
SELECT 'Tüm ilanlar başarıyla silindi!' as Result;

