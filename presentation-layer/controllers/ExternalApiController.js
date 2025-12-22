// 📄 presentation-layer/controllers/ExternalApiController.js
const googleMapsService = require('../../infrastructure-layer/external-apis/services/googleMapsService');
const SoapCurrencyClient = require('../../infrastructure-layer/external-apis/services/soapClient');
const grpcClient = require('../../infrastructure-layer/grpc/grpcClient');

const soapClient = new SoapCurrencyClient();

class ExternalApiController {
  // GOOGLE MAPS
  async getCoordinates(req, res) {
    try {
      const { address } = req.query;
      if (!address) throw new Error('Address parametresi gereklidir');
      
      const coordinates = await googleMapsService.getCoordinates(address);
      res.json({ success: true, ...coordinates });
    } catch (error) {
      res.status(500).json({ error: error.message });
    }
  }

  // SOAP - DÖVİZ KURU
  async getCurrency(req, res) {
    try {
      const rates = await soapClient.getExchangeRates();
      res.json({ success: true, protocol: 'SOAP', ...rates });
    } catch (error) {
      res.status(500).json({ error: 'SOAP servis hatası' });
    }
  }

  // SOAP - ÇEVİRİ
  async convertCurrency(req, res) {
    try {
      const { price, currency = 'USD' } = req.query;
      if (!price) throw new Error('Price parametresi gereklidir');
      
      const conversion = await soapClient.convertPropertyPrice(parseFloat(price), currency);
      res.json({ success: true, ...conversion });
    } catch (error) {
      res.status(500).json({ error: error.message });
    }
  }

  // gRPC - TAHMİN (EKSİK OLAN FONKSİYON)
  async predictPrice(req, res) {
    try {
      const { district, rooms, square_meters, building_age } = req.body;
      
      if (!district || !rooms || !square_meters || !building_age) {
        throw new Error('Tüm alanlar gereklidir: district, rooms, square_meters, building_age');
      }
      
      const prediction = await grpcClient.predict({
        district,
        rooms,
        square_meters: parseFloat(square_meters),
        building_age: parseInt(building_age)
      });
      
      res.json({ 
        success: true, 
        protocol: 'gRPC',
        prediction 
      });
    } catch (error) {
      console.error('gRPC Error:', error);
      res.status(500).json({ error: 'gRPC servis hatası', details: error.message });
    }
  }

  // DURUM ENDPOINT'LERİ
  getSoapStatus(req, res) {
    res.json({
      soap: {
        service: 'Currency Exchange',
        status: 'active',
        type: 'SOAP/XML Simulation',
        endpoints: [
          'GET /api/currency',
          'GET /api/convert?price=500000&currency=USD'
        ]
      }
    });
  }

  getGrpcStatus(req, res) {
    res.json({
      gRPC: {
        server: 'localhost:50051',
        status: 'connected',
        protocol: 'gRPC',
        endpoints: [
          'POST /api/predict'
        ]
      }
    });
  }

  getMapsStatus(req, res) {
    res.json({
      google_maps: {
        service: 'Geocoding API',
        status: 'active',
        api_key: process.env.GOOGLE_MAPS_API_KEY ? 'configured' : 'missing',
        endpoints: [
          'GET /api/coordinates?address=Manisa'
        ]
      }
    });
  }

  // TEKNİK BİLGİLER
  getTechInfo(req, res) {
    res.json({
      architecture: 'SOA (Service-Oriented Architecture)',
      status: 'Phase 2 - 6 Layers Complete',
      services: {
        google_maps: 'active',
        soap_currency: 'active',
        grpc_prediction: 'active'
      },
      layers: {
        presentation: 'active',
        application: 'active',
        domain: 'active',
        infrastructure: 'active',
        data: 'mock',
        common: 'active'
      }
    });
  }
}

module.exports = new ExternalApiController();