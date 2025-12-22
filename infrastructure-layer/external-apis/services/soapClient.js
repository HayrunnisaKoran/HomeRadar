// SOAP Client - Döviz Kuru Servisi
const axios = require('axios');

class SoapCurrencyClient {
  constructor() {
    // TCMB SOAP URL (veya alternatif)
    this.url = 'http://www.tcmb.gov.tr/kurlar/today.xml';
    this.alternativeUrl = 'https://api.exchangerate-api.com/v4/latest/TRY';
  }

  async getExchangeRates() {
    try {
      console.log('🔍 SOAP/Döviz servisine bağlanıyor...');
      
      // 1. ÖNCE TCMB'yi dene (XML/SOAP benzeri)
      try {
        const tcmbResponse = await axios.get(this.url);
        console.log('✅ TCMB XML alındı');
        
        // Basit XML parse (gerçek projede XML parser kullan)
        return this.parseTCMBData(tcmbResponse.data);
      } catch (tcmbError) {
        console.log('⚠️ TCMB erişilemedi, alternatif API deneniyor...');
        
        // 2. ALTERNATİF: exchangerate-api (REST, ama SOAP mantığını göster)
        const exchangeResponse = await axios.get(this.alternativeUrl);
        
        return {
          success: true,
          source: 'exchangerate-api (SOAP simulation)',
          base: 'TRY',
          rates: {
            USD: exchangeResponse.data.rates.USD || 0.028,
            EUR: exchangeResponse.data.rates.EUR || 0.026,
            GBP: exchangeResponse.data.rates.GBP || 0.022
          },
          last_updated: new Date().toISOString(),
          note: 'SOAP simulation - Real SOAP service would use WSDL'
        };
      }
      
    } catch (error) {
      console.error('❌ SOAP/Döviz servisi hatası:', error.message);
      
      // 3. FALLBACK: Mock data
      return {
        success: true,
        source: 'Mock Data (SOAP simulation)',
        base: 'TRY',
        rates: {
          USD: 0.028,  // 1 TRY = 0.028 USD
          EUR: 0.026,  // 1 TRY = 0.026 EUR
          GBP: 0.022   // 1 TRY = 0.022 GBP
        },
        reverse_rates: {  // Emlak fiyatları için
          USD: 35.71,     // 1 USD = 35.71 TRY
          EUR: 38.46,     // 1 EUR = 38.46 TRY
          GBP: 45.45      // 1 GBP = 45.45 TRY
        },
        last_updated: new Date().toISOString(),
        note: 'Mock data - SOAP service simulation'
      };
    }
  }

  // Basit XML parsing (mock)
  parseTCMBData(xmlData) {
    // Gerçek projede XML parser kullanılır
    return {
      success: true,
      source: 'TCMB (SOAP-like XML)',
      base: 'TRY',
      rates: {
        USD: 0.028,
        EUR: 0.026,
        GBP: 0.022
      },
      currencies: [
        { code: 'USD', name: 'ABD DOLARI', forexBuying: 35.5, forexSelling: 35.7 },
        { code: 'EUR', name: 'EURO', forexBuying: 38.2, forexSelling: 38.4 },
        { code: 'GBP', name: 'İNGİLİZ STERLİNİ', forexBuying: 44.8, forexSelling: 45.0 }
      ],
      last_updated: new Date().toISOString(),
      note: 'XML parsing simulation for SOAP demonstration'
    };
  }

  // Emlak fiyatını dövize çevir
  async convertPropertyPrice(priceTRY, targetCurrency = 'USD') {
    const rates = await this.getExchangeRates();
    
    let convertedPrice;
    if (rates.reverse_rates && rates.reverse_rates[targetCurrency]) {
      convertedPrice = priceTRY / rates.reverse_rates[targetCurrency];
    } else if (rates.rates && rates.rates[targetCurrency]) {
      convertedPrice = priceTRY * rates.rates[targetCurrency];
    } else {
      // Default conversion rates
      const defaultRates = { USD: 0.028, EUR: 0.026, GBP: 0.022 };
      convertedPrice = priceTRY * (defaultRates[targetCurrency] || 0.028);
    }
    
    return {
      original: {
        price: priceTRY,
        currency: 'TRY'
      },
      converted: {
        price: Math.round(convertedPrice * 100) / 100,
        currency: targetCurrency,
        rate: rates.rates ? rates.rates[targetCurrency] : 'unknown'
      },
      source: rates.source,
      timestamp: new Date().toISOString()
    };
  }
}

module.exports = SoapCurrencyClient;