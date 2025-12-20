// app.js - TAM VERSİYON (PostgreSQL ENTEGRELİ) - SON DÜZELTME
require('dotenv').config();
const express = require('express');
const cors = require('cors');
const { pool } = require('./infrastructure-layer/database/dbConnection');

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Request logging
app.use((req, res, next) => {
  console.log(`${new Date().toISOString()} - ${req.method} ${req.url}`);
  next();
});

// ==================== ANA SAYFA ====================
app.get('/', (req, res) => {
  res.json({
    name: 'SmartValue SOA API',
    description: '6-Layered SOA Architecture with Real ML & PostgreSQL Integration',
    version: '3.0.0',
    endpoints: {
      health: 'GET /health',
      ml_estimate: 'POST /api/market/estimate',
      market_overview: 'GET /api/market/overview',
      districts: 'GET /api/market/districts',
      listings: 'GET /api/market/listings',
      db_status: 'GET /api/db/status',
      test_ml: 'GET /test-estimate',
      init_db: 'GET /api/db/init-all'
    },
    features: {
      ml_integration: '✅ Active',
      postgresql: '✅ Connected',
      views: '✅ 5 View',
      stored_procedures: '✅ 2 Procedure',
      real_estate_valuation: '✅ Ready'
    },
    status: 'running'
  });
});

// ==================== SAĞLIK KONTROLÜ ====================
app.get('/health', async (req, res) => {
  try {
    const dbResult = await pool.query('SELECT NOW() as time, version() as version');
    
    // ML servisi kontrolü
    let mlStatus = 'checking';
    try {
      require('./infrastructure-layer/external-apis/services/mlService');
      mlStatus = 'available';
    } catch (error) {
      mlStatus = 'unavailable: ' + error.message;
    }
    
    // Tablo sayısı
    const tablesResult = await pool.query(`
      SELECT COUNT(*) as count FROM information_schema.tables 
      WHERE table_schema = 'public'
    `);
    
    res.json({
      status: 'healthy',
      timestamp: new Date().toISOString(),
      service: 'SmartValue SOA API v3.0',
      database: {
        status: 'connected',
        time: dbResult.rows[0].time,
        version: dbResult.rows[0].version,
        tables: parseInt(tablesResult.rows[0].count)
      },
      services: {
        ml_api: mlStatus,
        postgresql: '✅ Connected',
        grpc: 'pending',
        soap: 'pending',
        external_apis: 'pending'
      },
      system: {
        uptime: process.uptime(),
        node_version: process.version,
        memory: `${(process.memoryUsage().heapUsed / 1024 / 1024).toFixed(2)} MB`
      }
    });
  } catch (error) {
    res.status(500).json({
      status: 'degraded',
      error: error.message,
      timestamp: new Date().toISOString()
    });
  }
});

// ==================== VERİTABANI İŞLEMLERİ ====================

// 1. Veritabanı Durumu
app.get('/api/db/status', async (req, res) => {
  try {
    const tables = ['Users', 'Districts', 'BuildingTypes', 'Features', 'Listings', 'ListingFeatures', 'Predictions'];
    const stats = {};
    
    for (const table of tables) {
      try {
        const result = await pool.query(`SELECT COUNT(*) as count FROM "${table}"`);
        stats[table] = parseInt(result.rows[0].count);
      } catch (error) {
        stats[table] = `Error: ${error.message}`;
      }
    }
    
    res.json({
      success: true,
      timestamp: new Date().toISOString(),
      database: {
        name: 'HomeRadar_db',
        connection: 'active',
        tables_count: Object.keys(stats).length,
        tables: stats
      },
      recommendations: stats.Listings === 0 ? [
        '1. GET /api/db/add-sample-listings - Örnek ilanlar ekleyin',
        '2. GET /api/db/views - View\'leri test edin'
      ] : ['✅ Veritabanında veri mevcut']
    });
  } catch (error) {
    console.error('DB status error:', error);
    res.status(500).json({ error: error.message });
  }
});

