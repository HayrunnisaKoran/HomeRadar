// 📄 presentation-layer/controllers/HealthController.js
class HealthController {
  // Sağlık kontrolü
  static checkHealth(req, res) {
    const healthInfo = {
      status: 'çalışıyor',
      timestamp: new Date().toISOString(),
      uptime: `${Math.floor(process.uptime())} saniye`,
      memory: `${(process.memoryUsage().heapUsed / 1024 / 1024).toFixed(2)} MB`,
      environment: process.env.NODE_ENV || 'development'
    };
    res.json(healthInfo);
  }
  
  // Katman durumu
  static getLayerStatus(req, res) {
    res.json({
      layers: [
        {
          ad: 'Presentation Layer',
          durum: '✅ aktif',
          açıklama: 'API endpointlerini sunar',
          dosya: 'presentation-layer/controllers/'
        },
        {
          ad: 'Application Layer',
          durum: '🔄 geliştiriliyor',
          açıklama: 'İş mantığını yönetir',
          dosya: 'application-layer/services/'
        },
        {
          ad: 'Domain Layer',
          durum: '⏳ bekleniyor',
          açıklama: 'İş kurallarını tanımlar',
          dosya: 'domain-layer/models/'
        },
        {
          ad: 'Infrastructure Layer',
          durum: '✅ aktif',
          açıklama: 'Dış servislere bağlanır',
          dosya: 'infrastructure-layer/'
        },
        {
          ad: 'Data Layer',
          durum: '⏳ bekleniyor',
          açıklama: 'Veritabanına erişir',
          dosya: 'data-layer/repositories/'
        },
        {
          ad: 'Common Layer',
          durum: '✅ aktif',
          açıklama: 'Ortak araçları sağlar',
          dosya: 'common/middleware/'
        }
      ]
    });
  }
  
  // Proje bilgisi
  static getProjectInfo(req, res) {
    res.json({
      proje_adi: 'SmartValue',
      takım: '5 kişi',
      amaç: 'Manisa\'da emlak fiyat tahmini',
      dersler: ['SOA', 'Veritabanı', 'İleri Web', 'Makine Öğrenmesi'],
      github: 'https://github.com/FilizKalmis/emlak-fiyat-tahmini-uygulamasi'
    });
  }
}

module.exports = HealthController;