const MLService = require('../../../infrastructure-layer/external-apis/services/mlService');
const PropertyRepository = require('../../../data-layer/repositories/PropertyRepository');

class PredictionController {
  async predict(req, res) {
    try {
      const propertyData = req.body;
      
      // 1. Validasyon (basit)
      if (!propertyData.district || !propertyData.square_meters || !propertyData.rooms) {
        return res.status(400).json({
          success: false,
          error: 'Lütfen ilçe, metrekare ve oda sayısını giriniz'
        });
      }

      // 2. ML API'den tahmini al
      const prediction = await MLService.getPrediction(propertyData);
      
      // 3. Tahmini veritabanına kaydet (log)
      if (prediction.success) {
        const savedPrediction = await PropertyRepository.savePrediction({
          ...propertyData,
          predicted_price: prediction.price,
          predicted_at: new Date()
        });
        
        // 4. Kullanıcıya tahmin log ID'si de döndür
        prediction.predictionId = savedPrediction.id;
      }

      // 5. Sonucu döndür
      res.json(prediction);
      
    } catch (error) {
      console.error('Prediction error:', error);
      res.status(500).json({
        success: false,
        error: 'Tahmin işlemi sırasında bir hata oluştu'
      });
    }
  }

  async getMarketAnalysis(req, res) {
    try {
      // Veritabanından piyasa analizi verilerini çek
      const analysis = await PropertyRepository.getMarketAnalysis();
      
      res.json({
        success: true,
        data: analysis,
        timestamp: new Date()
      });
    } catch (error) {
      res.status(500).json({ success: false, error: error.message });
    }
  }
}

module.exports = new PredictionController();