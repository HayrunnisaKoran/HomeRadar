// BU KODU KOPYALA
const marketService = require('../../application-layer/services/MarketService');

class MarketController {
  
  // Tüm piyasa analizini getir
  async getMarketAnalysis(req, res) {
    try {
      const analysis = await marketService.getMarketAnalysis();
      res.json({
        success: true,
        data: analysis,
        message: 'Piyasa analizi başarıyla getirildi'
      });
    } catch (error) {
      res.status(500).json({
        success: false,
        error: error.message,
        message: 'Piyasa analizi alınırken hata oluştu'
      });
    }
  }

  // İlçeye göre analiz
  async getDistrictAnalysis(req, res) {
    try {
      const { district } = req.params;
      
      if (!district) {
        return res.status(400).json({
          success: false,
          message: 'İlçe adı gereklidir'
        });
      }
      
      const districtData = await marketService.getAveragePriceByDistrict(district);
      
      if (!districtData) {
        return res.status(404).json({
          success: false,
          message: 'İlçe bulunamadı'
        });
      }
      
      res.json({
        success: true,
        data: districtData
      });
    } catch (error) {
      res.status(500).json({
        success: false,
        error: error.message
      });
    }
  }

  // Filtreli ilan arama
  async searchListings(req, res) {
    try {
      const filters = req.query; // Örnek: ?districtId=1&roomCount=3&minPrice=300000
      
      const listings = await marketService.searchListings(filters);
      
      res.json({
        success: true,
        count: listings.length,
        data: listings
      });
    } catch (error) {
      res.status(500).json({
        success: false,
        error: error.message
      });
    }
  }
}

// Tek instance oluştur ve dışarı aktar
const marketController = new MarketController();
module.exports = marketController;