-- =============================================
-- SmartValue - Emlak Değerleme Sistemi
-- TEST VERİLERİ (Seed Data)
-- =============================================
-- 
-- NOT: Bu script, test ve geliştirme amaçlıdır.
-- Production ortamında kullanmayın!
-- =============================================

BEGIN;

-- =============================================
-- 1. KULLANICILAR (Users)
-- =============================================
-- Şifreler: "Admin123!" ve "User123!" (Hash'lenmiş olarak saklanmalı)
-- NOT: Gerçek uygulamada şifreler hash'lenmiş olmalıdır.
-- Burada örnek olarak plain text kullanıyoruz (güvenlik için değiştirin!)

INSERT INTO "Users" ("Email", "PasswordHash", "FirstName", "LastName", "Role", "CreatedAt", "IsActive")
VALUES 
    ('admin@smartvalue.com', 'AQAAAAEAACcQAAAAE...', 'Admin', 'User', 'Admin', NOW(), true),
    ('user1@smartvalue.com', 'AQAAAAEAACcQAAAAE...', 'Ahmet', 'Yılmaz', 'User', NOW(), true),
    ('user2@smartvalue.com', 'AQAAAAEAACcQAAAAE...', 'Ayşe', 'Demir', 'User', NOW(), true)
ON CONFLICT DO NOTHING;

-- =============================================
-- 2. İLÇELER (Districts) - Manisa İlçeleri
-- =============================================
-- NOT: Districts INSERT komutu kaldırıldı!
-- Districts tablosunu doldurmak için SQL/15_Insert_Districts_Clean.sql kullanın
-- Bu script artık Districts tablosuna veri eklemiyor (encoding sorunlarını önlemek için)
-- Eğer Districts tablosunda encoding sorunu varsa, SQL/13_Fix_Districts_Encoding_Final.sql kullanın

-- =============================================
-- 3. BİNA TİPLERİ (BuildingTypes)
-- =============================================
INSERT INTO "BuildingTypes" ("Name", "Description")
VALUES 
    ('Daire', 'Apartman dairesi'),
    ('Villa', 'Müstakil villa'),
    ('Müstakil', 'Müstakil ev'),
    ('Dubleks', 'İki katlı daire'),
    ('Tripleks', 'Üç katlı daire'),
    ('Residence', 'Lüks rezidans')
ON CONFLICT DO NOTHING;

-- =============================================
-- 4. ÖZELLİKLER (Features)
-- =============================================
INSERT INTO "Features" ("Name", "Description")
VALUES 
    ('Balkon', 'Balkonlu'),
    ('Asansör', 'Asansörlü'),
    ('Garaj', 'Garajlı'),
    ('Otopark', 'Otoparklı'),
    ('Havuz', 'Havuzlu'),
    ('Güvenlik', 'Güvenlikli'),
    ('Bahçe', 'Bahçeli'),
    ('Teras', 'Teraslı'),
    ('Manzara', 'Manzaralı'),
    ('Eşyalı', 'Eşyalı')
ON CONFLICT DO NOTHING;

-- =============================================
-- 5. İLANLAR (Listings) - Örnek Veriler
-- =============================================
-- NOT: DistrictId ve BuildingTypeId değerleri, yukarıdaki INSERT'lerden sonra
-- otomatik olarak oluşan ID'lere göre ayarlanmalıdır.
-- Bu script, ID'leri dinamik olarak alır.

DO $$
DECLARE
    v_yunusemre_id INTEGER;
    v_sehzadeler_id INTEGER;
    v_akhisar_id INTEGER;
    v_daire_id INTEGER;
    v_villa_id INTEGER;
    v_mustakil_id INTEGER;
BEGIN
    -- İlçe ID'lerini al
    SELECT "Id" INTO v_yunusemre_id FROM "Districts" WHERE "Name" = 'Yunusemre' LIMIT 1;
    SELECT "Id" INTO v_sehzadeler_id FROM "Districts" WHERE "Name" = 'Şehzadeler' LIMIT 1;
    SELECT "Id" INTO v_akhisar_id FROM "Districts" WHERE "Name" = 'Akhisar' LIMIT 1;
    
    -- Bina tipi ID'lerini al
    SELECT "Id" INTO v_daire_id FROM "BuildingTypes" WHERE "Name" = 'Daire' LIMIT 1;
    SELECT "Id" INTO v_villa_id FROM "BuildingTypes" WHERE "Name" = 'Villa' LIMIT 1;
    SELECT "Id" INTO v_mustakil_id FROM "BuildingTypes" WHERE "Name" = 'Müstakil' LIMIT 1;
    
    -- Örnek ilanlar ekle
    INSERT INTO "Listings" (
        "DistrictId", "BuildingTypeId", "Price", "SquareMeters", 
        "RoomCount", "SalonCount", "BathroomCount", "Floor", 
        "BuildingAge", "Aidat", "HeatingType", "Direction", 
        "BuildingStatus", "UsageStatus", "DeedStatus", "FurnitureStatus",
        "HasBalcony", "HasElevator", "HasGarage", "IsInComplex", 
        "HasSecurity", "IsCreditSuitable", "IsExchangeable",
        "Neighborhood", "ListingDate", "CreatedAt", "IsActive"
    ) VALUES
        -- Yunusemre - 3+1 Daireler
        (v_yunusemre_id, v_daire_id, 450000, 120, 3, 1, 1, '3. Kat', 5, 500, 'Doğalgaz', 'Güney', 'İkinci El', 'Boş', 'Kat Mülkiyetli', 'Eşyalı', true, true, false, true, true, true, false, 'Merkez', NOW() - INTERVAL '10 days', NOW(), true),
        (v_yunusemre_id, v_daire_id, 520000, 140, 3, 1, 2, '5. Kat', 3, 600, 'Doğalgaz', 'Güney', 'Sıfır', 'Boş', 'Kat Mülkiyetli', 'Boş', true, true, true, true, true, true, false, 'Merkez', NOW() - INTERVAL '8 days', NOW(), true),
        (v_yunusemre_id, v_daire_id, 380000, 100, 3, 1, 1, '2. Kat', 10, 400, 'Kombi', 'Kuzey', 'İkinci El', 'Kiracılı', 'Kat Mülkiyetli', 'Boş', true, false, false, false, false, true, true, 'Şehir Merkezi', NOW() - INTERVAL '15 days', NOW(), true),
        
        -- Yunusemre - 2+1 Daireler
        (v_yunusemre_id, v_daire_id, 320000, 85, 2, 1, 1, '1. Kat', 7, 350, 'Doğalgaz', 'Doğu', 'İkinci El', 'Boş', 'Kat Mülkiyetli', 'Eşyalı', true, true, false, true, false, true, false, 'Merkez', NOW() - INTERVAL '5 days', NOW(), true),
        (v_yunusemre_id, v_daire_id, 280000, 75, 2, 1, 1, 'Zemin', 15, 300, 'Kombi', 'Batı', 'İkinci El', 'Boş', 'Kat Mülkiyetli', 'Boş', false, false, false, false, false, true, false, 'Şehir Merkezi', NOW() - INTERVAL '12 days', NOW(), true),
        
        -- Şehzadeler - 3+1 Daireler
        (v_sehzadeler_id, v_daire_id, 480000, 130, 3, 1, 2, '4. Kat', 4, 550, 'Doğalgaz', 'Güney', 'Sıfır', 'Boş', 'Kat Mülkiyetli', 'Boş', true, true, true, true, true, true, false, 'Merkez', NOW() - INTERVAL '7 days', NOW(), true),
        (v_sehzadeler_id, v_daire_id, 410000, 115, 3, 1, 1, '6. Kat', 8, 450, 'Doğalgaz', 'Güney', 'İkinci El', 'Boş', 'Kat Mülkiyetli', 'Eşyalı', true, true, false, true, true, true, true, 'Merkez', NOW() - INTERVAL '9 days', NOW(), true),
        
        -- Şehzadeler - Villa
        (v_sehzadeler_id, v_villa_id, 850000, 200, 4, 1, 3, 'Villa', 2, 0, 'Doğalgaz', 'Güney', 'Sıfır', 'Boş', 'Müstakil', 'Boş', true, false, true, false, true, true, false, 'Villa Sitesi', NOW() - INTERVAL '3 days', NOW(), true),
        
        -- Akhisar - Müstakil
        (v_akhisar_id, v_mustakil_id, 650000, 180, 4, 1, 2, 'Müstakil', 12, 0, 'Kombi', 'Güney', 'İkinci El', 'Boş', 'Müstakil', 'Boş', true, false, true, false, false, true, true, 'Merkez', NOW() - INTERVAL '6 days', NOW(), true),
        (v_akhisar_id, v_mustakil_id, 720000, 220, 5, 1, 3, 'Müstakil', 5, 0, 'Doğalgaz', 'Güney', 'İkinci El', 'Boş', 'Müstakil', 'Eşyalı', true, false, true, false, true, true, false, 'Merkez', NOW() - INTERVAL '4 days', NOW(), true);
END $$;

-- =============================================
-- 6. İLAN-ÖZELLİK İLİŞKİLERİ (ListingFeatures)
-- =============================================
-- Bazı ilanlara özellik ekle
DO $$
DECLARE
    v_listing_id INTEGER;
    v_balkon_id INTEGER;
    v_asansor_id INTEGER;
    v_garaj_id INTEGER;
    v_guvenlik_id INTEGER;
BEGIN
    -- Özellik ID'lerini al
    SELECT "Id" INTO v_balkon_id FROM "Features" WHERE "Name" = 'Balkon' LIMIT 1;
    SELECT "Id" INTO v_asansor_id FROM "Features" WHERE "Name" = 'Asansör' LIMIT 1;
    SELECT "Id" INTO v_garaj_id FROM "Features" WHERE "Name" = 'Garaj' LIMIT 1;
    SELECT "Id" INTO v_guvenlik_id FROM "Features" WHERE "Name" = 'Güvenlik' LIMIT 1;
    
    -- İlk 5 ilana özellik ekle
    FOR v_listing_id IN SELECT "Id" FROM "Listings" ORDER BY "Id" LIMIT 5
    LOOP
        -- Her ilana balkon ekle
        INSERT INTO "ListingFeatures" ("ListingId", "FeatureId")
        VALUES (v_listing_id, v_balkon_id)
        ON CONFLICT DO NOTHING;
        
        -- İlk 3 ilana asansör ekle
        IF v_listing_id <= (SELECT MIN("Id") + 2 FROM "Listings") THEN
            INSERT INTO "ListingFeatures" ("ListingId", "FeatureId")
            VALUES (v_listing_id, v_asansor_id)
            ON CONFLICT DO NOTHING;
        END IF;
        
        -- İlk 2 ilana garaj ekle
        IF v_listing_id <= (SELECT MIN("Id") + 1 FROM "Listings") THEN
            INSERT INTO "ListingFeatures" ("ListingId", "FeatureId")
            VALUES (v_listing_id, v_garaj_id)
            ON CONFLICT DO NOTHING;
        END IF;
        
        -- İlk 3 ilana güvenlik ekle
        IF v_listing_id <= (SELECT MIN("Id") + 2 FROM "Listings") THEN
            INSERT INTO "ListingFeatures" ("ListingId", "FeatureId")
            VALUES (v_listing_id, v_guvenlik_id)
            ON CONFLICT DO NOTHING;
        END IF;
    END LOOP;
END $$;

-- =============================================
-- 7. ÖRNEK TAHMİNLER (Predictions)
-- =============================================
-- Kullanıcıların yaptığı örnek tahminler
DO $$
DECLARE
    v_user1_id INTEGER;
    v_user2_id INTEGER;
    v_yunusemre_id INTEGER;
    v_sehzadeler_id INTEGER;
    v_daire_id INTEGER;
BEGIN
    -- Kullanıcı ve ilçe ID'lerini al
    SELECT "Id" INTO v_user1_id FROM "Users" WHERE "Email" = 'user1@smartvalue.com' LIMIT 1;
    SELECT "Id" INTO v_user2_id FROM "Users" WHERE "Email" = 'user2@smartvalue.com' LIMIT 1;
    SELECT "Id" INTO v_yunusemre_id FROM "Districts" WHERE "Name" = 'Yunusemre' LIMIT 1;
    SELECT "Id" INTO v_sehzadeler_id FROM "Districts" WHERE "Name" = 'Şehzadeler' LIMIT 1;
    SELECT "Id" INTO v_daire_id FROM "BuildingTypes" WHERE "Name" = 'Daire' LIMIT 1;
    
    -- Örnek tahminler ekle
    INSERT INTO "Predictions" (
        "UserId", "DistrictId", "RoomCount", "SquareMeters", 
        "BuildingAge", "BuildingTypeId", 
        "PredictedPriceMin", "PredictedPriceMax", "PredictedPriceAvg",
        "ModelName", "ConfidenceScore", "CreatedAt"
    ) VALUES
        (v_user1_id, v_yunusemre_id, 3, 120, 5, v_daire_id, 420000, 480000, 450000, 'LinearRegression', 85.5, NOW() - INTERVAL '5 days'),
        (v_user1_id, v_yunusemre_id, 2, 85, 7, v_daire_id, 300000, 340000, 320000, 'DecisionTree', 82.3, NOW() - INTERVAL '3 days'),
        (v_user2_id, v_sehzadeler_id, 3, 130, 4, v_daire_id, 460000, 500000, 480000, 'LinearRegression', 88.1, NOW() - INTERVAL '2 days'),
        (v_user2_id, v_yunusemre_id, 4, 150, 3, v_daire_id, 550000, 600000, 575000, 'DecisionTree', 79.8, NOW() - INTERVAL '1 day');
END $$;

COMMIT;

-- =============================================
-- VERİ KONTROLÜ
-- =============================================
SELECT 'Kullanıcılar: ' || COUNT(*) FROM "Users";
SELECT 'İlçeler: ' || COUNT(*) FROM "Districts";
SELECT 'Bina Tipleri: ' || COUNT(*) FROM "BuildingTypes";
SELECT 'Özellikler: ' || COUNT(*) FROM "Features";
SELECT 'İlanlar: ' || COUNT(*) FROM "Listings";
SELECT 'İlan-Özellik İlişkileri: ' || COUNT(*) FROM "ListingFeatures";
SELECT 'Tahminler: ' || COUNT(*) FROM "Predictions";

\echo 'Test verileri başarıyla eklendi!'

