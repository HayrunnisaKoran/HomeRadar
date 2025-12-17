// PostgreSQL bağlantı testi
require('dotenv').config();
const { Pool } = require('pg');

async function testDB() {
  console.log('🔍 PostgreSQL bağlantı testi başlıyor...');
  
  const pool = new Pool({
    host: process.env.DB_HOST || 'localhost',
    port: process.env.DB_PORT || 5432,
    database: process.env.DB_NAME || 'smartvalue',
    user: process.env.DB_USER || 'postgres',
    password: process.env.DB_PASSWORD || 'postgres'
  });

  try {
    const client = await pool.connect();
    console.log('✅ PostgreSQL bağlantısı BAŞARILI!');
    
    // Basit sorgu testi
    const result = await client.query('SELECT NOW() as current_time');
    console.log('📅 Sunucu saati:', result.rows[0].current_time);
    
    // Tabloları listele
    const tables = await client.query(`
      SELECT table_name 
      FROM information_schema.tables 
      WHERE table_schema = 'public'
    `);
    
    console.log('📋 Mevcut tablolar:');
    tables.rows.forEach(table => {
      console.log(`   - ${table.table_name}`);
    });
    
    client.release();
  } catch (err) {
    console.error('❌ PostgreSQL bağlantı hatası:', err.message);
    console.log('💡 Çözüm önerileri:');
    console.log('1. PostgreSQL çalışıyor mu? (services.msc)');
    console.log('2. .env dosyasında doğru bilgiler var mı?');
    console.log('3. Firewall PostgreSQL portunu (5432) engelliyor mu?');
  } finally {
    await pool.end();
  }
}

testDB();