// debug-ml.js
const { spawn } = require('child_process');
const path = require('path');
const fs = require('fs');

const pythonScriptPath = path.join(__dirname, 'infrastructure-layer', 'ml-model', 'predict.py');

// Python scriptinin var olduğunu kontrol et
if (!fs.existsSync(pythonScriptPath)) {
    console.error('❌ Python scripti bulunamadı:', pythonScriptPath);
    process.exit(1);
}

console.log('=== ML ENTEGRASYON DEBUG ===');
console.log('📁 Python scripti:', pythonScriptPath);
console.log('✅ Python scripti mevcut:', fs.existsSync(pythonScriptPath));
console.log('');

// Test verisi - SENİN ÇALIŞAN VERİN
const testData = {
    district: "Yunusemre",
    square_meters: 120,
    rooms: 3,
    living_rooms: 1,
    building_age: 5,
    bathrooms: 2,
    floor: 3,
    heating: 1,
    elevator: 1,
    garage: 0,
    balcony: 1,
    furnished: 0,
    swap: 0,
    usage_status: "Owner",
    building_status: 1,
    title_deed: 1
};

console.log('📊 Test Verisi:');
console.log(JSON.stringify(testData, null, 2));
console.log('JSON string:', JSON.stringify(testData));
console.log('JSON uzunluğu:', JSON.stringify(testData).length);
console.log('');

// 1. TERMİNALDEN ÇALIŞTIĞIN KOMUTUN AYNISINI DENE
console.log('🔧 1. Terminal komutunu taklit et...');
const terminalCommand = `python "${pythonScriptPath}" "${JSON.stringify(testData).replace(/"/g, '\\"')}"`;
console.log('   Komut:', terminalCommand.substring(0, 100) + '...');

const pythonProcess1 = spawn('python', [pythonScriptPath, JSON.stringify(testData)]);

pythonProcess1.stdout.on('data', (data) => {
    console.log('   ✅ Stdout:', data.toString().trim());
});

pythonProcess1.stderr.on('data', (data) => {
    console.log('   ❌ Stderr:', data.toString().trim());
});

pythonProcess1.on('close', (code) => {
    console.log('   Exit code:', code);
    console.log('');
    
    // 2. JSON'ı DOSYAYA YAZIP PYTHON'A DOSYA YOLU VER
    console.log('🔧 2. JSON dosyası ile deneme...');
    const tempJsonFile = path.join(__dirname, 'temp_test.json');
    fs.writeFileSync(tempJsonFile, JSON.stringify(testData, null, 2));
    
    const pythonProcess2 = spawn('python', [pythonScriptPath, `@${tempJsonFile}`]);
    
    pythonProcess2.stdout.on('data', (data) => {
        console.log('   ✅ Stdout:', data.toString().trim());
    });
    
    pythonProcess2.stderr.on('data', (data) => {
        console.log('   ❌ Stderr:', data.toString().trim());
    });
    
    pythonProcess2.on('close', (code) => {
        console.log('   Exit code:', code);
        
        // Temizlik
        if (fs.existsSync(tempJsonFile)) {
            fs.unlinkSync(tempJsonFile);
        }
        
        console.log('');
        
        // 3. STDIN İLE GÖNDER (EN GÜVENLİ YÖNTEM)
        console.log('🔧 3. STDIN ile gönder...');
        const pythonProcess3 = spawn('python', [pythonScriptPath]);
        
        pythonProcess3.stdin.write(JSON.stringify(testData));
        pythonProcess3.stdin.end();
        
        pythonProcess3.stdout.on('data', (data) => {
            console.log('   ✅ Stdout:', data.toString().trim());
        });
        
        pythonProcess3.stderr.on('data', (data) => {
            console.log('   ❌ Stderr:', data.toString().trim());
        });
        
        pythonProcess3.on('close', (code) => {
            console.log('   Exit code:', code);
            console.log('\n=== DEBUG SONU ===');
            console.log('\n🎯 ÖNERİLER:');
            
            if (code === 0) {
                console.log('✅ STDIN yöntemi çalışıyor! mlService.js\'yi STDIN\'e göre güncelle.');
            } else {
                console.log('❌ Tüm yöntemler başarısız. Python scriptini debug etmeliyiz.');
            }
        });
    });
});

// Hata yakalama
process.on('uncaughtException', (err) => {
    console.error('❌ Beklenmeyen hata:', err);
});