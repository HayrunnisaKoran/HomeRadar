// infrastructure-layer/database/dbConnection.js
const { Pool } = require('pg');

const pool = new Pool({
  host: 'localhost',
  port: 5432,
  database: 'HomeRadar_db',  // BU SATIRI DEĞİŞTİRİN
  user: 'homeradar_app_user',
  password: 'HomeRadar2024!SecurePass',
  max: 20,
  idleTimeoutMillis: 30000,
  connectionTimeoutMillis: 2000,
});

// Test bağlantısı
pool.query('SELECT NOW()', (err, res) => {
  if (err) {
    console.error('❌ PostgreSQL bağlantı hatası:', err.message);
    console.log('📋 Lütfen şu bağlantı bilgilerini kontrol edin:');
    console.log('- Veritabanı: HomeRadar_db');
    console.log('- Kullanıcı: homeradar_app_user');
    console.log('- Şifre: HomeRadar2024!SecurePass');
  } else {
    console.log(`✅ PostgreSQL bağlantısı başarılı: ${res.rows[0].now}`);
  }
});

module.exports = { pool };