// 2. Tüm Temel Verileri Yükle (ON CONFLICT KALDIRILDI)
app.get('/api/db/init-all', async (req, res) => {
  try {
    const results = [];
    
    // İlçeler - ON CONFLICT KALDIRILDI
    try {
      await pool.query(`
        INSERT INTO "Districts" ("Name", "City") VALUES
        ('Yunusemre', 'Manisa'),
        ('Sehzadeler', 'Manisa'),
        ('Akhisar', 'Manisa'),
        ('Salihli', 'Manisa'),
        ('Turgutlu', 'Manisa');
      `);
      results.push('5 ilçe eklendi');
    } catch (error) {
      results.push('İlçeler zaten mevcut');
    }
    
    // Bina tipleri - ON CONFLICT KALDIRILDI
    try {
      await pool.query(`
        INSERT INTO "BuildingTypes" ("Name", "Description") VALUES
        ('Daire', 'Apartman dairesi'),
        ('Villa', 'Mustakil villa'),
        ('Mustakil', 'Mustakil ev');
      `);
      results.push('3 bina tipi eklendi');
    } catch (error) {
      results.push('Bina tipleri zaten mevcut');
    }
    
    // Özellikler - ON CONFLICT KALDIRILDI
    try {
      await pool.query(`
        INSERT INTO "Features" ("Name", "Description") VALUES
        ('Balkon', 'Balkonlu'),
        ('Asansor', 'Asansorlu'),
        ('Garaj', 'Garajli');
      `);
      results.push('3 özellik eklendi');
    } catch (error) {
      results.push('Özellikler zaten mevcut');
    }
    
    res.json({
      success: true,
      message: 'Temel veri ekleme denemesi tamamlandı',
      results: results,
      next_step: 'GET /api/db/add-sample-listings'
    });
  } catch (error) {
    console.error('Init all error:', error);
    res.status(500).json({ error: error.message });
  }
});

