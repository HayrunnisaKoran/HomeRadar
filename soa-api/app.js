const express = require('express');
const cors = require('cors');
require('dotenv').config();

// Katmanlar
const apiRoutes = require('./presentation-layer/routers/apiRoutes');
const errorHandler = require('./common/middleware/errorHandler');
const logger = require('./common/middleware/logger');

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware'ler
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Basit logger middleware (eğer yoksa)
app.use((req, res, next) => {
  console.log(`${new Date().toISOString()} - ${req.method} ${req.url}`);
  next();
});

// Routes
app.use('/api/v1', apiRoutes);

// Health check
app.get('/health', (req, res) => {
  res.json({ 
    status: 'healthy', 
    timestamp: new Date(),
    services: {
      database: 'checking',
      ml_api: 'checking'
    }
  });
});

// Ana sayfa
app.get('/', (req, res) => {
  res.send(`
    <h1>SmartValue SOA API</h1>
    <p>API Endpoints:</p>
    <ul>
      <li><a href="/health">Health Check</a></li>
      <li>POST /api/v1/predict</li>
      <li>GET /api/v1/market/analysis</li>
    </ul>
  `);
});

// Error handling middleware
app.use((err, req, res, next) => {
  console.error('Error:', err);
  res.status(500).json({
    success: false,
    error: err.message || 'Internal server error'
  });
});

// 404 handler
app.use((req, res) => {
  res.status(404).json({
    success: false,
    error: 'Route not found'
  });
});

// Server başlatma
app.listen(PORT, () => {
  console.log(`🚀 SOA API server ${PORT} portunda çalışıyor`);
  console.log(`📊 Health check: http://localhost:${PORT}/health`);
  console.log(`🏠 Ana sayfa: http://localhost:${PORT}`);
});