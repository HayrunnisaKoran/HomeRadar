const db = require('../../../infrastructure-layer/database/dbConnection');

class PropertyRepository {
  async savePrediction(predictionData) {
    const query = `
      INSERT INTO predictions 
      (district, square_meters, rooms, predicted_price, user_id, predicted_at)
      VALUES ($1, $2, $3, $4, $5, $6)
      RETURNING *;
    `;
    
    const values = [
      predictionData.district,
      predictionData.square_meters,
      predictionData.rooms,
      predictionData.predicted_price,
      predictionData.userId || null,
      predictionData.predicted_at
    ];
    
    const result = await db.query(query, values);
    return result.rows[0];
  }

  async getMarketAnalysis() {
    // Üye 1'in yazdığı View'i kullan
    const query = `
      SELECT * FROM vw_market_analysis 
      ORDER BY average_price DESC;
    `;
    
    const result = await db.query(query);
    return result.rows;
  }

  async getPropertiesByDistrict(district) {
    // Stored Procedure kullanımı
    const query = 'CALL sp_get_properties_by_district($1)';
    const result = await db.query(query, [district]);
    return result.rows;
  }

  // CRUD işlemleri
  async getAllListings() {
    const query = 'SELECT * FROM listings WHERE is_active = true';
    const result = await db.query(query);
    return result.rows;
  }

  async createListing(listingData) {
    const query = `
      INSERT INTO listings 
      (title, description, price, district, square_meters, rooms, user_id)
      VALUES ($1, $2, $3, $4, $5, $6, $7)
      RETURNING *;
    `;
    
    const values = [
      listingData.title,
      listingData.description,
      listingData.price,
      listingData.district,
      listingData.square_meters,
      listingData.rooms,
      listingData.userId
    ];
    
    const result = await db.query(query, values);
    return result.rows[0];
  }
}

module.exports = new PropertyRepository();