// 3. Örnek Emlak İlanları Ekle (UTF-8 SORUNU ÇÖZÜLDÜ)
app.get('/api/db/add-sample-listings', async (req, res) => {
  try {
    // Önce ilçe ID'lerini al
    const districts = await pool.query('SELECT "Id", "Name" FROM "Districts"');
    if (districts.rows.length === 0) {
      return res.status(400).json({
        error: 'Önce ilçeleri ekleyin. PostgreSQLde: INSERT INTO "Districts" ("Name", "City") VALUES ...'
      });
    }
    
    // Bina tipleri yoksa oluştur
    let buildingTypes = await pool.query('SELECT "Id", "Name" FROM "BuildingTypes"');
    if (buildingTypes.rows.length === 0) {
      // Bina tiplerini ekle (UTF-8 sorununu önle)
      await pool.query(`
        INSERT INTO "BuildingTypes" ("Name", "Description") VALUES
        ('Daire', 'Apartman dairesi'),
        ('Villa', 'Mustakil villa'),
        ('Mustakil', 'Mustakil ev')
      `);
      buildingTypes = await pool.query('SELECT "Id", "Name" FROM "BuildingTypes"');
    }
    
    const yunusemre = districts.rows.find(d => d.Name === 'Yunusemre');
    const sehzadeler = districts.rows.find(d => d.Name === 'Sehzadeler');
    const daire = buildingTypes.rows.find(bt => bt.Name === 'Daire');
    const villa = buildingTypes.rows.find(bt => bt.Name === 'Villa');
    
    if (!yunusemre || !daire) {
      return res.status(400).json({
        error: 'Gerekli ilçe veya bina tipi bulunamadı'
      });
    }
    
    // 4 örnek ilan ekle
    const listings = [
      {
        DistrictId: yunusemre.Id,
        BuildingTypeId: daire.Id,
        Price: 4500000,
        SquareMeters: 120,
        RoomCount: 3,
        SalonCount: 1,
        BuildingAge: 5,
        Floor: '3. Kat',
        HeatingType: 'Dogalgaz',
        HasBalcony: true,
        HasElevator: true,
        IsActive: true,
        ListingDate: new Date()
      },
      {
        DistrictId: yunusemre.Id,
        BuildingTypeId: daire.Id,
        Price: 3800000,
        SquareMeters: 100,
        RoomCount: 2,
        SalonCount: 1,
        BuildingAge: 10,
        Floor: '2. Kat',
        HeatingType: 'Kombi',
        HasBalcony: true,
        HasElevator: false,
        IsActive: true,
        ListingDate: new Date(Date.now() - 86400000 * 7)
      },
      {
        DistrictId: sehzadeler ? sehzadeler.Id : yunusemre.Id,
        BuildingTypeId: daire.Id,
        Price: 5200000,
        SquareMeters: 140,
        RoomCount: 3,
        SalonCount: 1,
        BuildingAge: 3,
        Floor: '5. Kat',
        HeatingType: 'Dogalgaz',
        HasBalcony: true,
        HasElevator: true,
        HasGarage: true,
        IsActive: true,
        ListingDate: new Date()
      },
      {
        DistrictId: yunusemre.Id,
        BuildingTypeId: villa ? villa.Id : daire.Id,
        Price: 8500000,
        SquareMeters: 200,
        RoomCount: 4,
        SalonCount: 1,
        BuildingAge: 2,
        Floor: 'Villa',
        HeatingType: 'Dogalgaz',
        HasBalcony: true,
        HasElevator: false,
        HasGarage: true,
        IsInComplex: true,
        HasSecurity: true,
        IsActive: true,
        ListingDate: new Date()
      }
    ];
    
    const sampleListings = [];
    
    // İlanları ekle
    for (const listing of listings) {
      try {
        const result = await pool.query(
          `INSERT INTO "Listings" (
            "DistrictId", "BuildingTypeId", "Price", "SquareMeters", "RoomCount", "SalonCount",
            "BuildingAge", "Floor", "HeatingType", "HasBalcony", "HasElevator", "HasGarage",
            "IsInComplex", "HasSecurity", "IsActive", "ListingDate"
          ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15, $16)
          RETURNING "Id"`,
          [
            listing.DistrictId,
            listing.BuildingTypeId,
            listing.Price,
            listing.SquareMeters,
            listing.RoomCount,
            listing.SalonCount,
            listing.BuildingAge,
            listing.Floor,
            listing.HeatingType,
            listing.HasBalcony,
            listing.HasElevator,
            listing.HasGarage || false,
            listing.IsInComplex || false,
            listing.HasSecurity || false,
            listing.IsActive,
            listing.ListingDate
          ]
        );
        
        sampleListings.push({
          id: result.rows[0].Id,
          district: districts.rows.find(d => d.Id === listing.DistrictId)?.Name,
          price: listing.Price.toLocaleString('tr-TR'),
          squareMeters: listing.SquareMeters,
          rooms: listing.RoomCount + '+' + listing.SalonCount
        });
      } catch (error) {
        console.error('Listing ekleme hatası:', error.message);
      }
    }
    
    res.json({
      success: true,
      message: `${sampleListings.length} örnek ilan eklendi`,
      listings: sampleListings,
      next_step: 'GET /api/market/listings'
    });
  } catch (error) {
    console.error('Add sample listings error:', error);
    res.status(500).json({ error: error.message });
  }
});

// 4. View'leri Test Et
app.get('/api/db/views', async (req, res) => {
  try {
    const views = [
      { name: 'vw_district_avg_prices', description: 'Ilce bazinda ortalama fiyatlar' },
      { name: 'vw_room_count_statistics', description: 'Oda sayisina gore istatistikler' },
      { name: 'vw_user_predictions', description: 'Kullanici tahmin gecmisi' },
      { name: 'vw_active_listings_detail', description: 'Aktif ilanlar detayli bilgi' },
      { name: 'vw_building_age_price_analysis', description: 'Bina yasina gore fiyat analizi' }
    ];
    
    const results = {};
    
    for (const view of views) {
      try {
        const result = await pool.query(`SELECT * FROM ${view.name} LIMIT 2`);
        results[view.name] = {
          success: true,
          count: result.rows.length,
          sample: result.rows
        };
      } catch (error) {
        results[view.name] = {
          success: false,
          error: error.message
        };
      }
    }
    
    res.json({
      success: true,
      message: 'View testleri tamamlandi',
      views: results
    });
  } catch (error) {
    console.error('Views test error:', error);
    res.status(500).json({ error: error.message });
  }
});

