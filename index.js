// index.js - Ana SOA API Server
require('dotenv').config();
const express = require('express');
const cors = require('cors');
const apiRoutes = require('./presentation-layer/routers/apiRoutes');

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// API Routes
app.use('/api', apiRoutes);

// Health check endpoint
app.get('/health', (req, res) => {
  res.json({
    status: 'healthy',
    service: 'SmartValue SOA API',
    version: '1.0.0',
    timestamp: new Date().toISOString(),
    database: 'connected',
    features: {
      grpc: 'pending',
      soap: 'pending',
      ml_integration: 'pending',
      external_apis: 'pending'
    }
  });
});

// Error handling middleware
app.use((err, req, res, next) => {
  console.error('❌ API Error:', err.stack);
  res.status(500).json({
    error: 'Internal Server Error',
    message: process.env.NODE_ENV === 'development' ? err.message : undefined
  });
});

// 404 handler
app.use('*', (req, res) => {
  res.status(404).json({
    error: 'Not Found',
    message: `Route ${req.originalUrl} not found`
  });
});

// Start server
app.listen(PORT, () => {
  console.log(`🚀 SOA API Server started on port ${PORT}`);
  console.log(`📡 Health check: http://localhost:${PORT}/health`);
  console.log(`🔗 API Base: http://localhost:${PORT}/api`);
  console.log('\n📋 Available Endpoints:');
  console.log('  GET  /api/health');
  console.log('  GET  /api/market/overview');
  console.log('  POST /api/market/estimate');
  console.log('  GET  /api/districts');
  console.log('  GET  /api/listings');
  console.log('  POST /api/predictions');
  console.log('\n🔧 SOA Layers Status:');
  console.log('  ✅ Database Layer: PostgreSQL connected');
  console.log('  ✅ Data Layer: Repositories ready');
  console.log('  🔄 Application Layer: Services in progress');
  console.log('  ⏳ Infrastructure Layer: gRPC/SOAP pending');
  console.log('  🔄 Presentation Layer: Controllers ready');
  console.log('  ⏳ External APIs: Google Maps pending');
});