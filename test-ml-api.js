// test-ml-api.js
require('dotenv').config();

// YOLU DÜZELT: mlService.js'nin tam yolu
const MLService = require('./infrastructure-layer/external-apis/services/mlService');

async function testML() {
  console.log('🤖 ML Entegrasyon Testi Başlıyor...\n');
  
  // Basit test verisi (Python'un çalıştığını biliyoruz)
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
  
  console.log('📤 Test Verisi:');
  console.log(JSON.stringify(testData, null, 2));
  console.log('\n---\n');
  
  try {
    console.log('🚀 MLService.predict() çağrılıyor...');
    const result = await MLService.predict(testData);
    
    console.log('✅ BAŞARILI!\n');
    console.log('💰 TAHMİN EDİLEN FİYAT:', result.price.toLocaleString('tr-TR'), result.currency || 'TL');
    console.log('📊 DURUM:', result.status || 'success');
    
    if (result.details) {
      console.log('📝 DETAYLAR:', JSON.stringify(result.details, null, 2));
    }
    
    // Ek bilgi
    console.log('\n🎯 SONUÇ:');
    console.log(`• ${testData.rooms}+${testData.living_rooms} daire`);
    console.log(`• ${testData.square_meters} m²`);
    console.log(`• ${testData.district} ilçesi`);
    console.log(`• Tahmini değer: ${result.price.toLocaleString('tr-TR')} TL`);
    
  } catch (error) {
    console.error('\n❌ HATA:', error.message);
    console.error('\n🔍 Stack trace:', error.stack);
  }
}

// Testi çalıştır
testML();