const express = require('express');
const router = express.Router();
const marketController = require('../controllers/MarketController');
const predictionController = require('../controllers/PredictionController');

// Market Routes
router.get('/market/listings', marketController.getListings);
router.post('/market/estimate', marketController.estimatePrice);
router.get('/market/statistics', marketController.getStatistics);
router.get('/market/districts', marketController.getDistricts);
router.get('/market/building-types', marketController.getBuildingTypes);
router.post('/market/listings', marketController.createListing);
router.get('/market/predictions/:userId', marketController.getUserPredictions);

// Prediction Routes (ML ile entegrasyon)
router.post('/predict', predictionController.predictPrice);

// Health check
router.get('/health', (req, res) => {
  res.json({ status: 'OK', database: 'connected', timestamp: new Date() });
});

module.exports = router;