// 📄 domain-layer/models/Prediction.js
class Prediction {
  constructor(input, result) {
    this.input = input; // {district, rooms, squareMeters, buildingAge}
    this.result = result; // {minPrice, maxPrice, averagePrice, confidence}
    this.timestamp = new Date();
    this.modelVersion = 'v1.0-mock';
  }
  
  static calculateMockPrediction(input) {
    // Eski app.js'deki hesaplama mantığı
    const districtRates = {
      'yunusemre': 1.3, 'şehzadeler': 1.2, 'akhisar': 1.0,
      'turgutlu': 0.9, 'salihli': 0.85
    };
    
    const roomRates = {
      '1+1': 0.8, '2+1': 1.0, '3+1': 1.4, '4+1': 1.8
    };
    
    const basePrice = 7500;
    const districtFactor = districtRates[input.district.toLowerCase()] || 1.0;
    const roomFactor = roomRates[input.rooms] || 1.0;
    const ageFactor = Math.max(0.7, 1 - (input.buildingAge * 0.015));
    const sizeFactor = input.squareMeters / 80;
    
    const estimated = basePrice * districtFactor * roomFactor * ageFactor * sizeFactor;
    
    return {
      minPrice: Math.round(estimated * 0.88),
      maxPrice: Math.round(estimated * 1.12),
      averagePrice: Math.round(estimated),
      confidence: 0.82,
      currency: 'TRY'
    };
  }
}

module.exports = Prediction;