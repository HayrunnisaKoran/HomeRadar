// soa-api/services/googleMapsService.js
require('dotenv').config();
const axios = require('axios');

class GoogleMapsService {
    constructor() {
        this.apiKey = process.env.GOOGLE_MAPS_API_KEY;
        this.baseUrl = 'https://maps.googleapis.com/maps/api/geocode/json';
    }

    async getCoordinates(address) {
        try {
            console.log('📍 Google Maps API çağrılıyor:', address);
            const response = await axios.get(this.baseUrl, {
                params: {
                    address: address,
                    key: this.apiKey
                }
            });
            
            if (response.data.status === 'OK') {
                const location = response.data.results[0].geometry.location;
                return {
                    success: true,
                    latitude: location.lat,
                    longitude: location.lng,
                    formattedAddress: response.data.results[0].formatted_address
                };
            } else {
                return {
                    success: false,
                    error: response.data.status
                };
            }
        } catch (error) {
            console.error('Google Maps API Error:', error.message);
            return {
                success: false,
                error: error.message
            };
        }
    }

    async getDistrictCoordinates(district) {
        const address = `${district}, Manisa, Turkey`;
        return await this.getCoordinates(address);
    }

    async getAllDistrictsCoordinates() {
        const districts = ['Yunusemre', 'Şehzadeler', 'Akhisar', 'Turgutlu', 'Salihli'];
        const results = [];
        
        for (const district of districts) {
            const coords = await this.getDistrictCoordinates(district);
            results.push({
                district: district,
                ...coords
            });
            // API rate limit için bekle
            await new Promise(resolve => setTimeout(resolve, 100));
        }
        
        return results;
    }
}

module.exports = new GoogleMapsService();