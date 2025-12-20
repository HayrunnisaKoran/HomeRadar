# 🏠 SmartValue SOA API - Akıllı Emlak Değerleme Sistemi

**📍 Proje Kimliği:** SmartValue - Akıllı Emlak Değerleme Sistemi  
**🎯 Amaç:** Belirli bir bölgedeki emlak verilerini analiz ederek, kullanıcıya evinin tahmini değerini söyleyen ve piyasa analizi sunan web tabanlı bir sistem.  
**📅 Sürüm:** 1.0.0  
**🚀 Durum:** Geliştirme Aşamasında (SOA API Tamamlandı ✅)


### 🔥 ACİL YAPMANIZ GEREKENLER:
1. **Güncel kodu çekin:** `git checkout develop` → `git pull origin develop`
2. **SOA API'yi kurun:** `cd soa-api` → `npm install`
3. **Çalıştırın:** `npm start` veya `npm run dev`
4. **Test edin:** Tarayıcınızda `http://localhost:3000/api/health` adresine gidin




## 📋 İÇİNDEKİLER
1. [🚀 Hızlı Başlangıç](#-hızlı-başlangıç)
2. [🏗️ Proje Yapısı](#️-proje-yapısı)
3. [🔌 API Endpoint'leri](#-api-endpointleri)
4. [🤝 Takım Entegrasyonu](#-takım-entegrasyonu)
5. [👥 5 Kişilik Görev Dağılımı](#-5-kişilik-görev-dağılımı)



## 🚀 HIZLI BAŞLANGIÇ

### Gereksinimler
- **Node.js** v16 veya üzeri
- **Git**
- **npm** (Node.js ile birlikte gelir)
- **PostgreSQL** (veritabanı için)
- **Python 3.8+** (ML modeli için)

### Kurulum Adımları
```bash
# 1. Güncel kodu çekin
git checkout develop
git pull origin develop

# 2. SOA API klasörüne gidin
cd soa-api

# 3. Bağımlılıkları yükleyin (node_modules oluşacak)
npm install

# 4. Ortam değişkenlerini ayarlayın
cp .env.example .env
# .env dosyasını düzenleyin (DB bilgilerinizi girin)

# 5. API'yi başlatın
npm start
# Veya geliştirme modunda (otomatik yeniden başlatma):
npm run dev

 

## PROJE YAPISI (SOA 6 Katmanlı Mimari)

text
soa-api/
├── 📁 presentation-layer/          # 📍 SUNUM KATMANI
│   ├── controllers/
│   │   ├── HealthController.js    # Sistem sağlık kontrolü
│   │   ├── PredictionController.js # Tahmin endpoint'i
│   │   ├── MarketController.js    # Piyasa analizi
│   │   └── ExternalApiController.js # Harici API'ler
│   └── routes/
│       ├── apiRoutes.js           # Ana API rotaları
│       └── auth.js                # Kimlik doğrulama
│
├── 📁 application-layer/           # 📍 UYGULAMA KATMANI
│   └── services/
│       ├── PredictionService.js   # Tahmin iş mantığı
│       └── MarketService.js       # Piyasa analizi mantığı
│
├── 📁 domain-layer/               # 📍 DOMAIN KATMANI
│   └── models/
│       ├── Property.js           # Emlak domain modeli
│       └── Prediction.js         # Tahmin domain modeli
│
├── 📁 data-layer/                 # 📍 VERİ KATMANI
│   ├── models/                   # C# Entity Framework modelleri
│   │   ├── User.cs              # Kullanıcılar
│   │   ├── Listing.cs           # İlanlar
│   │   ├── District.cs          # İlçeler
│   │   └── BuildingType.cs      # Bina tipleri
│   └── repositories/
│       ├── PropertyRepository.js # Veri erişim sınıfı
│       └── MockPropertyRepository.js # Test için mock
│
├── 📁 infrastructure-layer/       # 📍 ALTYAPI KATMANI
│   ├── database/
│   │   ├── dbConnection.js      # PostgreSQL bağlantısı
│   │   ├── SQL/                 # SQL dosyaları
│   │   │   ├── 01_Database_Schema.sql
│   │   │   ├── 02_Views.sql
│   │   │   ├── 03_StoredProcedures.sql
│   │   │   └── 06_SeedData.sql
│   │   └── migrations/          # Entity Framework migrations
│   ├── external-apis/
│   │   └── services/
│   │       ├── googleMapsService.js # Google Maps API
│   │       ├── mlService.js     # ML API bağlantısı
│   │       └── soapClient.js    # SOAP servis istemcisi
│   ├── grpc/                    # gRPC iletişimi
│   │   ├── grpcClient.js        # ML servisine bağlantı
│   │   ├── server.js            # gRPC sunucusu
│   │   └── protos/prediction.proto # Protobuf tanımı
│   └── ml-model/                # Python ML modeli
│       ├── predict.py           # Tahmin script'i
│       ├── manisa_ev_fiyat_modeli_v1_060.pkl # Eğitilmiş model
│       └── model_columns.pkl    # Model sütunları
│
├── 📁 common/                    # 📍 ORTAK KATMAN
│   └── middleware/
│       ├── errorHandler.js      # Hata yönetimi
│       └── logger.js            # Loglama
│
├── 📄 index.js                   # Ana uygulama giriş noktası
├── 📄 app.js                     # Express.js konfigürasyonu
├── 📄 package.json               # Bağımlılıklar (NPM)
├── 📄 .env             # Ortam değişkenleri şablonu
├── 📄 .gitignore                 # Git ignore dosyası
├── 📄 test-db.js                 # Veritabanı testi
└── 📄 test-ml-api.js            # ML API testi



##🔌 API ENDPOINT'LERİ

1. 📊 TAHMİN YAPMA - PROJENİN KALBİ
GET http://localhost:3000/api/predict

2. 📈 PİYASA ANALİZİ - GÖRSEL EKSİKLİĞİ BURADA ÇÖZÜLÜYOR
GET http://localhost:3000/api/market/analysis

3. 💊 SAĞLIK KONTROLÜ - SİSTEM DURUMU
GET http://localhost:3000/api/health

4. 🗺️ DIŞ API ENTEGRASYONU - GOOGLE MAPS
GET http://localhost:3000/api/external/maps?district=Yunusemre

5. 🔐 KİMLİK DOĞRULAMA
POST http://localhost:3000/api/auth/login



## 🤝 TAKIM ENTEGRASYONU

🧠 ÜYE 2 (ML Model) ile İletişim:
Protokol: gRPC (hızlı ve binary iletişim)

Port: 50051

Protobuf Dosyası: infrastructure-layer/grpc/protos/prediction.proto

Bağlantı Testi: node infrastructure-layer/grpc/grpcClient.js

🗄️ ÜYE 1 (Veritabanı) ile İletişim:
Veritabanı: PostgreSQL

Bağlantı String: .env dosyasında

SQL Dosyaları: infrastructure-layer/database/SQL/ klasöründe

Test: node test-db.js



## 👥 5 KİŞİLİK GÖREV DAĞILIMI


👤 ÜYE 1: Veritabanı Yöneticisi & Takım Lideri (Backend - DB)
📌 Odak: Veri Tabanı Dersi İsterleri
✅ Sizden Beklenenler:
SOA API'nin infrastructure-layer/database/SQL/ klasöründeki SQL dosyalarını çalıştırın
data-layer/models/ altındaki C# modelleri veritabanınıza uyarlayın
SOA API'nin veritabanına bağlanabildiğini test edin (node test-db.js)
Diğer ekiplere veritabanı bağlantı bilgilerini sağlayın


👤 ÜYE 2: Veri Bilimcisi (Machine Learning Specialist)
📌 Odak: Makine Öğrenmesi Dersi İsterleri
✅ Sizden Beklenenler:
infrastructure-layer/ml-model/ klasöründeki Python modelini çalıştırın
SOA API ile gRPC üzerinden iletişimi test edin
Yeni verilerle modeli güncelleyin (gerekirse)
ML servisini SOA API'ye bağlayın


👤 ÜYE 3: Servis Geliştirici (Backend - API & SOA) - ✅ TAMAMLANDI!
📌 Odak: Servis Odaklı Mimari (SOA) İsterleri
🎉 Tamamlananlar:
✅ Node.js ile ana API yazıldı
✅ gRPC iletişimi kuruldu (ML servisi için)
✅ SOAP/Dış API entegrasyonu yapıldı (Google Maps)
✅ 6 Katmanlı SOA mimarisi kurgulandı
✅ Tüm endpoint'ler hazır ve test edildi


👤 ÜYE 4: Full-Stack Web Geliştirici (Logic & Controller)
📌 Odak: İleri Web Programlama - Backend Logic
✅ Sizden Beklenenler:
SOA API endpoint'lerini ASP.NET Controller'larınıza bağlayın
Kullanıcı giriş/kayıt sistemini SOA API'nin auth endpoint'lerine bağlayın
View'lerden SOA API'ye istek gönderin
SOA API'den gelen verileri View'lere aktarın


👤 ÜYE 5: Frontend Tasarımcı & Web Geliştirici (UI/UX)
📌 Odak: İleri Web Programlama - Frontend Design
✅ Sizden Beklenenler:
SOA API endpoint'lerini JavaScript/AJAX ile çağırın
API'den gelen verileri Chart.js ile görselleştirin
Responsive tasarımları API verileriyle besleyin
Kullanıcı formlarını SOA API'ye bağlayın 


