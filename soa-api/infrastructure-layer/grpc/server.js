// MOCK gRPC SERVER - Python'u beklemeden!
const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');
const path = require('path');

// Proto dosyasını yükle
const PROTO_PATH = path.join(__dirname, 'protos/prediction.proto');
const packageDefinition = protoLoader.loadSync(PROTO_PATH);
const predictionProto = grpc.loadPackageDefinition(packageDefinition).smartvalue;

// MOCK ML HESAPLAMASI
function mockMLPrediction(data) {
  const { district, rooms, square_meters, building_age } = data;
  
  // Basit mock algoritma
  const base = 8000;
  const districtFactor = {
    'yunusemre': 1.3,
    'şehzadeler': 1.2,
    'akhisar': 1.0
  }[district.toLowerCase()] || 1.0;
  
  const roomFactor = {
    '1+1': 0.8,
    '2+1': 1.0,
    '3+1': 1.4,
    '4+1': 1.8
  }[rooms] || 1.0;
  
  const estimated = base * districtFactor * roomFactor * (square_meters / 80) * (1 - building_age * 0.01);
  
  return {
    min_price: Math.round(estimated * 0.9),
    max_price: Math.round(estimated * 1.1),
    confidence: 0.85,
    model_version: 'mock-v1.0'
  };
}

// gRPC Servisini tanımla
const server = new grpc.Server();

server.addService(predictionProto.PredictionService.service, {
  Predict: (call, callback) => {
    console.log('📡 gRPC Request received:', call.request);
    
    // MOCK tahmini yap
    const prediction = mockMLPrediction(call.request);
    
    // Yanıt gönder
    callback(null, prediction);
  }
});

// Server'ı başlat
const PORT = '50051';
server.bindAsync(
  `0.0.0.0:${PORT}`,
  grpc.ServerCredentials.createInsecure(),
  (error, port) => {
    if (error) {
      console.error('gRPC Server error:', error);
      return;
    }
    console.log(`
    ====================================
    🚀 MOCK gRPC SERVER ÇALIŞIYOR
    📡 Port: ${PORT}
    🎯 Python ML servisi yerine MOCK
    ====================================
    `);
  }
);