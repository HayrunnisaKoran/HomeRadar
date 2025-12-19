const marketService = require('../../application-layer/services/MarketService');
const propertyRepository = require('../../data-layer/repositories/PropertyRepository');

class MarketController {
  
  // GET /api/market/listings
  async getListings(req, res) {
    try {
      const filters = {
        districtId: req.query.districtId,
        roomCount: req.query.roomCount,
        minPrice: req.query.minPrice,
        maxPrice: req.query.maxPrice,
        minSquareMeters: req.query.minSqm,
        maxSquareMeters: req.query.maxSqm,
        includePrediction: req.query.predict === 'true'
      };
      
      const listings = await marketService.searchProperties(filters);
      
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

  // POST /api/market/estimate
  async estimatePrice(req, res) {
    try {
      const {
        districtId,
        roomCount,
        squareMeters,
        buildingAge,
        buildingTypeId,
        userId
      } = req.body;
      
      const predictionRequest = {
        userId: userId || 1, // Demo için
        districtId,
        roomCount,
        squareMeters,
        buildingAge,
        buildingTypeId
      };
      
      const result = await marketService.estimatePrice(predictionRequest);
      
      res.json({
        success: true,
        ...result
      });
    } catch (error) {
      res.status(500).json({
        success: false,
        error: error.message
      });
    }
  }

  // GET /api/market/statistics
  async getStatistics(req, res) {
    try {
      const statistics = await marketService.getMarketStatistics();
      
      res.json({
        success: true,
        ...statistics
      });
    } catch (error) {
      res.status(500).json({
        success: false,
        error: error.message
      });
    }
  }

  // GET /api/market/districts
  async getDistricts(req, res) {
    try {
      const districts = await propertyRepository.getDistricts();
      
      res.json({
        success: true,
        count: districts.length,
        data: districts
      });
    } catch (error) {
      res.status(500).json({
        success: false,
        error: error.message
      });
    }
  }

  // GET /api/market/building-types
  async getBuildingTypes(req, res) {
    try {
      const buildingTypes = await propertyRepository.getBuildingTypes();
      
      res.json({
        success: true,
        count: buildingTypes.length,
        data: buildingTypes
      });
    } catch (error) {
      res.status(500).json({
        success: false,
        error: error.message
      });
    }
  }

  // POST /api/market/listings
  async createListing(req, res) {
    try {
      const result = await marketService.addNewProperty(req.body);
      
      res.status(201).json(result);
    } catch (error) {
      res.status(400).json({
        success: false,
        error: error.message
      });
    }
  }

  // GET /api/market/predictions/:userId
  async getUserPredictions(req, res) {
    try {
      const { userId } = req.params;
      const predictions = await propertyRepository.getUserPredictions(userId);
      
      res.json({
        success: true,
        count: predictions.length,
        data: predictions
      });
    } catch (error) {
      res.status(500).json({
        success: false,
        error: error.message
      });
    }
  }
}

module.exports = new MarketController();