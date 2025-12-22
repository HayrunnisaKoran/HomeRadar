// 📄 application-layer/services/PredictionService.js (GÜNCELLE)
class PredictionService {
  async predictPrice(input) {
    try {
      const { district, rooms, squareMeters, buildingAge } = input;
      
      // Basit validasyon
      if (!district || !rooms || !squareMeters || !buildingAge) {
        throw new Error('Tüm alanlar gereklidir');
      }
      if (squareMeters < 20) throw new Error('Metrekare en az 20 olmalıdır');
      
      // Hesaplama
      const basePrice = 7500;
      const districtFactor = district === 'Yunusemre' ? 1.3 : 
                            district === 'Şehzadeler' ? 1.2 : 1.0;
      const roomFactor = rooms === '3+1' ? 1.4 : 
                        rooms === '2+1' ? 1.0 : 0.8;
      const ageFactor = Math.max(0.7, 1 - (buildingAge * 0.015));
      const sizeFactor = squareMeters / 80;
      
      const estimated = basePrice * districtFactor * roomFactor * ageFactor * sizeFactor;
      
      return {
        success: true,
        prediction: {
          minPrice: Math.round(estimated * 0.88),
          maxPrice: Math.round(estimated * 1.12),
          averagePrice: Math.round(estimated),
          confidence: 0.82,
          currency: 'TRY'
        },
        metadata: {
          modelVersion: 'v1.0-mock',
          timestamp: new Date().toISOString(),
          input: input
        }
      };
      
    } catch (error) {
      console.error('PredictionService error:', error);
      throw error;
    }
  }
  
  async predictWithGrpc(input) {
    // Şimdilik aynı hesaplamayı döndür
    return this.predictPrice(input);
  }
}

module.exports = new PredictionService();