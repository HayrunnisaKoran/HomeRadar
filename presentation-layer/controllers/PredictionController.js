const PredictionService = require('../../application-layer/services/PredictionService');
const logger = require('../../common/middleware/logger');

class PredictionController {
    static async predictPrice(req, res, next) {
        try {
            // 1. Gelen veriyi al ve formatla
            const inputData = {
                district: req.body.district,
                square_meters: parseFloat(req.body.square_meters),
                rooms: parseInt(req.body.rooms),
                living_rooms: parseInt(req.body.living_rooms || 1),
                building_age: parseInt(req.body.building_age),
                bathrooms: parseInt(req.body.bathrooms || 1),
                floor: parseInt(req.body.floor || 0),
                heating: req.body.heating ? 1 : 0, // bool → 1/0
                elevator: req.body.elevator ? 1 : 0,
                garage: req.body.garage ? 1 : 0,
                balcony: req.body.balcony ? 1 : 0,
                furnished: req.body.furnished ? 1 : 0,
                swap: req.body.swap ? 1 : 0,
                usage_status: req.body.usage_status || "Owner",
                building_status: req.body.building_status ? 1 : 0,
                title_deed: req.body.title_deed ? 1 : 0
            };
            
            logger.info(`Tahmin isteği alındı - İlçe: ${inputData.district}, m2: ${inputData.square_meters}`);
            
            // 2. ML servisini çağır (Python scripti)
            const mlResult = await PredictionService.predict(inputData);
            
            // 3. Yanıtı döndür
            res.json({
                success: true,
                prediction: mlResult.prediction || mlResult, // Python'dan direkt gelen sonuç
                similarListings: mlResult.similarListings || [],
                confidenceFactors: mlResult.confidenceFactors || {},
                timestamp: new Date(),
                source: mlResult.status === 'mock' ? 'mock_calculation' : 'ml_model'
            });
            
        } catch (error) {
            logger.error('Tahmin hatası:', error);
            next(error);
        }
    }
}

module.exports = PredictionController;