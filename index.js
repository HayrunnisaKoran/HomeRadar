const express = require("express");
const app = express();

app.use(express.json());

app.post("/api/predict", (req, res) => {
  res.json({
    min: 17500,
    max: 19000,
    message: "SOA API çalışıyor (Mock Tahmin)"
  });
});

app.listen(3000, () => {
  console.log("SOA API 3000 portunda çalışıyor");
});


// app.js veya index.js'de
const googleMapsService = require('./infrastructure-layer/external-apis/services/googleMapsService');
const SoapCurrencyClient = require('./infrastructure-layer/external-apis/services/soapClient');

// Kullanım
const soapClient = new SoapCurrencyClient();

async function testServices() {
    // Google Maps test
    const coords = await googleMapsService.getDistrictCoordinates('Yunusemre');
    console.log('Koordinatlar:', coords);
    
    // SOAP/Döviz test
    const rates = await soapClient.getExchangeRates();
    console.log('Döviz Kurları:', rates);
}