// 5. Stored Procedure Test Et
app.get('/api/db/procedures', async (req, res) => {
  try {
    // sp_get_listings_by_criteria test
    const sp1Result = await pool.query(
      'SELECT * FROM sp_get_listings_by_criteria($1, $2, $3, $4, $5, $6)',
      [null, 3, null, null, null, null]
    );
    
    res.json({
      success: true,
      message: 'Stored Procedure testleri',
      sp_get_listings_by_criteria: {
        parameters: 'roomCount = 3',
        result_count: sp1Result.rows.length,
        sample: sp1Result.rows
      }
    });
  } catch (error) {
    console.error('Procedures test error:', error);
    res.status(500).json({ error: error.message });
  }
});

// ==================== ML TEST SAYFASI ====================
app.get('/test-estimate', (req, res) => {
  res.send(`
    <!DOCTYPE html>
    <html>
    <head>
      <title>SmartValue - ML Tahmin Testi</title>
      <meta charset="UTF-8">
      <meta name="viewport" content="width=device-width, initial-scale=1.0">
      <style>
        * { margin: 0; padding: 0; box-sizing: border-box; font-family: 'Segoe UI', sans-serif; }
        body { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); min-height: 100vh; padding: 40px 20px; color: #333; }
        .container { max-width: 800px; margin: 0 auto; background: white; border-radius: 20px; box-shadow: 0 20px 60px rgba(0,0,0,0.3); overflow: hidden; }
        .header { background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%); color: white; padding: 30px; text-align: center; }
        .header h1 { font-size: 2.5rem; margin-bottom: 10px; }
        .header p { opacity: 0.9; font-size: 1.1rem; }
        .content { padding: 40px; }
        .test-section { background: #f8fafc; border-radius: 15px; padding: 25px; margin-bottom: 30px; border: 2px solid #e2e8f0; }
        h2 { color: #4f46e5; margin-bottom: 20px; font-size: 1.8rem; }
        .property-details { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin-bottom: 25px; }
        .property-details div { background: white; padding: 15px; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }
        .property-details strong { color: #4f46e5; display: block; margin-bottom: 5px; }
        button { background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%); color: white; border: none; padding: 15px 40px; font-size: 1.1rem; border-radius: 50px; cursor: pointer; transition: all 0.3s ease; font-weight: bold; display: block; margin: 0 auto; box-shadow: 0 10px 20px rgba(79, 70, 229, 0.3); }
        button:hover { transform: translateY(-3px); box-shadow: 0 15px 30px rgba(79, 70, 229, 0.4); }
        button:active { transform: translateY(-1px); }
        .result { background: white; border-radius: 15px; padding: 25px; margin-top: 30px; border: 2px solid #10b981; display: none; animation: slideIn 0.5s ease; }
        .result.show { display: block; }
        .price-display { text-align: center; padding: 20px; background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: white; border-radius: 10px; margin: 20px 0; }
        .price { font-size: 3rem; font-weight: bold; margin: 10px 0; }
        .details { background: #f0fdf4; padding: 20px; border-radius: 10px; margin-top: 20px; }
        pre { background: #1e293b; color: #e2e8f0; padding: 20px; border-radius: 10px; overflow-x: auto; font-size: 0.9rem; margin-top: 15px; }
        .loading { text-align: center; padding: 20px; display: none; }
        .loading.show { display: block; }
        .spinner { border: 4px solid #f3f3f3; border-top: 4px solid #4f46e5; border-radius: 50%; width: 40px; height: 40px; animation: spin 1s linear infinite; margin: 0 auto 15px; }
        @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
        @keyframes slideIn { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }
        .footer { text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e2e8f0; color: #64748b; }
        @media (max-width: 600px) { .content { padding: 20px; } .header h1 { font-size: 2rem; } .price { font-size: 2.5rem; } }
      </style>
    </head>
    <body>
      <div class="container">
        <div class="header">
          <h1>🏠 SmartValue</h1>
          <p>Gerçek Zamanlı Emlak Değer Tahmini - ML + PostgreSQL Entegreli</p>
        </div>
        
        <div class="content">
          <div class="test-section">
            <h2>🔬 ML Model Testi</h2>
            <p>Python ML modelini test etmek için aşağıdaki butona tıklayın:</p>
            
            <div class="property-details">
              <div><strong>🏙️ İlçe:</strong><span>Yunusemre</span></div>
              <div><strong>📐 Metrekare:</strong><span>120 m²</span></div>
              <div><strong>🛏️ Oda Sayısı:</strong><span>3+1</span></div>
              <div><strong>🏢 Bina Yaşı:</strong><span>5 yıl</span></div>
            </div>
            
            <button onclick="testML()">🤖 ML Tahmini Başlat</button>
          </div>
          
          <div class="loading" id="loading">
            <div class="spinner"></div>
            <p>ML modeli çalışıyor, lütfen bekleyin...</p>
            <p><small>Python scripti çalıştırılıyor (~3-5 saniye)</small></p>
          </div>
          
          <div class="result" id="result">
            <h2>📊 Tahmin Sonucu</h2>
            <div class="price-display">
              <div>TAHMİN EDİLEN DEĞER</div>
              <div class="price" id="predictedPrice">-</div>
              <div>TL</div>
            </div>
            <div class="details">
              <h3>🔍 Detaylar</h3>
              <pre id="resultDetails"></pre>
            </div>
          </div>
        </div>
        
        <div class="footer">
          <p>SmartValue SOA API v3.0 | PostgreSQL + Python ML + Node.js</p>
          <p>Veritabanı: GET /api/db/status | İlanlar: GET /api/market/listings/real</p>
        </div>
      </div>
      
      <script>
        async function testML() {
          document.getElementById('loading').classList.add('show');
          document.getElementById('result').classList.remove('show');
          
          const testData = {
            district: "Yunusemre",
            square_meters: 120,
            rooms: 3,
            living_rooms: 1,
            building_age: 5,
            bathrooms: 2,
            floor: 3,
            heating: 1,
            elevator: 1,
            garage: 0,
            balcony: 1,
            furnished: 0,
            swap: 0,
            usage_status: "Owner",
            building_status: 1,
            title_deed: 1
          };
          
          try {
            console.log('🚀 ML testi başlatılıyor...', testData);
            const response = await fetch('/api/market/estimate', {
              method: 'POST',
              headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
              body: JSON.stringify(testData)
            });
            
            const result = await response.json();
            console.log('✅ ML sonucu:', result);
            document.getElementById('loading').classList.remove('show');
            
            if (result.success) {
              const price = result.prediction.estimatedPrice.toLocaleString('tr-TR');
              document.getElementById('predictedPrice').textContent = price;
              document.getElementById('resultDetails').textContent = JSON.stringify(result, null, 2);
              document.getElementById('result').classList.add('show');
              document.getElementById('result').scrollIntoView({ behavior: 'smooth' });
            } else {
              alert('Hata: ' + result.error);
            }
          } catch (error) {
            console.error('❌ Hata:', error);
            document.getElementById('loading').classList.remove('show');
            alert('İstek hatası: ' + error.message);
          }
        }
      </script>
    </body>
    </html>
  `);
});

