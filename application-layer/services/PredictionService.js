// 📄 application-layer/services/PredictionService.js
// ML servisini kullanarak gerçek tahmin yapan servis
const mlService = require('../../infrastructure-layer/external-apis/services/mlService');

class PredictionService {
  // Controller'dan çağrılan ana metod
  async predict(input) {
    return this.predictPrice(input);
  }

  async predictPrice(input) {
    try {
      // DEBUG: Validasyon öncesi input'u logla
      console.log('[DEBUG] PredictionService - Validasyon öncesi input:');
      console.log('  district:', input.district, `(type: ${typeof input.district}, empty: ${!input.district}, trim: '${input.district ? input.district.trim() : ''}')`);
      console.log('  square_meters:', input.square_meters, `(type: ${typeof input.square_meters}, isNaN: ${isNaN(input.square_meters)})`);
      console.log('  rooms:', input.rooms, `(type: ${typeof input.rooms}, isNaN: ${isNaN(input.rooms)})`);
      console.log('  building_age:', input.building_age, `(type: ${typeof input.building_age}, isNaN: ${isNaN(input.building_age)})`);
      
      // Validasyon - 0 değerlerini de kabul et, sadece null/undefined/NaN/boş string kontrolü yap
      if (!input.district || typeof input.district !== 'string' || input.district.trim() === '') {
        console.error('[DEBUG] PredictionService - District validasyonu başarısız!');
        throw new Error('Tüm alanlar gereklidir: district, square_meters, rooms, building_age');
      }
      if (input.square_meters === null || input.square_meters === undefined || isNaN(input.square_meters)) {
        console.error('[DEBUG] PredictionService - square_meters validasyonu başarısız!');
        throw new Error('Tüm alanlar gereklidir: district, square_meters, rooms, building_age');
      }
      if (input.rooms === null || input.rooms === undefined || isNaN(input.rooms)) {
        console.error('[DEBUG] PredictionService - rooms validasyonu başarısız!');
        throw new Error('Tüm alanlar gereklidir: district, square_meters, rooms, building_age');
      }
      if (input.building_age === null || input.building_age === undefined || isNaN(input.building_age)) {
        console.error('[DEBUG] PredictionService - building_age validasyonu başarısız!');
        throw new Error('Tüm alanlar gereklidir: district, square_meters, rooms, building_age');
      }
      
      console.log('[DEBUG] PredictionService - Validasyon başarılı!');
      if (input.square_meters < 20) {
        throw new Error('Metrekare en az 20 olmalıdır');
      }
      
      // ML servisini çağır (Python script'i çağıran servis)
      const mlResult = await mlService.predict(input);
      
      // ML sonucunu formatla
      if (mlResult.status === 'error') {
        throw new Error(mlResult.message || 'ML tahmini başarısız');
      }
      
      // Python'dan gelen sonuç formatı: { status: 'success', price: 3500000, currency: 'TL' }
      const predictedPrice = mlResult.price || 0;
      
      return {
        success: true,
        prediction: {
          price: predictedPrice,
          minPrice: Math.round(predictedPrice * 0.9),
          maxPrice: Math.round(predictedPrice * 1.1),
          averagePrice: predictedPrice,
          confidence: 0.85, // ML modelinden confidence gelirse buraya eklenebilir
          currency: mlResult.currency || 'TL'
        },
        metadata: {
          modelVersion: 'XGBoost',
          timestamp: new Date().toISOString(),
          source: mlResult.status === 'mock' ? 'mock_calculation' : 'ml_model',
          input: input
        }
      };
      
    } catch (error) {
      console.error('PredictionService error:', error);
      throw error;
    }
  }
  
  async predictWithGrpc(input) {
    // gRPC ile tahmin yapılabilir, şimdilik aynı servisi kullan
    return this.predictPrice(input);
  }
}

module.exports = new PredictionService();