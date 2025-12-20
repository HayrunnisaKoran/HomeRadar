const { Pool } = require('pg');

const pool = new Pool({
    host: 'localhost',
    port: 5432,
    database: 'homeradar_db',
    user: 'postgres',
    password: '250400',
});

async function test() {
    console.log('🔌 PostgreSQL Bağlantı Testi...');
    
    try {
        // 1. Bağlantı testi
        const connResult = await pool.query('SELECT version(), current_database()');
        console.log('✅ PostgreSQL BAĞLANTISI ÇALIŞIYOR!');
        console.log('   Database:', connResult.rows[0].current_database);
        console.log('   Version:', connResult.rows[0].version.split(',')[0]);
        
        // 2. Fonksiyonları test et
        const funcResult = await pool.query('SELECT fn_calculate_price_per_square_meter(300000, 120) as price_per_sqm');
        console.log('✅ Fonksiyonlar çalışıyor:', funcResult.rows[0].price_per_sqm);
        
        // 3. Kullanıcıları kontrol et
        const users = await pool.query('SELECT * FROM pg_catalog.pg_roles WHERE rolname LIKE \'%homeradar%\';');
        console.log('✅ Kullanıcılar hazır:', users.rows.map(u => u.rolname));
        
    } catch (err) {
        console.log('❌ Hata:', err.message);
    }
    
    await pool.end();
}

test();