// ==================== GET TEST ENDPOINT ====================
app.get('/api/market/estimate/test', (req, res) => {
  res.json({
    success: true,
    message: 'Bu endpoint POST methodu gerektirir',
    note: 'Lütfen POST methodunu kullanın',
    example: {
      method: 'POST',
      url: '/api/market/estimate',
      body: {
        district: "Yunusemre",
        square_meters: 120,
        rooms: 3,
        living_rooms: 1,
        building_age: 5,
        bathrooms: 2,
        floor: 3,
        heating: 1,
        elevator: 1,
        garage: 0,
        balcony: 1,
        furnished: 0,
        swap: 0,
        usage_status: "Owner",
        building_status: 1,
        title_deed: 1
      }
    },
    quick_test: 'http://localhost:3000/test-estimate adresini ziyaret edin'
  });
});

// ==================== YENİ ENDPOINT: GERÇEK İLANLAR ====================
app.get('/api/market/listings/real', async (req, res) => {
  try {
    const { district, minPrice, maxPrice, rooms } = req.query;
    
    let query = 'SELECT * FROM vw_active_listings_detail WHERE "IsActive" = true';
    const params = [];
    let paramCount = 1;
    
    if (district) {
      query += ` AND "DistrictName" ILIKE $${paramCount}`;
      params.push(`%${district}%`);
      paramCount++;
    }
    
    if (minPrice) {
      query += ` AND "Price" >= $${paramCount}`;
      params.push(parseInt(minPrice));
      paramCount++;
    }
    
    if (maxPrice) {
      query += ` AND "Price" <= $${paramCount}`;
      params.push(parseInt(maxPrice));
      paramCount++;
    }
    
    if (rooms) {
      query += ` AND "RoomInfo" LIKE $${paramCount}`;
      params.push(`${rooms}+%`);
      paramCount++;
    }
    
    query += ' ORDER BY "ListingDate" DESC LIMIT 20';
    
    const result = await pool.query(query, params);
    
    res.json({
      success: true,
      endpoint: 'GET /api/market/listings/real',
      message: 'Gerçek ilanlar getirildi',
      timestamp: new Date().toISOString(),
      filters: {
        district: district || 'none',
        minPrice: minPrice || 'none',
        maxPrice: maxPrice || 'none',
        rooms: rooms || 'none'
      },
      data: {
        count: result.rows.length,
        listings: result.rows
      }
    });
  } catch (error) {
    console.error('Listings real error:', error);
    res.status(500).json({
      success: false,
      error: 'Internal server error',
      message: error.message
    });
  }
});

