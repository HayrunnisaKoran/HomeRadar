const { spawn } = require('child_process');
const path = require('path');

// BASIT LOGGER
const logger = {
  info: (msg) => console.log(`[INFO] ${new Date().toISOString()} - ${msg}`),
  error: (msg) => console.error(`[ERROR] ${new Date().toISOString()} - ${msg}`),
  debug: (msg) => console.debug(`[DEBUG] ${new Date().toISOString()} - ${msg}`),
  warn: (msg) => console.warn(`[WARN] ${new Date().toISOString()} - ${msg}`)
};


const PYTHON_SCRIPT_PATH = path.join(
    __dirname,      // infrastructure-layer/external-apis/services/
    '..',           // infrastructure-layer/external-apis/
    '..',           // infrastructure-layer/
    'ml-model',     // infrastructure-layer/ml-model/
    'predict.py'    // predict.py
);

class MLService {
   static async predict(inputData) {
    return new Promise((resolve, reject) => {
        try {
            const pythonScriptPath = PYTHON_SCRIPT_PATH;
            const pythonScriptDir = path.dirname(pythonScriptPath);
            
            logger.info(`ML Python scripti çağrılıyor: ${pythonScriptPath}`);
            logger.info(`Çalışma dizini: ${pythonScriptDir}`);
            
            // Python process'i BAŞLAT (ARGÜMAN OLMADAN)
            const pythonProcess = spawn('python', [pythonScriptPath], {
                cwd: pythonScriptDir,
                stdio: ['pipe', 'pipe', 'pipe']  // stdin, stdout, stderr
            });
            
            let stdoutData = '';
            let stderrData = '';
            
            // STDIN'e veri yaz (JSON string)
            const inputJson = JSON.stringify(inputData);
            logger.debug(`STDIN'e yazılıyor (${inputJson.length} karakter): ${inputJson.substring(0, 50)}...`);
            
            pythonProcess.stdin.write(inputJson);
            pythonProcess.stdin.end();
            
            // Çıktıları yakala
            pythonProcess.stdout.on('data', (data) => {
                const str = data.toString();
                stdoutData += str;
                logger.debug(`Python stdout: ${str.trim()}`);
            });
            
            pythonProcess.stderr.on('data', (data) => {
                const str = data.toString();
                stderrData += str;
                logger.error(`Python stderr: ${str.trim()}`);
            });
            
            pythonProcess.on('close', (code) => {
                logger.info(`Python process kapandı. Kod: ${code}`);
                logger.info(`Stderr (${stderrData.length} chars): ${stderrData.substring(0, 200)}...`);
                logger.info(`Stdout (${stdoutData.length} chars): ${stdoutData.substring(0, 200)}...`);
                
                if (code !== 0) {
                    logger.error(`Python hatası - Kod: ${code}`);
                    
                    const mockPrice = MLService.calculateMockPrice(inputData);
                    resolve({
                        status: 'mock',
                        price: mockPrice,
                        currency: 'TL',
                        details: { 
                            note: 'Python script failed',
                            exit_code: code,
                            stderr: stderrData,
                            stdout: stdoutData
                        }
                    });
                    return;
                }
                
                // Stdout boşsa bu da hata
                if (!stdoutData.trim()) {
                    logger.error('Python stdout boş!');
                    const mockPrice = MLService.calculateMockPrice(inputData);
                    resolve({
                        status: 'mock',
                        price: mockPrice,
                        currency: 'TL',
                        details: { 
                            note: 'Python script returned empty output',
                            stderr: stderrData
                        }
                    });
                    return;
                }
                
                try {
                    const result = JSON.parse(stdoutData);
                    logger.info(`✅ ML tahmini başarılı: ${result.price} TL`);
                    resolve(result);
                } catch (parseError) {
                    logger.error('Python çıktısı parse edilemedi:', stdoutData);
                    
                    const mockPrice = MLService.calculateMockPrice(inputData);
                    resolve({
                        status: 'mock',
                        price: mockPrice,
                        currency: 'TL',
                        details: { 
                            note: 'Python output parse failed',
                            raw_output: stdoutData,
                            stderr: stderrData
                        }
                    });
                }
            });
            
            pythonProcess.on('error', (error) => {
                logger.error('Python process başlatılamadı:', error.message);
                reject(new Error(`Python process error: ${error.message}`));
            });
            
        } catch (error) {
            logger.error('MLService predict hatası:', error.message);
            reject(error);
        }
    });
}

  // Mock fiyat hesaplama (fallback için)
  static calculateMockPrice(inputData) {
    const basePrice = 50000; // m² başına temel fiyat
    const squareMeters = inputData.square_meters || 100;
    const rooms = inputData.rooms || 2;
    const buildingAge = inputData.building_age || 10;
    
    // Basit hesaplama
    let price = basePrice * squareMeters;
    price *= (1 + (rooms - 2) * 0.1); // Her oda için %10 artış
    price *= (1 - buildingAge * 0.01); // Her yıl için %1 azalış
    price = Math.max(price, 200000); // Minimum fiyat
    
    return Math.round(price);
  }
}

module.exports = MLService;
