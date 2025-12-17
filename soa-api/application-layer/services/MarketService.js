// BU KODU KOPYALA
const propertyRepository = require('../../data-layer/repositories/PropertyRepository');

class MarketService {
  
  // İlçe bazlı piyasa analizi
  async getMarketAnalysis() {
    try {
      // 1. İlçe ortalamalarını al (C#'ın View'inden)
      const districtAverages = await propertyRepository.getDistrictAverages();
      
      // 2. Oda sayısı istatistikleri (C#'ın View'inden)
      const roomStatistics = await propertyRepository.getRoomStatistics();
      
      // 3. Aktif ilanları getir
      const activeListings = await propertyRepository.getAllActiveListings();
      
      return {
        districtAverages,
        roomStatistics,
        activeListingsCount: activeListings.length,
        lastUpdated: new Date().toISOString()
      };
    } catch (error) {
      console.error('Piyasa analizi hatası:', error);
      throw error;
    }
  }

  // İlçeye göre ortalama fiyat
  async getAveragePriceByDistrict(districtName) {
    try {
      const query = `
        SELECT * FROM vw_district_avg_prices 
        WHERE "DistrictName" = $1
      `;
      const db = require('../../infrastructure-layer/database/dbConnection');
      const result = await db.query(query, [districtName]);
      
      if (result.rows.length === 0) {
        return null;
      }
      
      return result.rows[0];
    } catch (error) {
      console.error('İlçe fiyat sorgusu hatası:', error);
      throw error;
    }
  }

  // Filtreli ilan arama
  async searchListings(filters) {
    try {
      const listings = await propertyRepository.getListingsByCriteria(filters);
      return listings;
    } catch (error) {
      console.error('İlan arama hatası:', error);
      throw error;
    }
  }
}

// Tek instance oluştur ve dışarı aktar
const marketService = new MarketService();
module.exports = marketService;