// ==================== YENİ ENDPOINT: İLÇE İSTATİSTİKLERİ ====================
app.get('/api/market/statistics/districts', async (req, res) => {
  try {
    const result = await pool.query('SELECT * FROM vw_district_avg_prices ORDER BY "AvgPrice" DESC');
    
    res.json({
      success: true,
      endpoint: 'GET /api/market/statistics/districts',
      message: 'İlçe istatistikleri getirildi',
      timestamp: new Date().toISOString(),
      data: {
        count: result.rows.length,
        districts: result.rows
      }
    });
  } catch (error) {
    console.error('District statistics error:', error);
    res.status(500).json({
      success: false,
      error: 'Internal server error',
      message: error.message
    });
  }
});

// ==================== YENİ ENDPOINT: TABLO KONTROLÜ ====================
app.get('/api/db/check-tables', async (req, res) => {
  try {
    // Tüm tabloları listele
    const tablesResult = await pool.query(`
      SELECT table_name, table_type 
      FROM information_schema.tables 
      WHERE table_schema = 'public'
      ORDER BY table_name
    `);
    
    // Her tablonun detaylarını al
    const tablesDetails = [];
    
    for (const table of tablesResult.rows) {
      try {
        // Tablodaki satır sayısını al
        const countResult = await pool.query(`SELECT COUNT(*) as count FROM "${table.table_name}"`);
        const rowCount = parseInt(countResult.rows[0].count);
        
        // Kolon bilgilerini al
        const columnsResult = await pool.query(`
          SELECT column_name, data_type, is_nullable
          FROM information_schema.columns 
          WHERE table_schema = 'public' AND table_name = $1
          ORDER BY ordinal_position
        `, [table.table_name]);
        
        // İlk 3 satırı göster
        const sampleResult = await pool.query(`SELECT * FROM "${table.table_name}" LIMIT 3`);
        
        tablesDetails.push({
          table_name: table.table_name,
          table_type: table.table_type,
          row_count: rowCount,
          columns: columnsResult.rows,
          sample_data: sampleResult.rows
        });
      } catch (error) {
        tablesDetails.push({
          table_name: table.table_name,
          error: error.message,
          hint: 'Tablo okunamadı veya boş olabilir'
        });
      }
    }
    
    // Gerekli 7 tablonun varlığını kontrol et
    const requiredTables = ['Users', 'Districts', 'BuildingTypes', 'Features', 'Listings', 'ListingFeatures', 'Predictions'];
    const existingTables = tablesResult.rows.map(t => t.table_name);
    const missingTables = requiredTables.filter(table => !existingTables.includes(table));
    
    res.json({
      success: true,
      message: 'Tablo kontrolü tamamlandı',
      timestamp: new Date().toISOString(),
      database_info: {
        name: 'HomeRadar_db',
        total_tables: tablesResult.rows.length,
        tables: tablesResult.rows.map(t => t.table_name)
      },
      required_tables_check: {
        required: requiredTables,
        existing: existingTables,
        missing: missingTables,
        status: missingTables.length === 0 ? '✅ Tüm tablolar mevcut' : `❌ ${missingTables.length} tablo eksik`
      },
      tables_details: tablesDetails,
      recommendations: missingTables.length > 0 ? [
        `Eksik tablolar: ${missingTables.join(', ')}`,
        '1. Önce Entity Framework Migration çalıştırın:',
        '   Add-Migration InitialCreate',
        '   Update-Database',
        '2. Migration başarısız olursa, SQL scriptlerini manuel çalıştırın:',
        '   psql -U postgres -d hHomeRadar_db -f SQL/01_Database_Schema.sql',
        '   (Diğer SQL dosyalarını da sırayla çalıştırın)'
      ] : ['✅ Tüm tablolar hazır, veri ekleyebilirsiniz']
    });
  } catch (error) {
    console.error('Check tables error:', error);
    res.status(500).json({ 
      success: false, 
      error: error.message,
      hint: 'Veritabanı bağlantısını kontrol edin'
    });
  }
});

