const express = require('express');
const router = express.Router();
const PredictionController = require('../controllers/PredictionController');
const MarketController = require('../controllers/MarketController');
const ExternalApiController = require('../controllers/ExternalApiController');

// Auth middleware (basit)
const authenticate = (req, res, next) => {
  const token = req.headers.authorization;
  if (!token) {
    return res.status(401).json({ error: 'Yetkisiz erişim' });
  }
  next();
};

// Tahmin endpoint'i
router.post('/predict', authenticate, PredictionController.predict);

// Piyasa analizi
router.get('/market/analysis', MarketController.getAnalysis);

// Google Maps entegrasyonu
router.get('/locations', ExternalApiController.getLocations);

// SOAP servisi (döviz kuru)
router.get('/exchange-rates', ExternalApiController.getExchangeRates);

// gRPC test endpoint'i
router.get('/grpc-test', ExternalApiController.testGrpc);

module.exports = router;