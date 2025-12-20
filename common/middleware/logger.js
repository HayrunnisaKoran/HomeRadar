const fs = require('fs');
const path = require('path');

const logDir = path.join(__dirname, '../../logs');
if (!fs.existsSync(logDir)) {
  fs.mkdirSync(logDir, { recursive: true });
}

const logger = (req, res, next) => {
  const logEntry = {
    timestamp: new Date().toISOString(),
    method: req.method,
    url: req.url,
    ip: req.ip,
    userAgent: req.get('User-Agent')
  };
  
  console.log(`[${logEntry.timestamp}] ${logEntry.method} ${logEntry.url}`);
  
  // Dosyaya log kaydet
  const logFile = path.join(logDir, 'api.log');
  fs.appendFileSync(logFile, JSON.stringify(logEntry) + '\n');
  
  next();
};

module.exports = logger;