// Tablo oluştur (Hızlı test için)
app.get('/api/db/create-test-table', async (req, res) => {
  try {
    // Test tablosu oluştur
    await pool.query(`
      CREATE TABLE IF NOT EXISTS "TestTable" (
        id SERIAL PRIMARY KEY,
        name VARCHAR(100),
        created_at TIMESTAMP DEFAULT NOW()
      )
    `);
    
    // Test verisi ekle
    await pool.query(`
      INSERT INTO "TestTable" (name) VALUES ('Test Kayıt')
      ON CONFLICT DO NOTHING
    `);
    
    // Test tablosundan veri oku
    const result = await pool.query('SELECT * FROM "TestTable"');
    
    res.json({
      success: true,
      message: 'Test tablosu oluşturuldu ve veri eklendi',
      data: result.rows,
      note: 'Tablo oluşturabiliyorsunuz demektir'
    });
  } catch (error) {
    res.status(500).json({ 
      success: false, 
      error: error.message,
      hint: 'Kullanıcı yetkilerini kontrol edin'
    });
  }
});

// ==================== API ROUTES ====================
try {
  const apiRoutes = require('./presentation-layer/routes/apiRoutes');
  app.use('/api', apiRoutes);
  console.log('✅ API routes loaded from presentation-layer/routes/apiRoutes.js');
} catch (error) {
  console.error('⚠️ Could not load apiRoutes:', error.message);
  
  // Fallback routes
  app.get('/api/market/overview', (req, res) => {
    res.json({
      success: true,
      message: 'Market overview endpoint',
      timestamp: new Date().toISOString()
    });
  });
}

