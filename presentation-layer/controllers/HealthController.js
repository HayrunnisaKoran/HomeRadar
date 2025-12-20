// presentation-layer/controllers/HealthController.js
const { pool } = require('../../infrastructure-layer/database/dbConnection');

class HealthController {
  async getHealth(req, res) {
    const healthChecks = {
      status: 'healthy',
      timestamp: new Date().toISOString(),
      service: 'SmartValue SOA API',
      version: '1.0.0',
      services: {}
    };

    try {
      // 1. PostgreSQL check
      const dbResult = await pool.query('SELECT NOW(), version()');
      healthChecks.services.database = {
        status: 'connected',
        timestamp: dbResult.rows[0].now,
        version: dbResult.rows[0].version.split(' ')[1],
        connection: 'pool active'
      };

      // 2. Tablo sayısı
      const tableCount = await pool.query(
        "SELECT COUNT(*) as count FROM information_schema.tables WHERE table_schema = 'public'"
      );
      healthChecks.services.database.table_count = parseInt(tableCount.rows[0].count);

      // 3. View sayısı
      const viewCount = await pool.query(
        "SELECT COUNT(*) as count FROM information_schema.views WHERE table_schema = 'public'"
      );
      healthChecks.services.database.view_count = parseInt(viewCount.rows[0].count);

      // 4. ML API check (simüle ediyoruz şimdilik)
      healthChecks.services.ml_api = {
        status: 'pending_integration',
        note: 'ML ekibi Flask API sağlayacak'
      };

      // 5. SOA katmanları durumu
      healthChecks.soa_layers = {
        presentation: 'active',
        application: 'active',
        domain: 'ready',
        infrastructure: 'partial',
        database: 'connected',
        external: 'pending'
      };

      res.json(healthChecks);

    } catch (error) {
      console.error('Health check error:', error);
      healthChecks.status = 'degraded';
      healthChecks.services.database = {
        status: 'error',
        error: error.message
      };
      res.status(500).json(healthChecks);
    }
  }
}

module.exports = HealthController;