// infrastructure-layer/database/dbConnection.js
const { Pool } = require('pg');

const pool = new Pool({
  host: 'localhost',
  port: 5432,
  database: 'homeradar_db',
  user: 'homeradar_app_user',
  password: 'HomeRadar2024!SecurePass',
  max: 20,
  idleTimeoutMillis: 30000,
});

module.exports = { pool };