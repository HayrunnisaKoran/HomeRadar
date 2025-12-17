-- =============================================
-- KULLANICI YETKİLENDİRME VE MASKELEME
-- =============================================

-- 1. Uygulama Kullanıcısı Oluştur veya Şifresini Güncelle
-- Bu kullanıcı Entity Framework tarafından kullanılacak
-- NOT: Eğer kullanıcı zaten varsa şifresini günceller, yoksa oluşturur
DO $$
BEGIN
    IF EXISTS (SELECT FROM pg_user WHERE usename = 'homeradar_app_user') THEN
        -- Kullanıcı varsa şifresini güncelle
        ALTER USER homeradar_app_user WITH PASSWORD 'HomeRadar2024!SecurePass';
    ELSE
        -- Kullanıcı yoksa oluştur
        CREATE USER homeradar_app_user WITH PASSWORD 'HomeRadar2024!SecurePass';
    END IF;
END $$;

-- 2. Veritabanı Yetkileri
-- NOT: Eğer kullanıcı yoksa bu satırlar hata verebilir, bu yüzden önce kullanıcı oluşturulmalı
GRANT CONNECT ON DATABASE "HomeRadar_db" TO homeradar_app_user;
GRANT USAGE ON SCHEMA public TO homeradar_app_user;

-- 3. Tablo Yetkileri (SELECT, INSERT, UPDATE, DELETE)
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO homeradar_app_user;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO homeradar_app_user;

-- 4. View Yetkileri
GRANT SELECT ON ALL TABLES IN SCHEMA public TO homeradar_app_user;

-- 5. Function ve Procedure Yetkileri
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO homeradar_app_user;

-- 6. Gelecekte oluşturulacak nesneler için varsayılan yetkiler
ALTER DEFAULT PRIVILEGES IN SCHEMA public 
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO homeradar_app_user;

ALTER DEFAULT PRIVILEGES IN SCHEMA public 
    GRANT USAGE, SELECT ON SEQUENCES TO homeradar_app_user;

ALTER DEFAULT PRIVILEGES IN SCHEMA public 
    GRANT EXECUTE ON FUNCTIONS TO homeradar_app_user;

-- =============================================
-- 7. MASKELEME (Row Level Security - RLS)
-- =============================================

-- ÖNEMLİ: RLS kullanmak için önce etkinleştirilmesi gerekir
-- Prediction tablosu için Row Level Security etkinleştir (eğer tablo varsa)
DO $$
BEGIN
    -- Predictions tablosunun var olup olmadığını kontrol et
    IF EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'Predictions'
    ) THEN
        -- Row Level Security etkinleştir
        ALTER TABLE "Predictions" ENABLE ROW LEVEL SECURITY;
        
        -- Eğer policy zaten varsa sil
        DROP POLICY IF EXISTS prediction_user_isolation ON "Predictions";
        
        -- Policy oluştur
        -- Kullanıcılar sadece kendi tahminlerini görebilir
        -- Not: Bu policy, uygulama tarafında session variable set edildiğinde çalışır
        -- Örnek kullanım: SET app.user_id = '123'; SET app.user_role = 'User';
        CREATE POLICY prediction_user_isolation ON "Predictions"
            FOR SELECT
            USING (
                "UserId" = current_setting('app.user_id', true)::INTEGER 
                OR current_setting('app.user_role', true) = 'Admin'
            );
    END IF;
END $$;

-- Not: current_setting kullanımı için uygulama tarafında session variable set edilmesi gerekir
-- Örnek: SET app.user_id = '123'; SET app.user_role = 'User';

-- =============================================
-- 8. READ-ONLY KULLANICI (Raporlama için)
-- =============================================

-- CREATE USER homeradar_readonly WITH PASSWORD 'ReadOnlyPass2024!';
-- GRANT CONNECT ON DATABASE "HomeRadar_db" TO homeradar_readonly;
-- GRANT USAGE ON SCHEMA public TO homeradar_readonly;
-- GRANT SELECT ON ALL TABLES IN SCHEMA public TO homeradar_readonly;
-- GRANT SELECT ON ALL TABLES IN SCHEMA public TO homeradar_readonly;