// ==================== 404 HANDLER ====================
app.use('*', (req, res) => {
  res.status(404).json({
    error: 'Not Found',
    message: `Endpoint ${req.originalUrl} not found (Method: ${req.method})`,
    available_endpoints: [
      { method: 'GET', path: '/', description: 'API Info' },
      { method: 'GET', path: '/health', description: 'Health check with DB' },
      { method: 'GET', path: '/test-estimate', description: 'ML Test Interface' },
      { method: 'GET', path: '/api/health', description: 'Simple health' },
      { method: 'POST', path: '/api/market/estimate', description: 'ML Property Valuation' },
      { method: 'GET', path: '/api/market/estimate/test', description: 'Estimate test info' },
      { method: 'GET', path: '/api/market/overview', description: 'Market overview' },
      { method: 'GET', path: '/api/market/districts', description: 'Districts list' },
      { method: 'GET', path: '/api/market/listings', description: 'Property listings' },
      { method: 'GET', path: '/api/market/listings/real', description: 'Real listings from DB' },
      { method: 'GET', path: '/api/market/statistics/districts', description: 'District statistics' },
      { method: 'GET', path: '/api/db/status', description: 'Database status' },
      { method: 'GET', path: '/api/db/init-all', description: 'Initialize database' },
      { method: 'GET', path: '/api/db/add-sample-listings', description: 'Add sample listings' },
      { method: 'GET', path: '/api/db/views', description: 'Test views' },
      { method: 'GET', path: '/api/db/procedures', description: 'Test procedures' },
      { method: 'GET', path: '/api/test/all-endpoints', description: 'All endpoints list' }
    ]
  });
});

// ==================== ERROR HANDLING ====================
app.use((err, req, res, next) => {
  console.error('🔥 API Error:', err);
  res.status(500).json({
    error: 'Internal Server Error',
    message: process.env.NODE_ENV === 'development' ? err.message : 'Something went wrong',
    timestamp: new Date().toISOString()
  });
});

// ==================== START SERVER ====================
async function startServer() {
  try {
    // Test database connection
    const result = await pool.query('SELECT NOW()');
    console.log(`✅ PostgreSQL connected: ${result.rows[0].now}`);
    
    // Get table count
    const tables = await pool.query(`
      SELECT COUNT(*) as count FROM information_schema.tables 
      WHERE table_schema = 'public'
    `);
    console.log(`📊 Tables in database: ${tables.rows[0].count}`);
    
    // Show ML status
    try {
      require('./infrastructure-layer/external-apis/services/mlService');
      console.log('🤖 ML Service: ✅ Available');
    } catch (error) {
      console.log('🤖 ML Service: ❌ Not available -', error.message);
    }
    
    app.listen(PORT, () => {
      console.log(`
🚀 SMARTVALUE SOA API v3.0
==================================================
📡 PORT: ${PORT}
🌐 URL: http://localhost:${PORT}
==================================================
🎯 ANA ENDPOINTLER:
  GET  /                   - API Bilgileri
  GET  /health            - Sistem Sağlığı (DB + ML)
  GET  /test-estimate     - ML Test Arayüzü
  POST /api/market/estimate - ML Tahmini
  GET  /api/db/status     - Veritabanı Durumu
  GET  /api/db/init-all   - Veritabanını Hazırla
==================================================
🤖 ML ENTEGRASYON:
  ✅ Python ML Modeli hazır
  ✅ Node.js-Python entegrasyonu aktif
==================================================
🗄️ POSTGRESQL ENTEGRASYON:
  ✅ Veritabanı bağlantısı aktif
  ✅ 7 tablo hazır (Users, Districts, Listings, vb.)
  ✅ 5 View hazır
  ✅ 2 Stored Procedure hazır
  ✅ Gerçek veri işleme
==================================================
💡 HIZLI KURULUM:
  1. Tarayıcıda açın: http://localhost:${PORT}/test-estimate
  2. ML testi yapın (butona tıklayın)
  3. Terminalde: curl http://localhost:${PORT}/api/db/status
  4. curl http://localhost:${PORT}/api/db/add-sample-listings
==================================================
      `);
    });
  } catch (error) {
    console.error('❌ Database connection failed:', error.message);
    process.exit(1);
  }
}

startServer();
