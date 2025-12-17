const axios = require('axios');
require('dotenv').config();

class MLService {
  constructor() {
    this.mlApiUrl = process.env.ML_API_URL || 'http://localhost:5000/predict';
  }

  async getPrediction(propertyData) {
    try {
      // ML API'ye gönderilecek veriyi formatlayın
      const formattedData = {
        district: propertyData.district,
        square_meters: propertyData.square_meters,
        rooms: propertyData.rooms,
        living_rooms: propertyData.living_rooms || 1,
        building_age: propertyData.building_age,
        bathrooms: propertyData.bathrooms || 1,
        floor: propertyData.floor || 0,
        heating: propertyData.heating ? 1 : 0,
        elevator: propertyData.elevator ? 1 : 0,
        garage: propertyData.garage ? 1 : 0,
        balcony: propertyData.balcony ? 1 : 0,
        furnished: propertyData.furnished ? 1 : 0,
        swap: propertyData.swap ? 1 : 0,
        usage_status: propertyData.usage_status || "Owner",
        building_status: propertyData.building_status || 0,
        title_deed: propertyData.title_deed ? 1 : 0
      };

      console.log('📤 ML API\'ye gönderilen veri:', formattedData);

      // ML API'ye POST isteği gönder
      const response = await axios.post(this.mlApiUrl, formattedData, {
        headers: { 'Content-Type': 'application/json' }
      });

      console.log('📥 ML API\'den gelen cevap:', response.data);

      if (response.data.status === 'success') {
        return {
          success: true,
          price: response.data.price,
          currency: response.data.currency,
          details: response.data.details || {}
        };
      } else {
        throw new Error(response.data.message || 'ML API hatası');
      }
    } catch (error) {
      console.error('❌ ML API hatası:', error.message);
      return {
        success: false,
        error: error.message,
        // Fallback: Eğer ML API çalışmıyorsa basit bir hesaplama yap
        price: this.calculateFallbackPrice(propertyData)
      };
    }
  }

  // Fallback fiyat hesaplama (ML API çalışmazsa)
  calculateFallbackPrice(data) {
    const basePrice = data.square_meters * 5000; // m² başına 5000 TL
    const roomBonus = data.rooms * 50000;
    const ageDiscount = data.building_age * 10000;
    
    return basePrice + roomBonus - ageDiscount;
  }
}

module.exports = new MLService();