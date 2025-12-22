const { query } = require('../../infrastructure-layer/database/dbConnection');

class PropertyRepository {
  
  // 1. İlçe listesini getir
  async getDistricts() {
    try {
      const result = await query('SELECT * FROM "Districts" ORDER BY "Name"');
      return result.rows;
    } catch (error) {
      throw new Error(`İlçe listesi alınamadı: ${error.message}`);
    }
  }

  // 2. Bina tiplerini getir
  async getBuildingTypes() {
    try {
      const result = await query('SELECT * FROM "BuildingTypes" ORDER BY "Name"');
      return result.rows;
    } catch (error) {
      throw new Error(`Bina tipleri alınamadı: ${error.message}`);
    }
  }

  // 3. View kullanarak ilçe ortalama fiyatları
  async getDistrictAveragePrices() {
    try {
      const result = await query('SELECT * FROM vw_district_avg_prices');
      return result.rows;
    } catch (error) {
      throw new Error(`İlçe ortalama fiyatları alınamadı: ${error.message}`);
    }
  }

  // 4. Stored Procedure ile ilanları getir
  async getListingsByCriteria(filters) {
    try {
      const {
        districtId = null,
        roomCount = null,
        minPrice = null,
        maxPrice = null,
        minSquareMeters = null,
        maxSquareMeters = null
      } = filters;

      const result = await query(
        'SELECT * FROM sp_get_listings_by_criteria($1, $2, $3, $4, $5, $6)',
        [districtId, roomCount, minPrice, maxPrice, minSquareMeters, maxSquareMeters]
      );
      return result.rows;
    } catch (error) {
      throw new Error(`İlanlar alınamadı: ${error.message}`);
    }
  }

  // 5. Yeni ilan ekle
  async createListing(listingData) {
    try {
      const {
        districtId,
        buildingTypeId,
        price,
        squareMeters,
        roomCount,
        salonCount,
        buildingAge,
        listingDate = new Date()
      } = listingData;

      const result = await query(
        `INSERT INTO "Listings" (
          "DistrictId", "BuildingTypeId", "Price", "SquareMeters",
          "RoomCount", "SalonCount", "BuildingAge", "ListingDate",
          "CreatedAt", "IsActive"
        ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, NOW(), true)
        RETURNING *`,
        [districtId, buildingTypeId, price, squareMeters, roomCount, salonCount, buildingAge, listingDate]
      );

      return result.rows[0];
    } catch (error) {
      throw new Error(`İlan eklenemedi: ${error.message}`);
    }
  }

  // 6. Stored Procedure ile tahmin ekle
  async createPrediction(predictionData) {
    try {
      const {
        userId,
        districtId,
        roomCount,
        squareMeters,
        buildingAge,
        buildingTypeId,
        predictedPriceMin,
        predictedPriceMax,
        modelName,
        confidenceScore
      } = predictionData;

      const result = await query(
        'SELECT sp_insert_prediction($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)',
        [userId, districtId, roomCount, squareMeters, buildingAge, 
         buildingTypeId, predictedPriceMin, predictedPriceMax, modelName, confidenceScore]
      );

      return result.rows[0].sp_insert_prediction; // Prediction ID döner
    } catch (error) {
      throw new Error(`Tahmin eklenemedi: ${error.message}`);
    }
  }

  // 7. View kullanarak kullanıcı tahmin geçmişi
  async getUserPredictions(userId) {
    try {
      const result = await query(
        'SELECT * FROM vw_user_predictions WHERE "UserId" = $1 ORDER BY "PredictionDate" DESC',
        [userId]
      );
      return result.rows;
    } catch (error) {
      throw new Error(`Kullanıcı tahminleri alınamadı: ${error.message}`);
    }
  }

  // 8. User Defined Function kullanımı
  async calculatePricePerSquareMeter(price, squareMeters) {
    try {
      const result = await query(
        'SELECT fn_calculate_price_per_square_meter($1, $2) AS price_per_sqm',
        [price, squareMeters]
      );
      return result.rows[0].price_per_sqm;
    } catch (error) {
      throw new Error(`Metrekare fiyatı hesaplanamadı: ${error.message}`);
    }
  }

  // 9. Aktif ilanları getir (View kullanarak)
  async getActiveListings() {
    try {
      const result = await query('SELECT * FROM vw_active_listings_detail');
      return result.rows;
    } catch (error) {
      throw new Error(`Aktif ilanlar alınamadı: ${error.message}`);
    }
  }
}

module.exports = new PropertyRepository();