const { pool } = require('./infrastructure-layer/database/dbConnection');

async function testDatabase() {
  console.log('📊 Veritabanı Testi Başlıyor...\n');
  
  try {
    // 1. Bağlantı testi
    const client = await pool.connect();
    console.log('✅ PostgreSQL bağlantısı başarılı');
    
    // 2. Tablo sayıları
    console.log('\n📋 Tablo İstatistikleri:');
    
    const tables = [
      '"Users"', '"Districts"', '"BuildingTypes"', '"Features"',
      '"Listings"', '"ListingFeatures"', '"Predictions"'
    ];
    
    for (const table of tables) {
      const result = await client.query(`SELECT COUNT(*) FROM ${table}`);
      console.log(`  ${table}: ${result.rows[0].count} kayıt`);
    }
    
    // 3. View testi
    console.log('\n👁️ View Testi:');
    const viewResult = await client.query('SELECT * FROM vw_district_avg_prices LIMIT 3');
    console.log('  vw_district_avg_prices (ilk 3 kayıt):');
    viewResult.rows.forEach(row => {
      console.log(`    - ${row.DistrictName}: ${row.AveragePrice} TL`);
    });
    
    // 4. Stored Procedure testi
    console.log('\n⚡ Stored Procedure Testi:');
    const spResult = await client.query(
      'SELECT * FROM sp_get_listings_by_criteria(1, 3, 300000, 500000, NULL, NULL) LIMIT 2'
    );
    console.log('  sp_get_listings_by_criteria (ilk 2 kayıt):');
    spResult.rows.forEach(row => {
      console.log(`    - İlan #${row.Id}: ${row.Price} TL, ${row.SquareMeters} m²`);
    });
    
    // 5. Function testi
    console.log('\n🧮 Function Testi:');
    const fnResult = await client.query(
      'SELECT fn_calculate_price_per_square_meter(450000, 120) AS price_per_sqm'
    );
    console.log(`  fn_calculate_price_per_square_meter: ${fnResult.rows[0].price_per_sqm} TL/m²`);
    
    client.release();
    
    console.log('\n🎉 Tüm testler başarıyla tamamlandı!');
    process.exit(0);
    
  } catch (error) {
    console.error('❌ Test sırasında hata:', error.message);
    process.exit(1);
  }
}

testDatabase();