const PredictionService = require('../../application-layer/services/PredictionService');
const logger = require('../../common/middleware/logger');

class PredictionController {
    static async predictPrice(req, res, next) {
        try {
            // DEBUG: Raw body'yi logla
            logger.info(`[DEBUG] PredictionController - Raw body:`, JSON.stringify(req.body, null, 2));
            
            // 1. Gelen veriyi al ve formatla
            const squareMeters = parseFloat(req.body.square_meters);
            const rooms = parseInt(req.body.rooms);
            const buildingAge = parseInt(req.body.building_age);
            
            // DEBUG: Parse sonuçlarını logla
            logger.info(`[DEBUG] PredictionController - Parse sonuçları:`);
            logger.info(`  square_meters: ${req.body.square_meters} -> ${squareMeters} (isNaN: ${isNaN(squareMeters)})`);
            logger.info(`  rooms: ${req.body.rooms} -> ${rooms} (isNaN: ${isNaN(rooms)})`);
            logger.info(`  building_age: ${req.body.building_age} -> ${buildingAge} (isNaN: ${isNaN(buildingAge)})`);
            logger.info(`  district: '${req.body.district}' (type: ${typeof req.body.district})`);
            
            // NaN kontrolü - geçersiz değerler için hata fırlat
            if (isNaN(squareMeters) || isNaN(rooms) || isNaN(buildingAge)) {
                logger.error(`[DEBUG] PredictionController - NaN hatası tespit edildi!`);
                return res.status(400).json({
                    success: false,
                    error: 'Geçersiz veri formatı: square_meters, rooms ve building_age sayısal değer olmalıdır'
                });
            }
            
            const inputData = {
                district: req.body.district || '',
                square_meters: squareMeters,
                rooms: rooms,
                living_rooms: parseInt(req.body.living_rooms || 1),
                building_age: buildingAge,
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
            
            // DEBUG: Formatlanmış inputData'yı logla
            logger.info(`[DEBUG] PredictionController - Formatlanmış inputData:`, JSON.stringify(inputData, null, 2));
            logger.info(`Tahmin isteği alındı - İlçe: ${inputData.district}, m2: ${inputData.square_meters}, Oda: ${inputData.rooms}, Yaş: ${inputData.building_age}`);
            
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
            
            // Hata mesajını kullanıcı dostu formata çevir
            let errorMessage = error.message || 'Tahmin yapılırken bir hata oluştu';
            let statusCode = 500;
            
            // Validasyon hataları için 400 Bad Request
            if (error.message && (
                error.message.includes('gereklidir') || 
                error.message.includes('required') ||
                error.message.includes('Geçersiz')
            )) {
                statusCode = 400;
            }
            
            return res.status(statusCode).json({
                success: false,
                error: errorMessage
            });
        }
    }
}

module.exports = PredictionController;