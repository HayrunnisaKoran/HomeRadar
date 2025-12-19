// 📄 common/middleware/auth.js
const authenticate = (req, res, next) => {
  const apiKey = req.headers['x-api-key'];
  
  // Basit kontrol (gerçek projede JWT kullan)
  const validKeys = {
    'admin-key-123': 'admin',
    'user-key-456': 'user',
    'smartvalue-2024': 'user' // default key
  };
  
  if (apiKey && validKeys[apiKey]) {
    req.userRole = validKeys[apiKey];
    next();
  } else {
    res.status(401).json({
      success: false,
      error: 'Unauthorized. Valid API key required.',
      hint: 'Use header: x-api-key: smartvalue-2024'
    });
  }
};

const authorize = (roles) => {
  return (req, res, next) => {
    if (roles.includes(req.userRole)) {
      next();
    } else {
      res.status(403).json({
        success: false,
        error: 'Forbidden. Insufficient permissions.',
        requiredRoles: roles,
        yourRole: req.userRole
      });
    }
  };
};

module.exports = { authenticate, authorize };