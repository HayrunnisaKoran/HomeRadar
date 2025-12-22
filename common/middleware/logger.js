const fs = require('fs');
const path = require('path');

const logDir = path.join(__dirname, '../../logs');
if (!fs.existsSync(logDir)) {
  fs.mkdirSync(logDir, { recursive: true });
}

// Logger middleware (Express için)
const loggerMiddleware = (req, res, next) => {
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

// Logger utility (Controller'larda kullanmak için)
const logger = {
  info: (message, ...args) => {
    const timestamp = new Date().toISOString();
    console.log(`[INFO] [${timestamp}] ${message}`, ...args);
    const logFile = path.join(logDir, 'api.log');
    fs.appendFileSync(logFile, `[INFO] [${timestamp}] ${message} ${args.length > 0 ? JSON.stringify(args) : ''}\n`);
  },
  
  error: (message, error) => {
    const timestamp = new Date().toISOString();
    console.error(`[ERROR] [${timestamp}] ${message}`, error);
    const logFile = path.join(logDir, 'api.log');
    const errorStr = error instanceof Error ? error.stack : JSON.stringify(error);
    fs.appendFileSync(logFile, `[ERROR] [${timestamp}] ${message} ${errorStr}\n`);
  },
  
  warn: (message, ...args) => {
    const timestamp = new Date().toISOString();
    console.warn(`[WARN] [${timestamp}] ${message}`, ...args);
    const logFile = path.join(logDir, 'api.log');
    fs.appendFileSync(logFile, `[WARN] [${timestamp}] ${message} ${args.length > 0 ? JSON.stringify(args) : ''}\n`);
  },
  
  debug: (message, ...args) => {
    const timestamp = new Date().toISOString();
    console.debug(`[DEBUG] [${timestamp}] ${message}`, ...args);
    const logFile = path.join(logDir, 'api.log');
    fs.appendFileSync(logFile, `[DEBUG] [${timestamp}] ${message} ${args.length > 0 ? JSON.stringify(args) : ''}\n`);
  }
};

// Utility logger'ı export et (Controller'larda kullanılacak)
module.exports = logger;

// Middleware'i de ayrı export et (isteğe bağlı)
module.exports.middleware = loggerMiddleware;