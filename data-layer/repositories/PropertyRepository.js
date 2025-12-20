// data-layer/repositories/PropertyRepository.js
const { pool } = require('../../infrastructure-layer/database/dbConnection');

class PropertyRepository {
  // 1. View kullanarak ilçe istatistikleri
  async getDistrictAveragePrices() {
    try {
      const result = await pool.query('SELECT * FROM vw_district_avg_prices ORDER BY "DistrictName"');
      return result.rows;
    } catch (error) {
      console.error('❌ getDistrictAveragePrices hatası:', error.message);
      throw error;
    }
  }

  // 2. Stored Procedure kullanarak filtreli ilan getirme
  async getListingsByCriteria(criteria = {}) {
    try {
      const { 
        districtId, 
        roomCount, 
        minPrice, 
        maxPrice, 
        minSquareMeters, 
        maxSquareMeters 
      } = criteria;
      
      const result = await pool.query(
        'SELECT * FROM sp_get_listings_by_criteria($1, $2, $3, $4, $5, $6)',
        [
          districtId || null, 
          roomCount || null, 
          minPrice || null, 
          maxPrice || null, 
          minSquareMeters || null, 
          maxSquareMeters || null
        ]
      );
      
      return result.rows;
    } catch (error) {
      console.error('❌ getListingsByCriteria hatası:', error.message);
      throw error;
    }
  }

  // 3. User Defined Function kullanarak tahmin
  async estimatePrice(districtId, roomCount, squareMeters, buildingAge) {
    try {
      const result = await pool.query(
        'SELECT fn_estimate_price_by_district($1, $2, $3, $4) as estimated_price',
        [districtId, roomCount, squareMeters, buildingAge]
      );
      
      return result.rows[0]?.estimated_price;
    } catch (error) {
      console.error('❌ estimatePrice hatası:', error.message);
      throw error;
    }
  }

  // 4. Aktif ilanları getir (View kullanarak)
  async getActiveListings(limit = 50) {
    try {
      const result = await pool.query(
        'SELECT * FROM vw_active_listings_detail LIMIT $1',
        [limit]
      );
      return result.rows;
    } catch (error) {
      console.error('❌ getActiveListings hatası:', error.message);
      throw error;
    }
  }

  // 5. Tahmin ekle (Stored Procedure ile)
  async insertPrediction(predictionData) {
    try {
      const result = await pool.query(
        'SELECT sp_insert_prediction($1, $2, $3, $4, $5, $6, $7, $8, $9, $10) as prediction_id',
        [
          predictionData.userId,
          predictionData.districtId,
          predictionData.roomCount,
          predictionData.squareMeters,
          predictionData.buildingAge,
          predictionData.buildingTypeId || null,
          predictionData.predictedPriceMin,
          predictionData.predictedPriceMax,
          predictionData.modelName || 'LinearRegression',
          predictionData.confidenceScore || null
        ]
      );
      
      return result.rows[0]?.prediction_id;
    } catch (error) {
      console.error('❌ insertPrediction hatası:', error.message);
      throw error;
    }
  }

  // 6. Kullanıcı tahmin geçmişi (View ile)
  async getUserPredictions(userId) {
    try {
      const result = await pool.query(
        'SELECT * FROM vw_user_predictions WHERE "UserId" = $1 ORDER BY "PredictionDate" DESC',
        [userId]
      );
      return result.rows;
    } catch (error) {
      console.error('❌ getUserPredictions hatası:', error.message);
      throw error;
    }
  }

  // 7. Bina yaşı analizi (View ile)
  async getBuildingAgeAnalysis() {
    try {
      const result = await pool.query('SELECT * FROM vw_building_age_price_analysis');
      return result.rows;
    } catch (error) {
      console.error('❌ getBuildingAgeAnalysis hatası:', error.message);
      throw error;
    }
  }

  // 8. Oda sayısı istatistikleri (View ile)
  async getRoomCountStatistics() {
    try {
      const result = await pool.query('SELECT * FROM vw_room_count_statistics ORDER BY "RoomCount", "SalonCount"');
      return result.rows;
    } catch (error) {
      console.error('❌ getRoomCountStatistics hatası:', error.message);
      throw error;
    }
  }

  // 9. Metrekare başına fiyat hesapla (Function ile)
  async calculatePricePerSquareMeter(price, squareMeters) {
    try {
      const result = await pool.query(
        'SELECT fn_calculate_price_per_square_meter($1, $2) as price_per_sqm',
        [price, squareMeters]
      );
      return result.rows[0]?.price_per_sqm;
    } catch (error) {
      console.error('❌ calculatePricePerSquareMeter hatası:', error.message);
      throw error;
    }
  }

  // 10. Temel veri sayıları
  async getDatabaseStats() {
    try {
      const result = await pool.query(`
        SELECT 'Users' as table_name, COUNT(*) as record_count FROM "Users"
        UNION ALL
        SELECT 'Listings', COUNT(*) FROM "Listings"
        UNION ALL
        SELECT 'Predictions', COUNT(*) FROM "Predictions"
        UNION ALL
        SELECT 'Districts', COUNT(*) FROM "Districts"
        UNION ALL
        SELECT 'BuildingTypes', COUNT(*) FROM "BuildingTypes"
      `);
      return result.rows;
    } catch (error) {
      console.error('❌ getDatabaseStats hatası:', error.message);
      throw error;
    }
  }
}

module.exports = PropertyRepository;