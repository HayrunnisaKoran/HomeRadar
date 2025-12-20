// 📄 domain-layer/models/Property.js
class Property {
  constructor(data) {
    this.id = data.id;
    this.district = data.district;
    this.rooms = data.rooms;
    this.squareMeters = data.squareMeters;
    this.buildingAge = data.buildingAge;
    this.price = data.price;
    this.type = data.type || 'Daire';
    this.latitude = data.latitude;
    this.longitude = data.longitude;
  }
  
  validate() {
    const errors = [];
    
    if (!this.district) errors.push('İlçe gereklidir');
    if (!this.rooms) errors.push('Oda sayısı gereklidir');
    if (this.squareMeters < 20) errors.push('Metrekare en az 20 olmalıdır');
    if (this.squareMeters > 500) errors.push('Metrekare en fazla 500 olabilir');
    if (this.buildingAge < 0) errors.push('Bina yaşı negatif olamaz');
    if (this.buildingAge > 100) errors.push('Bina yaşı en fazla 100 olabilir');
    if (this.price && this.price < 0) errors.push('Fiyat negatif olamaz');
    
    return {
      isValid: errors.length === 0,
      errors: errors
    };
  }
  
  toJSON() {
    return {
      id: this.id,
      district: this.district,
      rooms: this.rooms,
      squareMeters: this.squareMeters,
      buildingAge: this.buildingAge,
      price: this.price,
      type: this.type,
      location: {
        latitude: this.latitude,
        longitude: this.longitude
      }
    };
  }
}

module.exports = Property;