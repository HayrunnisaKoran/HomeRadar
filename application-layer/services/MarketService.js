const propertyRepository = require('../../data-layer/repositories/PropertyRepository');
const mlService = require('../../infrastructure-layer/external-apis/services/mlService');

class MarketService {
  
  // İlan arama servisi
  async searchProperties(filters) {
    try {
      const properties = await propertyRepository.getListingsByCriteria(filters);
      
      // Eğer ML servisi varsa, tahmin ekle
      if (filters.includePrediction) {
        for (const property of properties) {
          try {
            const prediction = await mlService.predict(property);
            property.predictedPrice = prediction.price || 0;
          } catch (error) {
            console.warn('ML tahmini başarısız:', error.message);
          }
        }
      }
      
      return properties;
    } catch (error) {
      throw new Error(`İlan arama hatası: ${error.message}`);
    }
  }

  // Fiyat tahmini servisi
  async estimatePrice(predictionRequest) {
    try {
      // 1. ML servisine tahmin yaptır
      const mlResult = await mlService.predict(predictionRequest);
      
      // ML sonucunu formatla (MLService'den gelen format: { status: 'success', price: 3500000, currency: 'TL' })
      const predictedPrice = mlResult.price || 0;
      const minPrice = Math.round(predictedPrice * 0.9);
      const maxPrice = Math.round(predictedPrice * 1.1);
      
      // 2. Veritabanında tahmin kaydı oluştur
      const predictionId = await propertyRepository.createPrediction({
        ...predictionRequest,
        predictedPriceMin: minPrice,
        predictedPriceMax: maxPrice,
        modelName: 'XGBoost',
        confidenceScore: 0.85
      });
      
      // 3. Veritabanından benzer ilanları getir
      const similarProperties = await propertyRepository.getListingsByCriteria({
        districtId: predictionRequest.districtId,
        roomCount: predictionRequest.roomCount,
        minPrice: minPrice * 0.9,
        maxPrice: maxPrice * 1.1
      });
      
      return {
        predictionId,
        estimatedPrice: {
          min: minPrice,
          max: maxPrice,
          average: predictedPrice
        },
        confidence: 0.85,
        model: 'XGBoost',
        similarProperties: similarProperties.slice(0, 5) // İlk 5 benzer ilan
      };
    } catch (error) {
      throw new Error(`Fiyat tahmini hatası: ${error.message}`);
    }
  }

  // İstatistik servisi
  async getMarketStatistics() {
    try {
      const [avgPrices, activeListings] = await Promise.all([
        propertyRepository.getDistrictAveragePrices(),
        propertyRepository.getActiveListings()
      ]);
      
      // User Defined Function ile metrekare fiyatını hesapla
      const totalPrice = activeListings.reduce((sum, listing) => sum + parseFloat(listing.Price), 0);
      const totalArea = activeListings.reduce((sum, listing) => sum + parseFloat(listing.SquareMeters), 0);
      const avgPricePerSqm = totalArea > 0 ? totalPrice / totalArea : 0;
      
      return {
        districtStatistics: avgPrices,
        activeListingsCount: activeListings.length,
        averagePricePerSquareMeter: avgPricePerSqm,
        totalMarketValue: totalPrice
      };
    } catch (error) {
      throw new Error(`İstatistik alma hatası: ${error.message}`);
    }
  }

  // Yeni ilan ekleme servisi
  async addNewProperty(propertyData) {
    try {
      // Veri doğrulama
      if (!propertyData.districtId || !propertyData.price || propertyData.price <= 0) {
        throw new Error('Geçersiz ilan verisi');
      }
      
      const newProperty = await propertyRepository.createListing(propertyData);
      
      // ML servisine bu yeni ilanı da öğret (opsiyonel)
      // await mlService.trainWithNewData(newProperty);
      
      return {
        success: true,
        propertyId: newProperty.Id,
        message: 'İlan başarıyla eklendi'
      };
    } catch (error) {
      throw new Error(`İlan ekleme hatası: ${error.message}`);
    }
  }
}

module.exports = new MarketService();