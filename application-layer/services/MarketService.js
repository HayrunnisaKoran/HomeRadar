// application-layer/services/MarketService.js
const PropertyRepository = require('../../data-layer/repositories/PropertyRepository');
const ExternalApiService = require('../../infrastructure-layer/external-apis/services/googleMapsService');

class MarketService {
  constructor() {
    this.propertyRepository = new PropertyRepository();
    this.externalApiService = new ExternalApiService();
  }

  async getPropertyEstimation(params) {
    try {
      // 1. Benzer ilanları getir
      const similarListings = await this.propertyRepository.getListingsByCriteria({
        districtId: params.districtId,
        roomCount: params.roomCount,
        minSquareMeters: params.squareMeters * 0.8,
        maxSquareMeters: params.squareMeters * 1.2
      });

      // 2. View'den ilçe istatistiklerini al
      const districtStats = await this.propertyRepository.getDistrictAveragePrices();
      const currentDistrict = districtStats.find(d => d.districtid === params.districtId);

      // 3. User Defined Function ile tahmin yap
      const estimatedPrice = await this.propertyRepository.estimatePrice(
        params.districtId,
        params.roomCount,
        params.squareMeters,
        params.buildingAge
      );

      // 4. Dış API'den ek bilgiler (Google Maps)
      const locationData = await this.externalApiService.getLocationInfo(params.districtId);

      return {
        success: true,
        data: {
          similarListings: similarListings.slice(0, 5), // İlk 5 benzer ilan
          districtStatistics: currentDistrict,
          estimatedPrice: estimatedPrice,
          locationInfo: locationData,
          calculationDetails: {
            roomCount: params.roomCount,
            squareMeters: params.squareMeters,
            buildingAge: params.buildingAge
          }
        }
      };
    } catch (error) {
      console.error('MarketService.getPropertyEstimation hatası:', error);
      return {
        success: false,
        error: error.message
      };
    }
  }

  async getMarketOverview() {
    try {
      const [
        districtPrices,
        ageAnalysis,
        roomStats,
        activeListings
      ] = await Promise.all([
        this.propertyRepository.getDistrictAveragePrices(),
        this.propertyRepository.getBuildingAgeAnalysis(),
        this.propertyRepository.getRoomCountStatistics(),
        this.propertyRepository.getActiveListings(10)
      ]);

      return {
        success: true,
        data: {
          districtPrices,
          ageAnalysis,
          roomStats,
          recentListings: activeListings
        }
      };
    } catch (error) {
      console.error('MarketService.getMarketOverview hatası:', error);
      return {
        success: false,
        error: error.message
      };
    }
  }
}

module.exports = MarketService;