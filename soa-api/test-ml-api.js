// ML API testi
require('dotenv').config();
const axios = require('axios');

async function testML() {
  console.log('🤖 ML API testi başlıyor...');
  
  const mlUrl = process.env.ML_API_URL || 'http://localhost:5000/predict';
  
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
  
  console.log('📤 Gönderilen veri:', JSON.stringify(testData, null, 2));
  
  try {
    console.log(`🌐 ML API URL: ${mlUrl}`);
    const response = await axios.post(mlUrl, testData, {
      headers: { 'Content-Type': 'application/json' },
      timeout: 10000 // 10 saniye timeout
    });
    
    console.log('✅ ML API BAŞARILI!');
    console.log('📥 Gelen cevap:', JSON.stringify(response.data, null, 2));
  } catch (error) {
    console.error('❌ ML API hatası:', error.message);
    
    if (error.code === 'ECONNREFUSED') {
      console.log('💡 ML API çalışmıyor. Şunları kontrol edin:');
      console.log('1. Üye 2\'nin Flask API\'si çalışıyor mu? (python app.py)');
      console.log('2. Doğru port mu? (Genellikle 5000)');
      console.log('3. Firewall engelliyor mu?');
    } else if (error.response) {
      console.log('📉 HTTP Hatası:', error.response.status);
      console.log('Hata detayı:', error.response.data);
    }
    
    // Mock (sahte) veri döndür
    console.log('\n🔄 Mock (sahte) veri kullanılıyor...');
    const mockPrice = testData.square_meters * 5000 + testData.rooms * 50000;
    console.log('💰 Tahmini fiyat (mock):', mockPrice, 'TL');
  }
}

testML();