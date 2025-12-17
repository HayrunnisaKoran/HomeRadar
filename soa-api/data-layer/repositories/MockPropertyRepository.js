// 📄 data-layer/repositories/MockPropertyRepository.js
const Property = require('../../domain-layer/models/Property');

class MockPropertyRepository {
  constructor() {
    this.properties = [
      new Property({
        id: 1,
        district: 'Yunusemre',
        rooms: '3+1',
        squareMeters: 120,
        buildingAge: 5,
        price: 18500,
        type: 'Daire',
        latitude: 38.6131,
        longitude: 27.4262
      }),
      new Property({
        id: 2,
        district: 'Şehzadeler',
        rooms: '2+1',
        squareMeters: 90,
        buildingAge: 8,
        price: 16500,
        type: 'Daire',
        latitude: 38.6191,
        longitude: 27.4289
      }),
      new Property({
        id: 3,
        district: 'Akhisar',
        rooms: '3+1',
        squareMeters: 200,
        buildingAge: 3,
        price: 14000,
        type: 'Villa',
        latitude: 38.9180,
        longitude: 27.8400
      })
    ];
  }
  
  async findAll() {
    return this.properties;
  }
  
  async findById(id) {
    return this.properties.find(p => p.id === id);
  }
  
  async findByDistrict(district) {
    return this.properties.filter(p => 
      p.district.toLowerCase() === district.toLowerCase()
    );
  }
  
  async create(propertyData) {
    const newId = Math.max(...this.properties.map(p => p.id)) + 1;
    const property = new Property({
      id: newId,
      ...propertyData
    });
    
    this.properties.push(property);
    return property;
  }
}

module.exports = new MockPropertyRepository();